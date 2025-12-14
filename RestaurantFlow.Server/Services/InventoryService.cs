using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Services;

public class InventoryService : IInventoryService
{
    private readonly RestaurantDbContext _context;
    
    public InventoryService(RestaurantDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Ingredient>> GetIngredientsAsync()
    {
        return await _context.Ingredients
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }
    
    public async Task<List<Ingredient>> GetLowStockIngredientsAsync()
    {
        return await _context.Ingredients
            .Where(i => i.IsActive && i.CurrentStock <= i.MinimumStock)
            .OrderBy(i => i.CurrentStock)
            .ToListAsync();
    }
    
    public async Task<Ingredient> UpdateIngredientStockAsync(int ingredientId, decimal quantity, string reason)
    {
        var ingredient = await _context.Ingredients.FindAsync(ingredientId);
        if (ingredient == null)
            throw new ArgumentException($"Ingredient with ID {ingredientId} not found");
            
        var oldStock = ingredient.CurrentStock;
        ingredient.CurrentStock += quantity;
        
        // Create inventory log
        var log = new InventoryLog
        {
            IngredientId = ingredientId,
            QuantityChanged = quantity,
            StockAfter = ingredient.CurrentStock,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.InventoryLogs.Add(log);
        await _context.SaveChangesAsync();
        
        return ingredient;
    }
    
    public async Task<List<InventoryLog>> GetInventoryLogsAsync(int? ingredientId = null)
    {
        var query = _context.InventoryLogs
            .Include(il => il.Ingredient)
            .AsQueryable();
            
        if (ingredientId.HasValue)
        {
            query = query.Where(il => il.IngredientId == ingredientId.Value);
        }
        
        return await query
            .OrderByDescending(il => il.CreatedAt)
            .Take(100) // Limit to last 100 logs
            .ToListAsync();
    }
    
    public async Task<Ingredient> CreateIngredientAsync(Ingredient ingredient)
    {
        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();
        return ingredient;
    }
    
    public async Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient)
    {
        _context.Ingredients.Update(ingredient);
        await _context.SaveChangesAsync();
        return ingredient;
    }
    
    public async Task DeleteIngredientAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient != null)
        {
            ingredient.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
        }
    }
}