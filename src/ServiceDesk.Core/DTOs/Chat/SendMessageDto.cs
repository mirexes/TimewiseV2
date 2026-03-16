using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Chat;

/// <summary>
/// DTO для отправки сообщения в чат
/// </summary>
public class SendMessageDto
{
    [Required(ErrorMessage = "Введите сообщение")]
    public string Text { get; set; } = string.Empty;

    public int TicketId { get; set; }
}
