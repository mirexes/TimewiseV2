using ServiceDesk.Core.DTOs.Reports;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис отчётов и статистики
/// </summary>
public interface IReportService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(int currentUserId, UserRole currentUserRole);
    Task<IEnumerable<EngineerStatsDto>> GetEngineerStatsAsync(ReportFilterDto filter);
}
