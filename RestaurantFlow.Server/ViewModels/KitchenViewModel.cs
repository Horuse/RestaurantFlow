using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.Models;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.ViewModels;

public partial class KitchenViewModel : ReactiveObject
{
    private readonly IOrderService _orderService;
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _pendingOrders = new();
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _inProgressOrders = new();
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _readyOrders = new();
    
    [Reactive]
    private bool _isLoading = false;

    public KitchenViewModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task LoadOrdersAsync()
    {
        IsLoading = true;
        try
        {
            var orders = await _orderService.GetActiveOrdersAsync();
            
            PendingOrders.Clear();
            InProgressOrders.Clear();
            ReadyOrders.Clear();
            
            foreach (var order in orders)
            {
                var orderCard = new OrderCardViewModel(order, _orderService);
                
                switch (order.Status)
                {
                    case OrderStatus.Pending:
                        PendingOrders.Add(orderCard);
                        break;
                    case OrderStatus.InProgress:
                        InProgressOrders.Add(orderCard);
                        break;
                    case OrderStatus.Ready:
                        ReadyOrders.Add(orderCard);
                        break;
                }
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
    private async Task StartOrderAsync(OrderCardViewModel orderCard)
    {
        await _orderService.UpdateOrderStatusAsync(orderCard.Id, OrderStatus.InProgress);
        await LoadOrdersAsync();
    }
    
    [ReactiveCommand]
    private async Task CompleteOrderAsync(OrderCardViewModel orderCard)
    {
        await _orderService.UpdateOrderStatusAsync(orderCard.Id, OrderStatus.Ready);
        await LoadOrdersAsync();
    }
}