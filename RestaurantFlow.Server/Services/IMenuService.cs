using RestaurantFlow.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantFlow.Server.Services;

public interface IMenuService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<MenuItem>> GetMenuItemsAsync();
    Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<MenuItem?> GetMenuItemByIdAsync(int id);
    Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem);
    Task<MenuItem> UpdateMenuItemAsync(MenuItem menuItem);
    Task DeleteMenuItemAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task<List<TopSellingMenuItem>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int limit);
}