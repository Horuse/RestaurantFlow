using System.Threading.Tasks;

namespace RestaurantFlow.Server.Services;

public interface INotificationService
{
    Task NotifyMenuUpdated();
}

public class NotificationService : INotificationService
{
    public async Task NotifyMenuUpdated()
    {
        await Task.CompletedTask;
    }
}