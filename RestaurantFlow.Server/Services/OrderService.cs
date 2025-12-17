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
    private readonly IInventoryService _inventoryService;
    private readonly IMenuService _menuService;
    
    public OrderService(IOrderRepository orderRepository, IInventoryService inventoryService, IMenuService menuService)
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
        _menuService = menuService;
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
        // Спочатку створюємо замовлення
        var createdOrder = await _orderRepository.AddAsync(order);
        
        // Потім зменшуємо запаси інгредієнтів для кожного айтему
        foreach (var orderItem in createdOrder.OrderItems)
        {
            await DeductIngredientStockAsync(orderItem.MenuItemId, orderItem.Quantity, createdOrder.OrderNumber);
        }
        
        return createdOrder;
    }
    
    private async Task DeductIngredientStockAsync(int menuItemId, int quantity, string orderNumber)
    {
        try
        {
            // Отримуємо всі інгредієнти для цієї страви
            var menuItemIngredients = await _menuService.GetMenuItemIngredientsAsync(menuItemId);
            
            foreach (var mii in menuItemIngredients)
            {
                // Розраховуємо скільки інгредієнта потрібно для кількості порцій
                var requiredQuantity = mii.Quantity * quantity;
                
                // Зменшуємо запас інгредієнта
                await _inventoryService.UpdateIngredientStockAsync(
                    mii.IngredientId, 
                    -requiredQuantity, 
                    $"Використано для замовлення #{orderNumber}: {quantity}x {mii.MenuItem?.Name ?? "Страва"}"
                );
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Помилка при зменшенні запасів для MenuItemId {menuItemId}: {ex.Message}");
        }
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