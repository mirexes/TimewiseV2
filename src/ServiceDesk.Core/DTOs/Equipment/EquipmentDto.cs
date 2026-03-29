namespace ServiceDesk.Core.DTOs.Equipment;

/// <summary>
/// DTO оборудования
/// </summary>
public class EquipmentDto
{
    public int Id { get; set; }
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime? InstalledAt { get; set; }
    public string? Description { get; set; }
    public string? PhotoPath { get; set; }
    public bool IsCompanyOwned { get; set; }
    public bool IsActive { get; set; }
    public int ServicePointId { get; set; }
    public string ServicePointName { get; set; } = string.Empty;
    public string ServicePointAddress { get; set; } = string.Empty;
}
