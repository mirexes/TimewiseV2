using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.DTOs.Tickets;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис чата заявки
/// </summary>
public interface IChatService
{
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int ticketId);
    Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto, int senderId);
    Task MarkAsReadAsync(int ticketId, int userId);

    /// <summary>
    /// Создаёт сообщение в чате с вложениями (при создании заявки)
    /// </summary>
    Task AddMessageWithAttachmentsAsync(int ticketId, int senderId, string text, IEnumerable<TicketAttachmentFile> files);

    /// <summary>
    /// Отправляет сообщение с вложениями и возвращает DTO (для чата)
    /// </summary>
    Task<ChatMessageDto> SendMessageWithAttachmentsAsync(int ticketId, int senderId, string text, IEnumerable<TicketAttachmentFile> files, int? replyToMessageId = null);
}
