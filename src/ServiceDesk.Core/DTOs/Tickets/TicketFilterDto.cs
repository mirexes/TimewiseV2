using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Tickets;

/// <summary>
/// DTO для фильтрации заявок
/// </summary>
public class TicketFilterDto
{
    public TicketStatus? Status { get; set; }
    public TicketType? Type { get; set; }
    public TicketPriority? Priority { get; set; }
    public int? EngineerId { get; set; }
    public string? Region { get; set; }
    public string? Network { get; set; }
    public string? SearchQuery { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
