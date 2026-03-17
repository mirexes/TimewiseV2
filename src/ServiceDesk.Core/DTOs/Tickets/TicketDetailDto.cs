using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Tickets;

/// <summary>
/// DTO для детальной карточки заявки
/// </summary>
public class TicketDetailDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? WorkStartedAt { get; set; }
    public DateTime? WorkCompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Специалист
    public int? AssignedEngineerId { get; set; }
    public string? EngineerName { get; set; }

    // Точка обслуживания
    public int ServicePointId { get; set; }
    public string ServicePointName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Оборудование
    public int? EquipmentId { get; set; }
    public string? EquipmentModel { get; set; }
    public string? EquipmentSerialNumber { get; set; }

    // Создатель
    public string CreatedByName { get; set; } = string.Empty;

    // АВР
    public string? AvrPhotoPath { get; set; }

    /// <summary>Допустимые переходы из текущего статуса</summary>
    public TicketStatus[] AllowedTransitions { get; set; } = [];

    /// <summary>Может ли текущий пользователь назначать специалиста</summary>
    public bool CanAssignEngineer { get; set; }
}
