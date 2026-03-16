using ServiceDesk.Core.DTOs.Chat;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис чата заявки
/// </summary>
public interface IChatService
{
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int ticketId);
    Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto, int senderId);
    Task MarkAsReadAsync(int ticketId, int userId);
}
