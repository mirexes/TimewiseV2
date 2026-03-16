using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Reports;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Отчёты и статистика
/// </summary>
[RoleAuthorize(UserRole.ChiefEngineer, UserRole.ManagerTimewise, UserRole.Moderator)]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var stats = await _reportService.GetDashboardStatsAsync();
        return View(stats);
    }

    [HttpGet]
    public async Task<IActionResult> Engineers([FromQuery] ReportFilterDto filter)
    {
        var stats = await _reportService.GetEngineerStatsAsync(filter);
        return View(stats);
    }
}
