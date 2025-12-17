using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.Repositories;
using RestaurantFlow.Server.Hubs;

namespace RestaurantFlow.Server.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IRestaurantNotificationService? _restaurantNotificationService;
    private readonly INotificationService? _notificationService;
    
    public InventoryService(IInventoryRepository inventoryRepository, IRestaurantNotificationService? restaurantNotificationService = null, INotificationService? notificationService = null)
    {
        _inventoryRepository = inventoryRepository;
        _restaurantNotificationService = restaurantNotificationService;
        _notificationService = notificationService;
    }
    
    public async Task<List<Ingredient>> GetIngredientsAsync()
    {
        return await _inventoryRepository.GetActiveIngredientsAsync();
    }
    
    public async Task<Ingredient?> GetIngredientByIdAsync(int id)
    {
        return await _inventoryRepository.GetByIdAsync(id);
    }
    
    public async Task<List<Ingredient>> GetLowStockIngredientsAsync()
    {
        return await _inventoryRepository.GetLowStockIngredientsAsync();
    }
    
    public async Task<Ingredient> UpdateIngredientStockAsync(int ingredientId, decimal quantity, string reason)
    {
        var result = await _inventoryRepository.UpdateIngredientStockAsync(ingredientId, quantity, reason);
        
        if (_restaurantNotificationService != null)
            await _restaurantNotificationService.NotifyMenuUpdated();
        else if (_notificationService != null)
            await _notificationService.NotifyMenuUpdated();
        
        return result;
    }
    
    public async Task<List<InventoryLog>> GetInventoryLogsAsync(int? ingredientId = null)
    {
        return await _inventoryRepository.GetInventoryLogsAsync(ingredientId);
    }
    
    public async Task<Ingredient> CreateIngredientAsync(Ingredient ingredient)
    {
        return await _inventoryRepository.AddAsync(ingredient);
    }
    
    public async Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient)
    {
        var result = await _inventoryRepository.UpdateAsync(ingredient);
        
        if (_restaurantNotificationService != null)
            await _restaurantNotificationService.NotifyMenuUpdated();
        else if (_notificationService != null)
            await _notificationService.NotifyMenuUpdated();
            
        return result;
    }
    
    public async Task DeleteIngredientAsync(int id)
    {
        await _inventoryRepository.SoftDeleteAsync(id);
        
        if (_restaurantNotificationService != null)
            await _restaurantNotificationService.NotifyMenuUpdated();
        else if (_notificationService != null)
            await _notificationService.NotifyMenuUpdated();
    }
}