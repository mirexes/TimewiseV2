namespace ServiceDesk.Core.DTOs.Equipment;

/// <summary>
/// DTO для истории ремонта оборудования
/// </summary>
public class EquipmentHistoryDto
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? EngineerName { get; set; }
}
