namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис Web Push уведомлений (VAPID)
/// </summary>
public interface IWebPushService
{
    Task SendAsync(int userId, string title, string message, string? url = null);
    Task SubscribeAsync(int userId, string endpoint, string p256dh, string auth);
    Task UnsubscribeAsync(int userId);
}
