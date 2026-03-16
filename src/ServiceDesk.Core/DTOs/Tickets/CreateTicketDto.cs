using System.ComponentModel.DataAnnotations;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Tickets;

/// <summary>
/// DTO для создания заявки
/// </summary>
public class CreateTicketDto
{
    [Required(ErrorMessage = "Укажите тип заявки")]
    public TicketType Type { get; set; }

    [Required(ErrorMessage = "Укажите описание")]
    [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
    public string Description { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Normal;

    /// <summary>Существующая точка обслуживания (если выбрана из списка)</summary>
    public int? ServicePointId { get; set; }

    /// <summary>Новый адрес (если указан через карту)</summary>
    public string? NewAddress { get; set; }

    /// <summary>Широта нового адреса</summary>
    public double? Latitude { get; set; }

    /// <summary>Долгота нового адреса</summary>
    public double? Longitude { get; set; }

    public int? EquipmentId { get; set; }

    public DateTime? Deadline { get; set; }
}
