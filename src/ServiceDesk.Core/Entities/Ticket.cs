using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Entities;

/// <summary>
/// Заявка на обслуживание
/// </summary>
public class Ticket : BaseEntity
{
    /// <summary>Номер заявки (генерируется автоматически)</summary>
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>Тип заявки</summary>
    public TicketType Type { get; set; }

    /// <summary>Статус заявки</summary>
    public TicketStatus Status { get; set; } = TicketStatus.New;

    /// <summary>Приоритет</summary>
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;

    /// <summary>Описание проблемы</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Комментарий</summary>
    public string? Comment { get; set; }

    /// <summary>Дедлайн выполнения</summary>
    public DateTime? Deadline { get; set; }

    /// <summary>Дата начала работ</summary>
    public DateTime? WorkStartedAt { get; set; }

    /// <summary>Дата завершения работ</summary>
    public DateTime? WorkCompletedAt { get; set; }

    /// <summary>Назначенный специалист</summary>
    public int? AssignedEngineerId { get; set; }
    public AppUser? AssignedEngineer { get; set; }

    /// <summary>Точка обслуживания</summary>
    public int ServicePointId { get; set; }
    public ServicePoint ServicePoint { get; set; } = null!;

    /// <summary>Оборудование</summary>
    public int? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    /// <summary>Создатель заявки</summary>
    public int CreatedByUserId { get; set; }
    public AppUser CreatedByUser { get; set; } = null!;

    /// <summary>Фото АВР (путь к файлу)</summary>
    public string? AvrPhotoPath { get; set; }

    // Навигационные свойства
    public ICollection<TicketSparePart> SpareParts { get; set; } = new List<TicketSparePart>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public ICollection<TicketPhoto> CompletionPhotos { get; set; } = new List<TicketPhoto>();
}
