using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RestaurantFlow.Server.DTOs;
using RestaurantFlow.Shared.Enums;

namespace RestaurantFlow.Server.Hubs;

public class RestaurantApiHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}

public interface IRestaurantNotificationService
{
    Task NotifyOrderStatusChanged(OrderStatusUpdate update);
    Task NotifyNewOrder(OrderResponse order);
    Task NotifyMenuUpdated();
    Task NotifyInventoryUpdated(int ingredientId);
}

public class RestaurantNotificationService : IRestaurantNotificationService
{
    private readonly IHubContext<RestaurantApiHub> _hubContext;

    public RestaurantNotificationService(IHubContext<RestaurantApiHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyOrderStatusChanged(OrderStatusUpdate update)
    {
        await _hubContext.Clients.All.SendAsync("OrderStatusChanged", update);
    }

    public async Task NotifyNewOrder(OrderResponse order)
    {
        await _hubContext.Clients.All.SendAsync("NewOrder", order);
    }

    public async Task NotifyMenuUpdated()
    {
        await _hubContext.Clients.All.SendAsync("MenuUpdated");
    }

    public async Task NotifyInventoryUpdated(int ingredientId)
    {
        await _hubContext.Clients.All.SendAsync("InventoryUpdated", ingredientId);
    }
}