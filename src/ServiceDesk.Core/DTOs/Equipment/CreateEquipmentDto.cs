using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Equipment;

/// <summary>
/// DTO для создания оборудования
/// </summary>
public class CreateEquipmentDto
{
    [Required(ErrorMessage = "Укажите модель")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите серийный номер")]
    public string SerialNumber { get; set; } = string.Empty;

    public DateTime? InstalledAt { get; set; }
    public string? Description { get; set; }

    /// <summary>Подменное оборудование (собственность компании)</summary>
    public bool IsCompanyOwned { get; set; }

    [Required(ErrorMessage = "Укажите точку обслуживания")]
    public int ServicePointId { get; set; }

    /// <summary>Новый адрес (если точка не выбрана из списка)</summary>
    public string? NewAddress { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
}
