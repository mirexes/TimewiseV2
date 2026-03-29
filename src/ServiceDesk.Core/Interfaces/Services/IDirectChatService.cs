using ServiceDesk.Core.DTOs.Chat;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис личных чатов между сотрудниками
/// </summary>
public interface IDirectChatService
{
    /// <summary>Получить список всех чатов пользователя (личные + групповой)</summary>
    Task<IEnumerable<ChatListItemDto>> GetUserChatsAsync(int userId);

    /// <summary>Создать или найти существующий личный чат между двумя пользователями</summary>
    Task<int> GetOrCreateDirectChatAsync(int userId1, int userId2);

    /// <summary>Получить список сотрудников, с которыми можно начать чат</summary>
    Task<IEnumerable<CompanyChatMemberDto>> GetAvailableEmployeesAsync(int currentUserId);
}
