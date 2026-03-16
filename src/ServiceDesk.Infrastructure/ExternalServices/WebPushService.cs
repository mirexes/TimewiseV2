using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceDesk.Core.Interfaces.Services;

namespace ServiceDesk.Infrastructure.ExternalServices;

/// <summary>
/// Реализация Web Push уведомлений (VAPID)
/// </summary>
public class WebPushService : IWebPushService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebPushService> _logger;

    public WebPushService(IConfiguration configuration, ILogger<WebPushService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(int userId, string title, string message, string? url = null)
    {
        // TODO: Реализовать отправку Web Push через VAPID
        _logger.LogInformation("Web Push для пользователя {UserId}: {Title} — {Message}", userId, title, message);
        await Task.CompletedTask;
    }

    public async Task SubscribeAsync(int userId, string endpoint, string p256dh, string auth)
    {
        // TODO: Сохранить подписку в БД
        _logger.LogInformation("Web Push подписка для пользователя {UserId}", userId);
        await Task.CompletedTask;
    }

    public async Task UnsubscribeAsync(int userId)
    {
        // TODO: Удалить подписку из БД
        _logger.LogInformation("Web Push отписка для пользователя {UserId}", userId);
        await Task.CompletedTask;
    }
}
