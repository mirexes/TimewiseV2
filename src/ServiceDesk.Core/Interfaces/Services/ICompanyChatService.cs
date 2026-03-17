using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.DTOs.Tickets;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис группового чата компании
/// </summary>
public interface ICompanyChatService
{
    /// <summary>Получить или создать групповой чат компании</summary>
    Task<int> GetOrCreateChatAsync();

    /// <summary>Проверить, является ли пользователь участником чата</summary>
    Task<bool> IsMemberAsync(int chatId, int userId);

    /// <summary>Получить сообщения группового чата</summary>
    Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int chatId);

    /// <summary>Отправить сообщение в групповой чат</summary>
    Task<ChatMessageDto> SendMessageAsync(int chatId, int senderId, string text, int? replyToMessageId = null);

    /// <summary>Отправить сообщение с вложениями</summary>
    Task<ChatMessageDto> SendMessageWithAttachmentsAsync(int chatId, int senderId, string text, IEnumerable<TicketAttachmentFile> files, int? replyToMessageId = null);

    /// <summary>Отметить сообщения как прочитанные</summary>
    Task MarkAsReadAsync(int chatId, int userId);

    /// <summary>Получить список участников чата</summary>
    Task<IEnumerable<CompanyChatMemberDto>> GetMembersAsync(int chatId);

    /// <summary>Добавить участника в чат (только модератор)</summary>
    Task AddMemberAsync(int chatId, int userId);

    /// <summary>Удалить участника из чата (только модератор)</summary>
    Task RemoveMemberAsync(int chatId, int userId);
}
