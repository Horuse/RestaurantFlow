using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reactive;
using System;

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

        CurrentView = KitchenViewModel;
        SelectedView = "Kitchen";
        
        this.WhenAnyValue(x => x.SelectedView)
            .Subscribe(async viewName =>
            {
                await RefreshCurrentViewData(viewName);
            });
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
    
    private async System.Threading.Tasks.Task RefreshCurrentViewData(string viewName)
    {
        try
        {
            switch (viewName)
            {
                case "Kitchen":
                    await KitchenViewModel.LoadOrdersAsync();
                    break;
                case "Counter":
                    await CounterViewModel.LoadOrdersAsync();
                    break;
                case "Menu":
                    await MenuViewModel.LoadDataAsync();
                    break;
                case "Inventory":
                    await InventoryViewModel.LoadDataAsync();
                    break;
                case "Analytics":
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing view data: {ex.Message}");
        }
    }

}