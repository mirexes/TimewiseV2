using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Tickets;

/// <summary>
/// DTO для смены статуса заявки
/// </summary>
public class UpdateTicketStatusDto
{
    public int TicketId { get; set; }
    public TicketStatus NewStatus { get; set; }
    public string? Comment { get; set; }
}
