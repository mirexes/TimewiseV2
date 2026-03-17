namespace ServiceDesk.Core.DTOs.Chat;

/// <summary>
/// DTO для отправки сообщения в чат
/// </summary>
public class SendMessageDto
{
    public string Text { get; set; } = string.Empty;

    public int TicketId { get; set; }

    /// <summary>ID сообщения, на которое отвечаем</summary>
    public int? ReplyToMessageId { get; set; }
}
