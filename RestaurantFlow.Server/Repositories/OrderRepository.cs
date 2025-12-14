using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Data;
using RestaurantFlow.Data.Entities;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(RestaurantDbContext context) : base(context)
    {
    }

    public async Task<List<Order>> GetAllOrdersWithItemsAsync()
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetActiveOrdersAsync()
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt < endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetRecentCompletedOrdersAsync()
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.Status == OrderStatus.Completed)
            .OrderByDescending(o => o.CompletedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(int id)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _dbSet.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            if (status == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateOrderItemStatusAsync(int orderItemId, OrderStatus status)
    {
        var orderItem = await _context.OrderItems.FindAsync(orderItemId);
        if (orderItem != null)
        {
            orderItem.Status = status;
            if (status == OrderStatus.InProgress)
            {
                orderItem.StartedCookingAt = DateTime.UtcNow;
            }
            else if (status == OrderStatus.Ready)
            {
                orderItem.ReadyAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<OrderItem>> GetOrderItemsByStatusAsync(OrderStatus status)
    {
        return await _context.OrderItems
            .Include(oi => oi.MenuItem)
            .Include(oi => oi.Order)
            .Where(oi => oi.Status == status)
            .OrderBy(oi => oi.Order.CreatedAt)
            .ToListAsync();
    }
}