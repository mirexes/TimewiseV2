using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.AuditLogs;

/// <summary>
/// Фильтр для журнала аудита
/// </summary>
public class AuditLogFilterDto
{
    public string? Search { get; set; }
    public AuditAction? Action { get; set; }
    public string? EntityType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}
