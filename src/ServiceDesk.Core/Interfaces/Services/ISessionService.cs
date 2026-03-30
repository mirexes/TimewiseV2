using ServiceDesk.Core.Entities;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Управление сессиями пользователей
/// </summary>
public interface ISessionService
{
    /// <summary>Получить все активные сессии пользователя</summary>
    Task<List<UserSession>> GetActiveSessionsAsync(int userId);

    /// <summary>Отозвать конкретную сессию</summary>
    Task RevokeSessionAsync(int sessionId);

    /// <summary>Отозвать все сессии пользователя</summary>
    Task RevokeAllSessionsAsync(int userId);

    /// <summary>Отозвать все сессии пользователя, кроме указанной</summary>
    Task RevokeOtherSessionsAsync(int userId, string currentSessionToken);

    /// <summary>Удалить просроченные сессии (для фоновой очистки)</summary>
    Task CleanupExpiredSessionsAsync();
}
