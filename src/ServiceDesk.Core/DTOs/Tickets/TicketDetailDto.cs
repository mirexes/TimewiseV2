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
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Клиент (через точку обслуживания)
    public string? ClientName { get; set; }
    public string? ClientTtkFilePath { get; set; }

    // Оборудование
    public int? EquipmentId { get; set; }
    public string? EquipmentModel { get; set; }
    public string? EquipmentSerialNumber { get; set; }

    // Создатель
    public int CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    // АВР
    public string? AvrPhotoPath { get; set; }

    /// <summary>Фото акта выполненных работ</summary>
    public List<TicketPhotoDto> CompletionPhotos { get; set; } = new();

    /// <summary>Отображаемое название типа</summary>
    public string TypeDisplayName => Type switch
    {
        TicketType.Repair => "Ремонт",
        TicketType.Maintenance => "Техническое обслуживание",
        TicketType.Installation => "Установка",
        TicketType.Dismantling => "Демонтаж",
        TicketType.Delivery => "Поставка",
        TicketType.Consultation => "Консультация",
        _ => Type.ToString()
    };

    /// <summary>Отображаемое название приоритета</summary>
    public string PriorityDisplayName => Priority switch
    {
        TicketPriority.Low => "Низкий",
        TicketPriority.Normal => "Обычный",
        TicketPriority.High => "Высокий",
        TicketPriority.Critical => "Критичный",
        _ => Priority.ToString()
    };

    /// <summary>Допустимые переходы из текущего статуса</summary>
    public TicketStatus[] AllowedTransitions { get; set; } = [];

    /// <summary>Может ли текущий пользователь назначать специалиста</summary>
    public bool CanAssignEngineer { get; set; }

    /// <summary>Может ли текущий пользователь привязывать/менять оборудование</summary>
    public bool CanEditEquipment { get; set; }

    /// <summary>Может ли текущий пользователь редактировать заявку (только создатель)</summary>
    public bool CanEdit { get; set; }
}

/// <summary>
/// DTO для фото акта выполненных работ
/// </summary>
public class TicketPhotoDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
