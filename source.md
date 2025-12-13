Supported Attributes
[Reactive]
Marks properties as reactive, generating getter and setter code.

[ObservableAsProperty]
Generates read-only properties backed by an ObservableAsPropertyHelper based on an IObservable.

[ReactiveCommand]
Generates commands, with options to add attributes or enable CanExecute functionality.

[IViewFor]
Links a view to a view model for data binding. Supports generic types and Splat registration.

[RoutedControlHost] and [ViewModelControlHost]
Platform-specific attributes for control hosting in WinForms applications.

[BindableDerivedList]
Generates a derived list from a ReadOnlyObservableCollection backing field.

[ReactiveCollection]
Generates property changed notifications on add, remove, and new actions on an ObservableCollection backing field.

Historical Approach
Read-Write Properties
Previously, properties were declared like this:

private string _name;
public string Name
{
get => _name;
set => this.RaiseAndSetIfChanged(ref _name, value);
}
Before these Source Generators were available we used ReactiveUI.Fody. With ReactiveUI.Fody the [Reactive] Attribute was placed on a Public Property with Auto get / set properties, the generated code from the Source Generator and the Injected code using Fody are very similar with the exception of the Attributes.

[Reactive]
public string Name { get; set; }
ObservableAsPropertyHelper properties
Similarly, to declare output properties, the code looks like this:

public partial class MyReactiveClass : ReactiveObject
{
ObservableAsPropertyHelper<string> _firstName;

    public MyReactiveClass()
    {
        _firstName = firstNameObservable
            .ToProperty(this, x => x.FirstName);
    }

    public string FirstName => _firstName.Value;

    private IObservable<string> firstNameObservable() => Observable.Return("Test");
}
With ReactiveUI.Fody, you can simply declare a read-only property using the [ObservableAsProperty] attribute, using either option of the two options shown below.

[ObservableAsProperty]
public string FirstName { get; }
Welcome to a new way - Source Generators
Usage Reactive property [Reactive]
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
[Reactive]
private string _myProperty;
}
Usage Reactive property with set Access Modifier
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
[Reactive(SetModifier = AccessModifier.Protected)]
private string _myProperty;
}
Usage Reactive property with property Attribute pass through
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
[Reactive]
[property: JsonIgnore]
private string _myProperty;
}
Usage Reactive property from partial property
Partial properties are supported in C# 13 and Visual Studio 17.12.0 and later. Both the getter and setter must be empty, and the [Reactive] attribute must be placed on the property. Override and Virtual properties are supported. Set Access Modifier is also supported on partial properties.

using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
[Reactive]
public partial string MyProperty { get; set; }
}
Usage Reactive property from partial property with default value
Partial properties with initial value are supported in C# preview and Visual Studio 17.12.0 and later. Both the getter and setter must be empty, and the [Reactive] attribute must be placed on the property. Override and Virtual properties are supported. Set Access Modifier is also supported on partial properties.

using ReactiveUI.SourceGenerators;
public partial class MyReactiveClass : ReactiveObject
{
[Reactive]
public partial string MyProperty { get; set; } = "Default Value"
}
Usage Reactive property with other source generators
Roslyn source generators don’t have a defined run order and each generator sees the same initial compilation. Code/attributes that one generator emits aren’t visible to other generators in the same compilation round, so adding [JsonPropertyName]/[JsonInclude] from ReactiveUI.SourceGenerators won’t cause the System.Text.Json source generator to pick them up in that project. That’s by design of the generator pipeline (no inter-generator dependencies / ordering).

So System.Text.Json needs special care. In the case that you want to Json-serialize [Reactive] properties, and you want to use the System.Text.Json source generator they must run in different assemblies. The same applies to other source generators depending on the output of ReactiveUI.SourceGenerators.

Define types with [Reactive] properties in assembly A, and then define the System.Text.Json.JsonSerializerContext source generation context in assembly B, and let B reference A.

Usage ObservableAsPropertyHelper [ObservableAsProperty]
ObservableAsPropertyHelper is used to create a read-only property from an IObservable. The generated code will create a backing field and a property that returns the value of the backing field. The backing field is initialized with the value of the IObservable when the class is instantiated.

A private field is created with the name of the property prefixed with an underscore. The field is initialized with the value of the IObservable when the class is instantiated. The property is created with the same name as the field without the underscore. The property returns the value of the field until initialized, then it returns the value of the IObservable.

You can define the name of the property by using the PropertyName parameter. If you do not define the PropertyName, the property name will be the same as the field name without the underscore.

