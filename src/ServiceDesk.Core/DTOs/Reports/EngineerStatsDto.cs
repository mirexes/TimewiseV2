namespace ServiceDesk.Core.DTOs.Reports;

/// <summary>
/// DTO статистики по инженеру
/// </summary>
public class EngineerStatsDto
{
    public int EngineerId { get; set; }
    public string EngineerName { get; set; } = string.Empty;
    public int TotalTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int InProgressTickets { get; set; }
    public double AvgCompletionHours { get; set; }
}
