using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantFlow.Server.Repositories;

public interface IAnalyticsRepository
{
    Task<List<TopSellingMenuItem>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int limit);
}

public class TopSellingMenuItem
{
    public string Name { get; set; } = "";
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}