Usage ObservableAsPropertyHelper with Field
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
[ObservableAsProperty]
private string _myProperty = "Default Value";

    public MyReactiveClass()
    {
        _myPrpertyHelper = MyPropertyObservable()
            .ToProperty(this, x => x.MyProperty);
    }

    IObservable<string> MyPropertyObservable() => Observable.Return("Test Value");
}
Usage ObservableAsPropertyHelper with Field and non readonly nullable OAPH field
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject, IActivatableViewModel
{
[ObservableAsProperty(ReadOnly = false)]
private string _myProperty = "Default Value";

    public MyReactiveClass()
    {
        this.WhenActivated(disposables =>
        {
            _myPrpertyHelper = MyPropertyObservable()
                .ToProperty(this, x => x.MyProperty)
                .DisposeWith(disposables);
        });
    }

    IObservable<string> MyPropertyObservable() => Observable.Return("Test Value");
}
Usage ObservableAsPropertyHelper with Observable Property and specific PropertyName
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{    
[ObservableAsProperty(ReadOnly = false)]
private string _myProperty = "Default Value";

    public MyReactiveClass()
    {
        this.WhenActivated(disposables =>
        {
            _myPrpertyHelper = MyPropertyObservable()
                .ToProperty(this, x => x.MyProperty)
                .DisposeWith(disposables);
        });
    }

    IObservable<string> MyPropertyObservable() => Observable.Return("Test Value");
}
Usage ObservableAsPropertyHelper with Observable Property and protected OAPH field
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
[ObservableAsProperty(UseProtected = true)]
private string _myProperty = "Default Value";

    public MyReactiveClass()
    {
        _myPrpertyHelper = MyPropertyObservable()
            .ToProperty(this, x => x.MyProperty);
    }

    IObservable<string> MyPropertyObservable() => Observable.Return("Test Value");
}
Usage ObservableAsPropertyHelper with Observable Method
NOTE: This does not currently support methods with parameters

using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{    
public MyReactiveClass()
{
// Initialize generated _myObservablePropertyHelper
// for the generated MyObservableProperty
InitializeOAPH();
}

    [ObservableAsProperty]
    IObservable<string> MyObservable() => Observable.Return("Test Value");
}
Usage ObservableAsPropertyHelper with Observable Method and specific PropertyName
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{    
public MyReactiveClass()
{
// Initialize generated _testValuePropertyHelper
// for the generated TestValueProperty
InitializeOAPH();
}

    [ObservableAsProperty(PropertyName = TestValueProperty)]
    IObservable<string> MyObservable() => Observable.Return("Test Value");
}
Usage ObservableAsPropertyHelper with Observable Method and protected OAPH field
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{    
public MyReactiveClass()
{
// Initialize generated _myObservablePropertyHelper
// for the generated MyObservableProperty
InitializeOAPH();
}

    [ObservableAsProperty(UseProtected = true)]
    IObservable<string> MyObservable() => Observable.Return("Test Value");
}
Usage ObservableAsPropertyHelper with partial Property and a default value
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
public MyReactiveClass()
{
// The value of MyProperty will be "Default Value" until the Observable is initialized
_myPrpertyHelper = MyPropertyObservable()
.ToProperty(this, nameof(MyProperty));
}

    [ObservableAsProperty(InitialValue = "Default Value")]
    public partial string MyProperty { get; }

    public IObservable<string> MyPropertyObservable() => Observable.Return("Test Value");
}
Usage ReactiveCommand [ReactiveCommand]
Usage ReactiveCommand without parameter
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand]
private void Execute() { }
}
Usage ReactiveCommand with parameter
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand]
private void Execute(string parameter) { }
}
Usage ReactiveCommand with parameter and return value
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand]
private string Execute(string parameter) => parameter;
}
Usage ReactiveCommand with parameter and async return value.
Note: the Async suffix is removed from the generated command

