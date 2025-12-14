using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Data;

namespace RestaurantFlow.Server.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly RestaurantDbContext _context;

    public AnalyticsRepository(RestaurantDbContext context)
    {
        _context = context;
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