using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Сотрудники — учёт наработки за месяц (часы работы и время в дороге)
/// </summary>
[RoleAuthorize(UserRole.ChiefEngineer, UserRole.ManagerTimewise, UserRole.Moderator)]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now = DateTime.UtcNow;
        var report = await _employeeService.GetMonthlyWorkStatsAsync(year ?? now.Year, month ?? now.Month);
        return View(report);
    }
}