using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand]
private async Task<string> ExecuteAsync(string parameter) => await Task.FromResult(parameter);

    // Generates the following code ExecuteCommand, Note the Async suffix is removed
}
Usage ReactiveCommand with IObservable return value
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand]
private IObservable<string> Execute(string parameter) => Observable.Return(parameter);
}
Usage ReactiveCommand with CancellationToken
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand]
private async Task Execute(CancellationToken token) => await Task.Delay(1000, token);
}
Usage ReactiveCommand with CancellationToken and parameter
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand]
private async Task<string> Execute(string parameter, CancellationToken token)
{
await Task.Delay(1000, token);
return parameter;
}
}
Usage ReactiveCommand with CanExecute
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
private IObservable<bool> _canExecute;

    [Reactive]
    private string _myProperty1;

    [Reactive]
    private string _myProperty2;

    public MyReactiveClass()
    {
        _canExecute = this.WhenAnyValue(x => x.MyProperty1, x => x.MyProperty2, (x, y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y));
    }

    [ReactiveCommand(CanExecute = nameof(_canExecute))]
    private void Search() { }
}
Usage ReactiveCommand with property Attribute pass through
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
private IObservable<bool> _canExecute;

    [Reactive]
    private string _myProperty1;

    [Reactive]
    private string _myProperty2;

    public MyReactiveClass()
    {
        _canExecute = this.WhenAnyValue(x => x.MyProperty1, x => x.MyProperty2, (x, y) => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y));
    }

    [ReactiveCommand(CanExecute = nameof(_canExecute))]
    [property: JsonIgnore]
    private void Search() { }
}
Usage ReactiveCommand with ReactiveUI OutputScheduler
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand(OutputScheduler = "RxApp.MainThreadScheduler")]
private void Execute() { }
}
Usage ReactiveCommand with custom OutputScheduler
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
private IScheduler _customScheduler = new TestScheduler();

    [ReactiveCommand(OutputScheduler = nameof(_customScheduler))]
    private void Execute() { }
}
Usage ReactiveCommand with AccessModifier
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[ReactiveCommand(AccessModifier = PropertyAccessModifier.Internal)]
private void Execute() { }
}
Usage IViewFor [IViewFor(nameof(ViewModelName))]
IViewFor usage
IViewFor is used to link a View to a ViewModel, this is used to link the ViewModel to the View in a way that ReactiveUI can use it to bind the ViewModel to the View. The ViewModel is passed as a type to the IViewFor Attribute using generics. The class must inherit from a UI Control from any of the following platforms and namespaces:

Maui (Microsoft.Maui)
WinUI (Microsoft.UI.Xaml)
WPF (System.Windows or System.Windows.Controls)
WinForms (System.Windows.Forms)
Avalonia (Avalonia)
Uno (Windows.UI.Xaml).
IViewFor with Splat Registration Type
Choose from the following Splat Registration Types, option for IViewFor Registration and / or ViewModel Registration:

SplatRegistrationType.PerRequest
SplatRegistrationType.LazySingleton
SplatRegistrationType.Constant
SplatRegistrationType.None (Default if not specified - no registration is performed)
using ReactiveUI.SourceGenerators;
using Splat;
[IViewFor<MyReactiveClass>(RegistrationType = SplatRegistrationType.PerRequest, ViewModelRegistrationType = SplatRegistrationType.LazySingleton)]
public partial class MyReactiveControl : UserControl
{
public MyReactiveControl()
{
InitializeComponent();
ViewModel = AppLocator.Current.GetService<MyReactiveClass>();
}
}
this will generate the following code to enable you register the marked Views as IViewFor<ViewModel> with Splat:

using ReactiveUI.SourceGenerators;

Splat.AppLocator.CurrentMutable.RegisterViewsForViewModelsSourceGenerated();
Usage IViewFor with ViewModel Name - Generic Types should be used with the fully qualified name, otherwise use nameof(ViewModelTypeName)
using ReactiveUI.SourceGenerators;

[IViewFor("MyReactiveGenericClass<int>")]
public partial class MyReactiveControl : UserControl
{
public MyReactiveControl()
{
InitializeComponent();
ViewModel = new MyReactiveClass();
}
}
Usage IViewFor with ViewModel Type
using ReactiveUI.SourceGenerators;

[IViewFor<MyReactiveClass>]
public partial class MyReactiveControl : UserControl
{
public MyReactiveControl()
{
InitializeComponent();
ViewModel = new MyReactiveClass();
}
}
Usage ReadOnlyObservableCollection
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass
{
[BindableDerivedList]
private readonly ReadOnlyObservableCollection<string> _myList;
}
Platform specific Attributes
WinForms
RoutedControlHost
using ReactiveUI.SourceGenerators.WinForms;

[RoutedControlHost("YourNameSpace.CustomControl")]
public partial class MyCustomRoutedControlHost;
ViewModelControlHost
using ReactiveUI.SourceGenerators.WinForms;

[ViewModelControlHost("YourNameSpace.CustomControl")]
public partial class MyCustomViewModelControlHost;
ReactiveCollection
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

public partial class MyReactiveClass : ReactiveObject
{
[ReactiveCollection]
private ObservableCollection<string> _myCollection;

    public MyReactiveClass()
    {
        MyCollection = new ObservableCollection<string>();
        _myCollection.Add("Item 1");
    }
}