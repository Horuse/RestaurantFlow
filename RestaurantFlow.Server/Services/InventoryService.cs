using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.Repositories;

namespace RestaurantFlow.Server.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    
    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
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
        return await _inventoryRepository.UpdateIngredientStockAsync(ingredientId, quantity, reason);
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
        return await _inventoryRepository.UpdateAsync(ingredient);
    }
    
    public async Task DeleteIngredientAsync(int id)
    {
        await _inventoryRepository.SoftDeleteAsync(id);
    }
}