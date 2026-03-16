namespace ServiceDesk.Core.DTOs.Reports;

/// <summary>
/// DTO статистики для дашборда
/// </summary>
public class DashboardStatsDto
{
    public int TotalTickets { get; set; }
    public int NewTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int OverdueTickets { get; set; }
    public int TotalEquipment { get; set; }
    public int TotalClients { get; set; }
    public int TotalEngineers { get; set; }

    /// <summary>Заявки по статусам (для Chart.js)</summary>
    public Dictionary<string, int> TicketsByStatus { get; set; } = new();

    /// <summary>Заявки по дням за последние 30 дней</summary>
    public Dictionary<string, int> TicketsByDay { get; set; } = new();
}
