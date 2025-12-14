using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Server.Repositories;

namespace RestaurantFlow.Server.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IAnalyticsRepository _analyticsRepository;
    
    public MenuService(IMenuRepository menuRepository, IAnalyticsRepository analyticsRepository)
    {
        _menuRepository = menuRepository;
        _analyticsRepository = analyticsRepository;
    }
    
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _menuRepository.GetCategoriesAsync();
    }
    
    public async Task<List<MenuItem>> GetMenuItemsAsync()
    {
        return await _menuRepository.GetMenuItemsWithCategoriesAsync();
    }
    
    public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return await _menuRepository.GetMenuItemsByCategoryAsync(categoryId);
    }
    
    public async Task<MenuItem?> GetMenuItemByIdAsync(int id)
    {
        return await _menuRepository.GetMenuItemWithCategoryAsync(id);
    }
    
    public async Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem)
    {
        return await _menuRepository.AddAsync(menuItem);
    }
    
    public async Task<MenuItem> UpdateMenuItemAsync(MenuItem menuItem)
    {
        return await _menuRepository.UpdateAsync(menuItem);
    }
    
    public async Task DeleteMenuItemAsync(int id)
    {
        await _menuRepository.DeleteAsync(id);
    }
    
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        return await _menuRepository.AddCategoryAsync(category);
    }
    
    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        return await _menuRepository.UpdateCategoryAsync(category);
    }
    
    public async Task DeleteCategoryAsync(int id)
    {
        await _menuRepository.DeleteCategoryAsync(id);
    }
    
    public async Task<List<TopSellingMenuItem>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int limit)
    {
        return await _analyticsRepository.GetTopSellingItemsAsync(startDate, endDate, limit);
    }
}