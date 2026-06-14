using ServiceDesk.Core.DTOs.Reports;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис учёта наработки сотрудников
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Возвращает статистику наработки выездного персонала за указанный месяц.
    /// Период всегда отсчитывается с 1-го числа месяца.
    /// </summary>
    Task<EmployeeWorkReportDto> GetMonthlyWorkStatsAsync(int year, int month);
}
