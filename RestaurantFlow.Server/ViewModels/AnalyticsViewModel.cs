using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;

namespace RestaurantFlow.Server.ViewModels;

public partial class AnalyticsViewModel : ReactiveObject
{
    private readonly IOrderService _orderService;
    private readonly IMenuService _menuService;
    
    [Reactive]
    private int _todayOrderCount = 0;
    
    [Reactive]
    private decimal _todayRevenue = 0;
    
    [Reactive]
    private double _averageOrderTime = 0;
    
    [Reactive]
    private int _pendingOrdersCount = 0;
    
    [Reactive]
    private ObservableCollection<TopSellingItem> _topSellingItems = new();
    
    [Reactive]
    private ObservableCollection<HourlyOrderData> _hourlyOrderData = new();
    
    [Reactive]
    private DateTime _selectedDate = DateTime.Today;
    
    [Reactive]
    private bool _isLoading = false;

    public AnalyticsViewModel(IOrderService orderService, IMenuService menuService)
    {
        _orderService = orderService;
        _menuService = menuService;
    }

    public async Task LoadAnalyticsAsync()
    {
        IsLoading = true;
        try
        {
            var startDate = SelectedDate.Date;
            var endDate = startDate.AddDays(1);
            
            // Get basic metrics
            var todayOrders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            TodayOrderCount = todayOrders.Count;
            TodayRevenue = todayOrders.Sum(o => o.TotalAmount);
            
            // Calculate average order time for completed orders
            var completedOrders = todayOrders.Where(o => o.CompletedAt.HasValue).ToList();
            if (completedOrders.Any())
            {
                var totalMinutes = completedOrders.Average(o => 
                    (o.CompletedAt!.Value - o.CreatedAt).TotalMinutes);
                AverageOrderTime = totalMinutes;
            }
            
            // Get pending orders count
            var pendingOrders = await _orderService.GetActiveOrdersAsync();
            PendingOrdersCount = pendingOrders.Count;
            
            // Get top selling items
            var topItems = await _menuService.GetTopSellingItemsAsync(startDate, endDate, 10);
            TopSellingItems.Clear();
            foreach (var item in topItems)
            {
                TopSellingItems.Add(new TopSellingItem
                {
                    ItemName = item.Name,
                    Quantity = item.TotalQuantity,
                    Revenue = item.TotalRevenue
                });
            }
            
            // Get hourly order data
            var hourlyData = todayOrders
                .GroupBy(o => o.CreatedAt.Hour)
                .Select(g => new HourlyOrderData
                {
                    Hour = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(h => h.Hour)
                .ToList();
                
            HourlyOrderData.Clear();
            foreach (var data in hourlyData)
            {
                HourlyOrderData.Add(data);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [ReactiveCommand]
    private async Task RefreshAnalyticsAsync()
    {
        await LoadAnalyticsAsync();
    }
    
    [ReactiveCommand]
    private async Task ChangeDateAsync(DateTime newDate)
    {
        SelectedDate = newDate;
        await LoadAnalyticsAsync();
    }
    
    [ReactiveCommand]
    private async Task PreviousDay()
    {
        await ChangeDateAsync(SelectedDate.AddDays(-1));
    }
    
    [ReactiveCommand]
    private async Task NextDay()
    {
        await ChangeDateAsync(SelectedDate.AddDays(1));
    }
    
    [ReactiveCommand]
    private async Task Today()
    {
        await ChangeDateAsync(DateTime.Today);
    }
}

public class TopSellingItem
{
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class HourlyOrderData
{
    public int Hour { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public string HourDisplay => $"{Hour:00}:00";
}