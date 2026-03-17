using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;
using WebPush;

namespace ServiceDesk.Infrastructure.ExternalServices;

/// <summary>
/// Реализация Web Push уведомлений (VAPID)
/// </summary>
public class WebPushService : IWebPushService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebPushService> _logger;

    public WebPushService(AppDbContext db, IConfiguration configuration, ILogger<WebPushService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(int userId, string title, string message, string? url = null)
    {
        var publicKey = _configuration["WebPush:VapidPublicKey"];
        var privateKey = _configuration["WebPush:VapidPrivateKey"];
        var subject = _configuration["WebPush:Subject"] ?? "mailto:admin@timewise.ru";

        if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
        {
            _logger.LogWarning("VAPID ключи не настроены, пропускаем отправку Push");
            return;
        }

        var subscriptions = await _db.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (!subscriptions.Any())
        {
            _logger.LogDebug("Нет Push-подписок для пользователя {UserId}", userId);
            return;
        }

        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        var webPushClient = new WebPushClient();
        var payload = System.Text.Json.JsonSerializer.Serialize(new { title, message, url });

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSubscription = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone ||
                                                ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Подписка устарела — удаляем
                _logger.LogInformation("Удаляем устаревшую Push-подписку {Endpoint}", sub.Endpoint);
                _db.PushSubscriptions.Remove(sub);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки Push на {Endpoint}", sub.Endpoint);
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task SubscribeAsync(int userId, string endpoint, string p256dh, string auth)
    {
        // Проверяем, нет ли уже такой подписки
        var existing = await _db.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Endpoint == endpoint);

        if (existing != null)
        {
            // Обновляем ключи и привязку к пользователю
            existing.P256dh = p256dh;
            existing.Auth = auth;
            existing.UserId = userId;
        }
        else
        {
            _db.PushSubscriptions.Add(new Core.Entities.PushSubscription
            {
                UserId = userId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Web Push подписка сохранена для пользователя {UserId}", userId);
    }

    public async Task UnsubscribeAsync(int userId)
    {
        var subscriptions = await _db.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        _db.PushSubscriptions.RemoveRange(subscriptions);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Web Push подписки удалены для пользователя {UserId}", userId);
    }
}
