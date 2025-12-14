using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetAllOrdersWithItemsAsync();
    Task<List<Order>> GetActiveOrdersAsync();
    Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Order>> GetRecentCompletedOrdersAsync();
    Task<Order?> GetOrderWithItemsAsync(int id);
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task UpdateOrderItemStatusAsync(int orderItemId, OrderStatus status);
    Task<List<OrderItem>> GetOrderItemsByStatusAsync(OrderStatus status);
}