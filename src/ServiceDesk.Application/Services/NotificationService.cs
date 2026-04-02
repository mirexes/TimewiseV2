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
        // Уведомляем назначенного специалиста о смене статуса
        if (ticket.AssignedEngineerId.HasValue)
        {
            var notification = new Notification
            {
                Title = $"Заявка {ticket.TicketNumber}",
                Message = $"Статус изменён: {GetStatusDisplayName(oldStatus)} → {GetStatusDisplayName(ticket.Status)}",
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

    public async Task OnTicketAssignedAsync(Ticket ticket)
    {
        // Уведомляем назначенного специалиста о новой заявке
        if (!ticket.AssignedEngineerId.HasValue)
            return;

        var notification = new Notification
        {
            Title = $"Новая заявка {ticket.TicketNumber}",
            Message = "На вас назначена заявка",
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

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int count = 20)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
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

    /// <summary>
    /// Русское название статуса заявки
    /// </summary>
    private static string GetStatusDisplayName(TicketStatus status) => status switch
    {
        TicketStatus.New => "Новая",
        TicketStatus.Processed => "Обработана",
        TicketStatus.CompletedRemotely => "Дистанционно",
        TicketStatus.PartsApproval => "Согласование",
        TicketStatus.RepairApproved => "Согласована",
        TicketStatus.DepartureConfirmation => "Подтверждение выезда",
        TicketStatus.EngineerEnRoute => "В пути",
        TicketStatus.InProgress => "Выполнение",
        TicketStatus.Completed => "Выполнена",
        TicketStatus.ContinuationRequired => "Продолжение",
        TicketStatus.Cancelled => "Отменена",
        _ => status.ToString()
    };
}
