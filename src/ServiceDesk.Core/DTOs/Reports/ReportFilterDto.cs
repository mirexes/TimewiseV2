namespace ServiceDesk.Core.DTOs.Reports;

/// <summary>
/// DTO фильтра для отчётов
/// </summary>
public class ReportFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Region { get; set; }
    public int? EngineerId { get; set; }
}
