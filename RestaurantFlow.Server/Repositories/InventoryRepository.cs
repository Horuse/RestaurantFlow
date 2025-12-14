using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Repositories;

public class InventoryRepository : Repository<Ingredient>, IInventoryRepository
{
    public InventoryRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<List<Ingredient>> GetActiveIngredientsAsync()
    {
        return await _dbSet
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<List<Ingredient>> GetLowStockIngredientsAsync()
    {
        return await _dbSet
            .Where(i => i.IsActive && i.CurrentStock <= i.MinimumStock)
            .OrderBy(i => i.CurrentStock)
            .ToListAsync();
    }

    public async Task<Ingredient> UpdateIngredientStockAsync(int ingredientId, decimal quantity, string reason)
    {
        var ingredient = await _dbSet.FindAsync(ingredientId);
        if (ingredient == null)
            throw new ArgumentException($"Ingredient with ID {ingredientId} not found");

        var oldStock = ingredient.CurrentStock;
        ingredient.CurrentStock += quantity;

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
            .Take(100)
            .ToListAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var ingredient = await _dbSet.FindAsync(id);
        if (ingredient != null)
        {
            ingredient.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public override async Task DeleteAsync(int id)
    {
        await SoftDeleteAsync(id);
    }
}