using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.Models;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.ViewModels;

public partial class CounterViewModel : ReactiveObject
{
    private readonly IOrderService _orderService;
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _readyOrders = new();
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _completedOrders = new();
    
    [Reactive]
    private bool _isLoading = false;

    public CounterViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        
        // Load orders when ViewModel is created
        _ = LoadOrdersAsync();
    }

    public async Task LoadOrdersAsync()
    {
        IsLoading = true;
        try
        {
            var readyOrders = await _orderService.GetOrdersByStatusAsync(OrderStatus.Ready);
            var completedOrders = await _orderService.GetRecentCompletedOrdersAsync();
            
            ReadyOrders.Clear();
            CompletedOrders.Clear();
            
            foreach (var order in readyOrders)
            {
                ReadyOrders.Add(new OrderCardViewModel(order, _orderService));
            }
            
            foreach (var order in completedOrders)
            {
                CompletedOrders.Add(new OrderCardViewModel(order, _orderService));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [ReactiveCommand]
    private async Task RefreshOrdersAsync()
    {
        await LoadOrdersAsync();
    }
    
    [ReactiveCommand]
    private async Task CompleteOrder(OrderCardViewModel orderCard)
    {
        await _orderService.UpdateOrderStatusAsync(orderCard.Id, OrderStatus.Completed);
        await LoadOrdersAsync();
    }
    
    [ReactiveCommand]
    private async Task CallCustomer(OrderCardViewModel orderCard)
    {
    }
}