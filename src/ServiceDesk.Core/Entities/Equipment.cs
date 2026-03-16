namespace ServiceDesk.Core.Entities;

/// <summary>
/// Оборудование (кофемашина и т.п.)
/// </summary>
public class Equipment : BaseEntity
{
    /// <summary>Модель оборудования</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Серийный номер</summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Дата установки</summary>
    public DateTime? InstalledAt { get; set; }

    /// <summary>Описание / заметки</summary>
    public string? Description { get; set; }

    /// <summary>Активно ли оборудование</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Точка обслуживания</summary>
    public int ServicePointId { get; set; }
    public ServicePoint ServicePoint { get; set; } = null!;

    // Навигационные свойства
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
