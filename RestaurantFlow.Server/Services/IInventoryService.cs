using RestaurantFlow.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantFlow.Server.Services;

public interface IInventoryService
{
    Task<List<Ingredient>> GetIngredientsAsync();
    Task<List<Ingredient>> GetLowStockIngredientsAsync();
    Task<Ingredient> UpdateIngredientStockAsync(int ingredientId, decimal quantity, string reason);
    Task<List<InventoryLog>> GetInventoryLogsAsync(int? ingredientId = null);
    Task<Ingredient> CreateIngredientAsync(Ingredient ingredient);
    Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient);
    Task DeleteIngredientAsync(int id);
}