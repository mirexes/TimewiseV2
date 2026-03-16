namespace ServiceDesk.Core.Entities;

/// <summary>
/// Сообщение в чате заявки
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>Текст сообщения</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Заявка</summary>
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    /// <summary>Автор сообщения</summary>
    public int SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;

    /// <summary>Прочитано ли сообщение</summary>
    public bool IsRead { get; set; }

    // Навигационные свойства
    public ICollection<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();
}
