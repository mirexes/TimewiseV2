using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис аудита действий
/// </summary>
public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityType, int entityId,
        string? oldValue, string? newValue, int userId);
}
