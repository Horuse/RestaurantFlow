using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RestaurantFlow.Server.Services;
using RestaurantFlow.Server.Repositories;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.ViewModels;

public partial class AnalyticsMetric : ReactiveObject
{
    [Reactive]
    private string _title = "";
    
    [Reactive]
    private string _value = "";
    
    [Reactive]
    private string _subtitle = "";
    
    [Reactive]
    private string _icon = "";
}

public partial class AnalyticsViewModel : ReactiveObject
{
    private readonly IOrderService _orderService;
    private readonly IMenuService _menuService;
    private readonly IAnalyticsRepository _analyticsRepository;
    
    [Reactive]
    private int _todayOrderCount = 0;
    
    [Reactive]
    private decimal _todayRevenue = 0;
    
    [Reactive]
    private double _averageOrderTime = 0;
    
    [Reactive]
    private int _pendingOrdersCount = 0;
    
    [Reactive]
    private string _averageCookingTime = "0 хв";
    
    [Reactive]
    private string _orderToDeliveryTime = "0 хв";
    
    [Reactive]
    private string _onTimeCompletionRate = "0%";
    
    [Reactive]
    private ObservableCollection<TopSellingItem> _topSellingItems = new();
    
    [Reactive]
    private ObservableCollection<TopSellingItem> _topSellingItemsWeek = new();
    
    [Reactive]
    private ObservableCollection<HourlyOrderData> _hourlyOrderData = new();
    
    public ObservableCollection<AnalyticsMetric> MainMetrics { get; } = new();
    
    [Reactive]
    private DateTime _selectedDate = DateTime.Today;
    
    [Reactive]
    private bool _isLoading = false;

    public AnalyticsViewModel(IOrderService orderService, IMenuService menuService, IAnalyticsRepository analyticsRepository)
    {
        _orderService = orderService;
        _menuService = menuService;
        _analyticsRepository = analyticsRepository;
        
        // Завантаження даних при ініціалізації
        _ = LoadAnalyticsAsync();
    }

    public async Task LoadAnalyticsAsync()
    {
        IsLoading = true;
        try
        {
            var startDate = SelectedDate.Date;
            var endDate = startDate.AddDays(1);
            
            var todayOrders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            TodayOrderCount = todayOrders.Count;
            TodayRevenue = todayOrders.Sum(o => o.TotalAmount);
            
            var completedOrders = todayOrders.Where(o => o.CompletedAt.HasValue).ToList();
            if (completedOrders.Any())
            {
                var totalMinutes = completedOrders.Average(o => 
                    (o.CompletedAt!.Value - o.CreatedAt).TotalMinutes);
                AverageOrderTime = totalMinutes;
            }
            
            var pendingOrders = await _orderService.GetActiveOrdersAsync();
            PendingOrdersCount = pendingOrders.Count;
            
            // Завантаження нових метрик
            await LoadCookingTimeMetricsAsync();
            await LoadOrderDeliveryMetricsAsync();
            await LoadOnTimeCompletionAsync();
            await LoadTopSellingItemsAsync();
            
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
            
            UpdateMainMetrics();
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadCookingTimeMetricsAsync()
    {
        try
        {
            var startDate = SelectedDate.Date;
            var endDate = startDate.AddDays(1);
            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed && 
                                              o.CompletedAt.HasValue).ToList();
            
            if (completedOrders.Any())
            {
                var totalCookingMinutes = completedOrders
                    .Select(o => (o.CompletedAt.Value - o.PaidAt).TotalMinutes)
                    .Average();
                
                AverageCookingTime = $"{totalCookingMinutes:F0} хв";
            }
            else
            {
                AverageCookingTime = "Н/Д";
            }
        }
        catch
        {
            AverageCookingTime = "Помилка";
        }
    }
    
    private async Task LoadOrderDeliveryMetricsAsync()
    {
        try
        {
            var startDate = SelectedDate.Date;
            var endDate = startDate.AddDays(1);
            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed && 
                                              o.CompletedAt.HasValue).ToList();
            
            if (completedOrders.Any())
            {
                var totalDeliveryMinutes = completedOrders
                    .Select(o => (o.CompletedAt.Value - o.CreatedAt).TotalMinutes)
                    .Average();
                
                OrderToDeliveryTime = $"{totalDeliveryMinutes:F0} хв";
            }
            else
            {
                OrderToDeliveryTime = "Н/Д";
            }
        }
        catch
        {
            OrderToDeliveryTime = "Помилка";
        }
    }
    
