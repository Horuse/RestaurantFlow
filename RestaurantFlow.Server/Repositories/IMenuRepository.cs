using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Repositories;

public interface IMenuRepository : IRepository<MenuItem>
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<MenuItem?> GetMenuItemWithCategoryAsync(int id);
    Task<List<MenuItem>> GetMenuItemsWithCategoriesAsync();
    Task<Category> AddCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task<Category?> GetCategoryByIdAsync(int id);
}