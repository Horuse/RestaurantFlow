using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Models;

public partial class OrderCardViewModel : ReactiveObject
{
    private readonly Order _order;
    private readonly IOrderService _orderService;
    
    public OrderCardViewModel(Order order, IOrderService orderService)
    {
        _order = order;
        _orderService = orderService;
    }
    
    public int Id => _order.Id;
    public string OrderNumber => _order.OrderNumber;
    
    public string TableInfo => _order.OrderType == OrderType.DineIn 
        ? $"Столик #{_order.TableNumber}" 
        : "З собою";
    
    public string ItemsDescription => string.Join(", ", 
        _order.OrderItems.Select(oi => $"{oi.MenuItem.Name} x{oi.Quantity}"));
    
    public string CreatedTime => $"Замовлено: {_order.CreatedAt:HH:mm}";
    
    public string CookingTime
    {
        get
        {
            var elapsed = DateTime.UtcNow - _order.CreatedAt;
            return $"Готується: {elapsed.Minutes} хв";
        }
    }
    
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
    }
    
    [ReactiveCommand]
    private async Task MarkReady()
    {
        await _orderService.UpdateOrderStatusAsync(_order.Id, OrderStatus.Ready);
    }
    
    [ReactiveCommand]
    private async Task MarkCompleted()
    {
        await _orderService.UpdateOrderStatusAsync(_order.Id, OrderStatus.Completed);
    }
}