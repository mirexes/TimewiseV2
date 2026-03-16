using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Entities;

/// <summary>
/// Журнал аудита действий в системе
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Действие</summary>
    public AuditAction Action { get; set; }

    /// <summary>Тип сущности (Ticket, Equipment и т.д.)</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>ID сущности</summary>
    public int EntityId { get; set; }

    /// <summary>Старое значение</summary>
    public string? OldValue { get; set; }

    /// <summary>Новое значение</summary>
    public string? NewValue { get; set; }

    /// <summary>Кто выполнил действие</summary>
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>IP-адрес</summary>
    public string? IpAddress { get; set; }
}
