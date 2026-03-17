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

    /// <summary>Ответ на сообщение (null если не ответ)</summary>
    public int? ReplyToMessageId { get; set; }
    public ChatMessage? ReplyToMessage { get; set; }

    // Навигационные свойства
    public ICollection<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();
    public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
}
