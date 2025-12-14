using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Services;

public class MenuService : IMenuService
{
    private readonly RestaurantDbContext _context;
    
    public MenuService(RestaurantDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }
    
    public async Task<List<MenuItem>> GetMenuItemsAsync()
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .ToListAsync();
    }
    
    public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId)
            .ToListAsync();
    }
    
    public async Task<MenuItem?> GetMenuItemByIdAsync(int id)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
    
    public async Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem)
    {
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();
        return menuItem;
    }
    
    public async Task<MenuItem> UpdateMenuItemAsync(MenuItem menuItem)
    {
        _context.MenuItems.Update(menuItem);
        await _context.SaveChangesAsync();
        return menuItem;
    }
    
    public async Task DeleteMenuItemAsync(int id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem != null)
        {
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }
    
    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }
    
    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<List<TopSellingMenuItem>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int limit)
    {
        return await _context.OrderItems
            .Include(oi => oi.MenuItem)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt < endDate)
            .GroupBy(oi => new { oi.MenuItemId, oi.MenuItem.Name })
            .Select(g => new TopSellingMenuItem
            {
                Name = g.Key.Name,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(t => t.TotalQuantity)
            .Take(limit)
            .ToListAsync();
    }
}

public class TopSellingMenuItem
{
    public string Name { get; set; } = "";
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}