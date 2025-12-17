using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Reactive.Linq;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.Models;
using RestaurantFlow.Server.DTOs;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.ViewModels;

public partial class KitchenViewModel : ReactiveObject, IDisposable
{
    private readonly IOrderService _orderService;
    private readonly ISignalRConnectionService _signalRService;
    private readonly IDisposable? _reconnectTimer;
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _pendingOrders = new();
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _inProgressOrders = new();
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _readyOrders = new();
    
    [Reactive]
    private bool _isLoading = false;

    [Reactive]
    private bool _isSignalRConnected = false;

    public KitchenViewModel(IOrderService orderService, ISignalRConnectionService signalRService)
    {
        _orderService = orderService;
        _signalRService = signalRService;
        
        _signalRService.NewOrderReceived += OnNewOrderReceived;
        _signalRService.OrderStatusChanged += OnOrderStatusChanged;
        
        // Перевіряємо підключення кожні 30 секунд
        _reconnectTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(30))
            .Subscribe(async _ => await EnsureSignalRConnectionAsync());
        
        _ = LoadOrdersAsync();
        _ = ConnectToSignalRAsync();
    }
    
    private async Task EnsureSignalRConnectionAsync()
    {
        if (!_signalRService.IsConnected)
        {
            await ConnectToSignalRAsync();
        }
        IsSignalRConnected = _signalRService.IsConnected;
    }

    private async Task ConnectToSignalRAsync()
    {
        try
        {
            await _signalRService.StartConnectionAsync();
            IsSignalRConnected = _signalRService.IsConnected;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Failed to connect to SignalR: {ex.Message}");
            IsSignalRConnected = false;
        }
    }

    private async void OnNewOrderReceived(OrderResponse order)
    {
        System.Console.WriteLine($"Kitchen received new order: {order.OrderNumber}");
        
        var orderEntity = new RestaurantFlow.Data.Entities.Order
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            TableNumber = order.TableNumber,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            CompletedAt = order.CompletedAt,
            OrderItems = order.Items.Select(item => new RestaurantFlow.Data.Entities.OrderItem
            {
                Id = item.Id,
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                Price = item.Price,
                Status = item.Status,
                SpecialRequests = item.SpecialInstructions ?? string.Empty,
                StartedCookingAt = item.StartedCookingAt,
                ReadyAt = item.ReadyAt,
                MenuItem = new RestaurantFlow.Data.Entities.MenuItem { Name = item.MenuItemName }
            }).ToList()
        };

        var orderCard = new OrderCardViewModel(orderEntity, _orderService, LoadOrdersAsync);
        
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (order.Status == OrderStatus.Pending)
            {
                PendingOrders.Add(orderCard);
            }
        });
    }

    private async void OnOrderStatusChanged(OrderStatusUpdate update)
    {
        System.Console.WriteLine($"Kitchen received order status update: {update.OrderId} -> {update.Status}");
        
        await LoadOrdersAsync();
    }

    public async Task LoadOrdersAsync()
    {
        IsLoading = true;
        try
        {
            var orders = await _orderService.GetActiveOrdersAsync();
            
            foreach (var order in PendingOrders.Concat(InProgressOrders).Concat(ReadyOrders))
            {
                order.Dispose();
            }
            
            PendingOrders.Clear();
            InProgressOrders.Clear();
            ReadyOrders.Clear();
            
            foreach (var order in orders)
            {
                var orderCard = new OrderCardViewModel(order, _orderService, LoadOrdersAsync);
                
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

    public void Dispose()
    {
        _signalRService.NewOrderReceived -= OnNewOrderReceived;
        _signalRService.OrderStatusChanged -= OnOrderStatusChanged;
        _reconnectTimer?.Dispose();
        
        foreach (var order in PendingOrders.Concat(InProgressOrders).Concat(ReadyOrders))
        {
            order.Dispose();
        }
    }
}