using Microsoft.AspNetCore.Mvc;
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

    public NotificationsApiController(INotificationService notificationService)
    {
        _notificationService = notificationService;
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
}
