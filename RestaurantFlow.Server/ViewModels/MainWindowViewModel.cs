using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;

namespace RestaurantFlow.Server.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    [Reactive]
    private ReactiveObject? _currentView;
    
    [Reactive]
    private string _selectedView = "Kitchen";

    public KitchenViewModel KitchenViewModel { get; }
    public CounterViewModel CounterViewModel { get; }
    public MenuViewModel MenuViewModel { get; }
    public InventoryViewModel InventoryViewModel { get; }
    public AnalyticsViewModel AnalyticsViewModel { get; }
    
    public ReactiveCommand<string, Unit> NavigateToCommand { get; }
    
    public object? DialogManager => null;
    public object? ToastManager => null;

    public MainWindowViewModel(
        KitchenViewModel kitchenViewModel,
        CounterViewModel counterViewModel,
        MenuViewModel menuViewModel,
        InventoryViewModel inventoryViewModel,
        AnalyticsViewModel analyticsViewModel)
    {
        KitchenViewModel = kitchenViewModel;
        CounterViewModel = counterViewModel;
        MenuViewModel = menuViewModel;
        InventoryViewModel = inventoryViewModel;
        AnalyticsViewModel = analyticsViewModel;

        NavigateToCommand = ReactiveCommand.Create<string>(NavigateTo);

        // Set initial view
        CurrentView = KitchenViewModel;
        SelectedView = "Kitchen";
    }
    
    private void NavigateTo(string viewName)
    {
        SelectedView = viewName;
        CurrentView = viewName switch
        {
            "Kitchen" => KitchenViewModel,
            "Counter" => CounterViewModel,
            "Menu" => MenuViewModel,
            "Inventory" => InventoryViewModel,
            "Analytics" => AnalyticsViewModel,
            _ => KitchenViewModel
        };
    }

}