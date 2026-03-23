using ServiceDesk.Core.DTOs.AuditLogs;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис аудита действий
/// </summary>
public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityType, int entityId,
        string? oldValue, string? newValue, int userId);

    /// <summary>Получить журнал аудита с фильтрацией и пагинацией</summary>
    Task<PagedResultDto<AuditLogListDto>> GetLogsAsync(AuditLogFilterDto filter);
}
