using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.Models;
using RestaurantFlow.Shared.Enums;
using RestaurantFlow.Server.DTOs;

namespace RestaurantFlow.Server.ViewModels;

public partial class CounterViewModel : ReactiveObject
{
    private readonly IOrderService _orderService;
    private readonly IAudioNotificationService _audioService;
    private readonly ISignalRConnectionService _signalRService;
    private int _previousReadyOrdersCount = 0;
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _readyOrders = new();
    
    [Reactive]
    private ObservableCollection<OrderCardViewModel> _completedOrders = new();
    
    [Reactive]
    private bool _isLoading = false;

    public CounterViewModel(IOrderService orderService, IAudioNotificationService audioService, ISignalRConnectionService signalRService)
    {
        _orderService = orderService;
        _audioService = audioService;
        _signalRService = signalRService;
        
        // Підписуємося на зміни статусу замовлень
        _signalRService.OrderStatusChanged += OnOrderStatusChanged;
        
        // Load orders when ViewModel is created
        _ = LoadOrdersAsync();
    }
    
    private async void OnOrderStatusChanged(OrderStatusUpdate update)
    {
        // Якщо замовлення стало готовим до видачі
        if (update.Status == OrderStatus.Ready)
        {
            // Відтворюємо звуковий сигнал
            _ = Task.Run(async () => await _audioService.PlayOrderReadyNotification());
        }
        
        // Оновлюємо список замовлень в UI потоці
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await LoadOrdersAsync();
        });
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
            
            // Перевіряємо чи є нові готові замовлення
            if (ReadyOrders.Count > _previousReadyOrdersCount && _previousReadyOrdersCount > 0)
            {
                // Відтворюємо звуковий сигнал про нові готові замовлення
                _ = Task.Run(async () => await _audioService.PlayOrderReadyNotification());
            }
            
            _previousReadyOrdersCount = ReadyOrders.Count;
            
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