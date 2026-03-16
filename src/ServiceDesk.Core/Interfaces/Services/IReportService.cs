using ServiceDesk.Core.DTOs.Reports;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис отчётов и статистики
/// </summary>
public interface IReportService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<IEnumerable<EngineerStatsDto>> GetEngineerStatsAsync(ReportFilterDto filter);
}
