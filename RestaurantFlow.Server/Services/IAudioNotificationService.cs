using System;
using System.Threading.Tasks;

namespace RestaurantFlow.Server.Services;

public interface IAudioNotificationService
{
    Task PlayOrderReadyNotification();
}

public class AudioNotificationService : IAudioNotificationService
{
    public async Task PlayOrderReadyNotification()
    {
        try
        {
            System.Diagnostics.Process.Start("afplay", "/System/Library/Sounds/Funk.aiff");
        }
        catch (Exception ex)
        {
            
            // Логування помилки без зупинки роботи
            System.Console.WriteLine($"Помилка відтворення звуку: {ex.Message}");
        }
    }
}