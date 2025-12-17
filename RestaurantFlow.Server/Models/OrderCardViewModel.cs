using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Models;

public partial class OrderCardViewModel : ReactiveObject, IDisposable
{
    private readonly Order _order;
    private readonly IOrderService _orderService;
    private readonly Func<Task>? _onStatusChanged;
    private readonly IDisposable? _timerSubscription;
    
    [Reactive]
    private string _cookingTime = "";
    
    public OrderCardViewModel(Order order, IOrderService orderService, Func<Task>? onStatusChanged = null)
    {
        _order = order;
        _orderService = orderService;
        _onStatusChanged = onStatusChanged;
        
        // Оновлюємо час кожну хвилину
        _timerSubscription = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(1))
            .Subscribe(_ => UpdateCookingTime());
        
        UpdateCookingTime();
    }
    
    private void UpdateCookingTime()
    {
        if (_order.Status == OrderStatus.InProgress)
        {
            var startTime = _order.OrderItems.FirstOrDefault()?.StartedCookingAt ?? _order.CreatedAt;
            var elapsed = DateTime.UtcNow - startTime;
            CookingTime = $"Готується: {elapsed.Minutes} хв";
        }
    }
    
    public int Id => _order.Id;
    public string OrderNumber => _order.OrderNumber;
    
    public string TableInfo => _order.OrderType == OrderType.DineIn 
        ? $"Столик #{_order.TableNumber}" 
        : "З собою";
    
    public string ItemsDescription => string.Join(", ", 
        _order.OrderItems.Select(oi => $"{oi.MenuItem.Name} x{oi.Quantity}"));
    
    public string CreatedTime => $"Замовлено: {_order.CreatedAt:HH:mm}";
    
    public string ReadyTime => _order.OrderItems.FirstOrDefault()?.ReadyAt != null
        ? $"Готово: {_order.OrderItems.First().ReadyAt:HH:mm}"
        : "";
        
    public string StatusColor => _order.Status switch
    {
        OrderStatus.Pending => "#6c757d",
        OrderStatus.InProgress => "#ffc107", 
        OrderStatus.Ready => "#28a745",
        OrderStatus.Completed => "#007bff",
        OrderStatus.Cancelled => "#dc3545",
        _ => "#6c757d"
    };
    
    [ReactiveCommand]
    private async Task StartCooking()
    {
        await _orderService.UpdateOrderStatusAsync(_order.Id, OrderStatus.InProgress);
        if (_onStatusChanged != null)
        {
            await _onStatusChanged();
        }
    }
    
    [ReactiveCommand]
    private async Task MarkReady()
    {
        await _orderService.UpdateOrderStatusAsync(_order.Id, OrderStatus.Ready);
        if (_onStatusChanged != null)
        {
            await _onStatusChanged();
        }
    }
    
    [ReactiveCommand]
    private async Task MarkCompleted()
    {
        await _orderService.UpdateOrderStatusAsync(_order.Id, OrderStatus.Completed);
        if (_onStatusChanged != null)
        {
            await _onStatusChanged();
        }
    }
    
    public void Dispose()
    {
        _timerSubscription?.Dispose();
    }
}