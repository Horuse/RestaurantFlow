using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;
using RestaurantFlow.Server.Repositories;

namespace RestaurantFlow.Server.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    
    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllOrdersWithItemsAsync();
    }
    
    public async Task<List<Order>> GetActiveOrdersAsync()
    {
        return await _orderRepository.GetActiveOrdersAsync();
    }
    
    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _orderRepository.GetOrdersByStatusAsync(status);
    }
    
    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate);
    }
    
    public async Task<List<Order>> GetRecentCompletedOrdersAsync()
    {
        return await _orderRepository.GetRecentCompletedOrdersAsync();
    }
    
    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _orderRepository.GetOrderWithItemsAsync(id);
    }
    
    public async Task<Order> CreateOrderAsync(Order order)
    {
        return await _orderRepository.AddAsync(order);
    }
    
    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        await _orderRepository.UpdateOrderStatusAsync(orderId, status);
    }
    
    public async Task UpdateOrderItemStatusAsync(int orderItemId, OrderStatus status)
    {
        await _orderRepository.UpdateOrderItemStatusAsync(orderItemId, status);
    }
}