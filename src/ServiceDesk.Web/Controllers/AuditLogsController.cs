using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.AuditLogs;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Контроллер журнала аудита (только для модератора)
/// </summary>
[RoleAuthorize(UserRole.Moderator)]
public class AuditLogsController : Controller
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] AuditLogFilterDto filter)
    {
        var result = await _auditService.GetLogsAsync(filter);
        ViewBag.Filter = filter;
        return View(result);
    }
}
