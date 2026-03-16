using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Главная страница — Дашборд
/// </summary>
public class HomeController : Controller
{
    private readonly IReportService _reportService;

    public HomeController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        // Для неавторизованных — редирект на логин
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToAction("Login", "Account");

        var stats = await _reportService.GetDashboardStatsAsync(User.GetUserId(), User.GetRole());
        return View(stats);
    }

    [HttpGet]
    public IActionResult Error(int? code)
    {
        ViewBag.ErrorCode = code ?? 500;
        return View();
    }
}
