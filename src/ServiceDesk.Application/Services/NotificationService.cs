using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис уведомлений
/// </summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IWebPushService _webPush;

    public NotificationService(AppDbContext db, IWebPushService webPush)
    {
        _db = db;
        _webPush = webPush;
    }

    public async Task OnTicketStatusChangedAsync(Ticket ticket, TicketStatus oldStatus)
    {
        // Уведомляем назначенного специалиста
        if (ticket.AssignedEngineerId.HasValue)
        {
            var notification = new Notification
            {
                Title = $"Заявка {ticket.TicketNumber}",
                Message = $"Статус изменён: {oldStatus} → {ticket.Status}",
                Url = $"/Tickets/Details/{ticket.Id}",
                UserId = ticket.AssignedEngineerId.Value,
                TicketId = ticket.Id
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await _webPush.SendAsync(
                ticket.AssignedEngineerId.Value,
                notification.Title,
                notification.Message,
                notification.Url);
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications)
            n.IsRead = true;

        await _db.SaveChangesAsync();
    }
}
