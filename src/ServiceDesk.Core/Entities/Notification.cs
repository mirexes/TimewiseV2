namespace ServiceDesk.Core.Entities;

/// <summary>
/// Уведомление пользователя
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>Заголовок</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Текст уведомления</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Ссылка для перехода</summary>
    public string? Url { get; set; }

    /// <summary>Прочитано ли</summary>
    public bool IsRead { get; set; }

    /// <summary>Получатель</summary>
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>Связанная заявка</summary>
    public int? TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}
