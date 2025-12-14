using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Repositories;

public interface IInventoryRepository : IRepository<Ingredient>
{
    Task<List<Ingredient>> GetActiveIngredientsAsync();
    Task<List<Ingredient>> GetLowStockIngredientsAsync();
    Task<Ingredient> UpdateIngredientStockAsync(int ingredientId, decimal quantity, string reason);
    Task<List<InventoryLog>> GetInventoryLogsAsync(int? ingredientId = null);
    Task SoftDeleteAsync(int id);
}