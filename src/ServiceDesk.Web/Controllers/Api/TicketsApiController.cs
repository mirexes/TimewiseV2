using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Controllers.Api;

/// <summary>
/// API для AJAX-операций с заявками
/// </summary>
[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsApiController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsApiController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateTicketStatusDto dto)
    {
        await _ticketService.UpdateStatusAsync(dto, User.GetUserId());
        return Ok(new { success = true });
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignEngineer([FromBody] AssignEngineerRequest request)
    {
        if (!PermissionChecker.CanAssignEngineer(User.GetRole()))
            return StatusCode(403, new { error = "Недостаточно прав" });

        await _ticketService.AssignEngineerAsync(request.TicketId, request.EngineerId, User.GetUserId());
        return Ok(new { success = true });
    }

    [HttpGet("engineers")]
    public async Task<IActionResult> GetEngineers()
    {
        if (!PermissionChecker.CanAssignEngineer(User.GetRole()))
            return StatusCode(403, new { error = "Недостаточно прав" });

        var engineers = await _ticketService.GetEngineersAsync();
        return Ok(engineers);
    }

    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] TicketFilterDto filter, int page = 1)
    {
        var result = await _ticketService.GetTicketsAsync(
            filter, page, 20, User.GetUserId(), User.GetRole());
        return Ok(result);
    }
}

/// <summary>
/// Запрос на назначение специалиста
/// </summary>
public class AssignEngineerRequest
{
    public int TicketId { get; set; }
    public int EngineerId { get; set; }
}
