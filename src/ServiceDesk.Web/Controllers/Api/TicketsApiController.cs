using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Controllers.Api;

/// <summary>
/// API для AJAX-операций с заявками
/// </summary>
[ApiController]
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

    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] TicketFilterDto filter, int page = 1)
    {
        var result = await _ticketService.GetTicketsAsync(
            filter, page, 20, User.GetUserId(), User.GetRole());
        return Ok(result);
    }
}
