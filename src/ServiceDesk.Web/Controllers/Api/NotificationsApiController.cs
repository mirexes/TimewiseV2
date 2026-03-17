using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Controllers.Api;

/// <summary>
/// API для уведомлений
/// </summary>
[ApiController]
[Route("api/notifications")]
public class NotificationsApiController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IWebPushService _webPushService;
    private readonly IConfiguration _configuration;

    public NotificationsApiController(
        INotificationService notificationService,
        IWebPushService webPushService,
        IConfiguration configuration)
    {
        _notificationService = notificationService;
        _webPushService = webPushService;
        _configuration = configuration;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetList()
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(User.GetUserId());
        var result = notifications.Select(n => new
        {
            n.Id,
            n.Title,
            n.Message,
            n.Url,
            n.IsRead,
            createdAt = n.CreatedAt.ToString("dd.MM.yyyy HH:mm")
        });
        return Ok(result);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(User.GetUserId());
        return Ok(new { count });
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(User.GetUserId());
        return Ok(new { success = true });
    }

    [HttpGet("vapid-key")]
    public IActionResult GetVapidKey()
    {
        var key = _configuration["WebPush:VapidPublicKey"] ?? "";
        return Ok(new { key });
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscribeRequest request)
    {
        await _webPushService.SubscribeAsync(User.GetUserId(), request.Endpoint, request.P256dh, request.Auth);
        return Ok(new { success = true });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe()
    {
        await _webPushService.UnsubscribeAsync(User.GetUserId());
        return Ok(new { success = true });
    }
}

/// <summary>
/// Модель запроса подписки на Push
/// </summary>
public class PushSubscribeRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
