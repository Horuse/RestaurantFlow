using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RestaurantFlow.Server.Hubs;

public class RestaurantHub : Hub
{
    public async Task OrderCreated(int orderId)
    {
        await Clients.All.SendAsync("OnOrderCreated", orderId);
    }
    
    public async Task OrderStatusChanged(int orderId, string status)
    {
        await Clients.All.SendAsync("OnOrderStatusChanged", orderId, status);
    }
    
    public async Task OrderItemStatusChanged(int orderItemId, string status)
    {
        await Clients.All.SendAsync("OnOrderItemStatusChanged", orderItemId, status);
    }
    
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
    
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}