    private async Task LoadOnTimeCompletionAsync()
    {
        try
        {
            var startDate = SelectedDate.Date;
            var endDate = startDate.AddDays(1);
            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed && 
                                              o.CompletedAt.HasValue).ToList();
            
            if (completedOrders.Any())
            {
                var onTimeOrders = 0;
                
                foreach (var order in completedOrders)
                {
                    var totalEstimatedTime = order.OrderItems
                        .Select(oi => oi.MenuItem?.EstimatedCookingTimeMinutes ?? 0)
                        .DefaultIfEmpty(0)
                        .Max();
                    
                    var actualCookingTime = (order.CompletedAt.Value - order.PaidAt).TotalMinutes;
                    
                    // Вважаємо замовлення вчасним, якщо воно виконане не більше ніж на 25% довше від запланованого
                    if (actualCookingTime <= totalEstimatedTime * 1.25)
                    {
                        onTimeOrders++;
                    }
                }
                
                var onTimePercentage = (double)onTimeOrders / completedOrders.Count * 100;
                OnTimeCompletionRate = $"{onTimePercentage:F0}%";
            }
            else
            {
                OnTimeCompletionRate = "Н/Д";
            }
        }
        catch
        {
            OnTimeCompletionRate = "Помилка";
        }
    }
    
    private async Task LoadTopSellingItemsAsync()
    {
        try
        {
            // Популярні страви сьогодні
            var startDate = SelectedDate.Date;
            var endDate = startDate.AddDays(1);
            var topItems = await _analyticsRepository.GetTopSellingItemsAsync(startDate, endDate, 5);
            
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
            
            // Популярні страви за тиждень
            var weekStart = DateTime.Today.AddDays(-7);
            var weekEnd = DateTime.Today.AddDays(1);
            var topItemsWeek = await _analyticsRepository.GetTopSellingItemsAsync(weekStart, weekEnd, 5);
            
            TopSellingItemsWeek.Clear();
            foreach (var item in topItemsWeek)
            {
                TopSellingItemsWeek.Add(new TopSellingItem
                {
                    ItemName = item.Name,
                    Quantity = item.TotalQuantity,
                    Revenue = item.TotalRevenue
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading top selling items: {ex.Message}");
        }
    }
    
    private void UpdateMainMetrics()
    {
        MainMetrics.Clear();
        
        MainMetrics.Add(new AnalyticsMetric
        {
            Title = "Середній час приготування",
            Value = AverageCookingTime,
            Subtitle = "За обраний день",
            Icon = ""
        });
        
        MainMetrics.Add(new AnalyticsMetric
        {
            Title = "Від замовлення до видачі",
            Value = OrderToDeliveryTime,
            Subtitle = "Повний цикл обслуговування",
            Icon = ""
        });
        
        MainMetrics.Add(new AnalyticsMetric
        {
            Title = "Виконання вчасно",
            Value = OnTimeCompletionRate,
            Subtitle = "% замовлень вчасно",
            Icon = ""
        });
        
        MainMetrics.Add(new AnalyticsMetric
        {
            Title = "Замовлень сьогодні",
            Value = TodayOrderCount.ToString(),
            Subtitle = "Загальна кількість",
            Icon = ""
        });
        
        MainMetrics.Add(new AnalyticsMetric
        {
            Title = "Дохід сьогодні",
            Value = $"₴{TodayRevenue:F2}",
            Subtitle = "За обраний день",
            Icon = ""
        });
        
        MainMetrics.Add(new AnalyticsMetric
        {
            Title = "Активні замовлення",
            Value = PendingOrdersCount.ToString(),
            Subtitle = "В процесі виконання",
            Icon = ""
        });
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