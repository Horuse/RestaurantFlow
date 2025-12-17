using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;

namespace RestaurantFlow.Server.Repositories;

public class MenuRepository : Repository<MenuItem>, IMenuRepository
{
    public MenuRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetMenuItemWithCategoryAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<MenuItem>> GetMenuItemsWithCategoriesAsync()
    {
        return await _dbSet
            .Include(m => m.Category)
            .ToListAsync();
    }

    public async Task<Category> AddCategoryAsync(Category category)
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

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<List<MenuItemIngredient>> GetMenuItemIngredientsAsync(int menuItemId)
    {
        return await _context.MenuItemIngredients
            .Include(mii => mii.Ingredient)
            .Where(mii => mii.MenuItemId == menuItemId)
            .ToListAsync();
    }

    public async Task UpdateMenuItemIngredientsAsync(int menuItemId, List<MenuItemIngredient> ingredients)
    {
        // Видаляємо старі інгредієнти
        var existingIngredients = await _context.MenuItemIngredients
            .Where(mii => mii.MenuItemId == menuItemId)
            .ToListAsync();
        
        _context.MenuItemIngredients.RemoveRange(existingIngredients);

        // Додаємо нові інгредієнти
        foreach (var ingredient in ingredients)
        {
            ingredient.MenuItemId = menuItemId;
            _context.MenuItemIngredients.Add(ingredient);
        }

        await _context.SaveChangesAsync();
    }
}