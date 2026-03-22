using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис уведомлений
/// </summary>
public interface INotificationService
{
    Task OnTicketStatusChangedAsync(Ticket ticket, TicketStatus oldStatus);
    Task OnTicketAssignedAsync(Ticket ticket);
    Task<int> GetUnreadCountAsync(int userId);
    Task<List<Notification>> GetUserNotificationsAsync(int userId, int count = 20);
    Task MarkAllAsReadAsync(int userId);
}
