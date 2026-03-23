using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.AuditLogs;

/// <summary>
/// DTO для отображения записи журнала аудита в списке
/// </summary>
public class AuditLogListDto
{
    public int Id { get; set; }
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Отображаемое название действия</summary>
    public string ActionDisplayName => Action switch
    {
        AuditAction.Created => "Создание",
        AuditAction.Updated => "Обновление",
        AuditAction.Deleted => "Удаление",
        AuditAction.StatusChanged => "Смена статуса",
        AuditAction.Assigned => "Назначение",
        AuditAction.Login => "Вход",
        AuditAction.Logout => "Выход",
        AuditAction.PartsApproved => "Согласование запчастей",
        AuditAction.PartsRejected => "Отклонение запчастей",
        AuditAction.ConsentGranted => "Согласие на ПД",
        AuditAction.ConsentRevoked => "Отзыв согласия на ПД",
        AuditAction.PersonalDataRequested => "Запрос ПД",
        AuditAction.PersonalDataExported => "Экспорт ПД",
        AuditAction.PersonalDataDeleted => "Удаление ПД",
        _ => Action.ToString()
    };

    /// <summary>Отображаемое название типа сущности</summary>
    public string EntityTypeDisplayName => EntityType switch
    {
        "Ticket" => "Заявка",
        "Equipment" => "Оборудование",
        "ServicePoint" => "Точка обслуживания",
        "Client" => "Клиент",
        "AppUser" or "User" => "Пользователь",
        "SparePart" => "Запчасть",
        "ChatMessage" => "Сообщение",
        _ => EntityType
    };
}
