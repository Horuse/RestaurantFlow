using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantFlow.Server.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetActiveOrdersAsync();
    Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Order>> GetRecentCompletedOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order> CreateOrderAsync(Order order);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task UpdateOrderItemStatusAsync(int orderItemId, OrderStatus status);
}