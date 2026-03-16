using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Управление заявками
/// </summary>
[RoleAuthorize]
public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly IClientService _clientService;

    public TicketsController(ITicketService ticketService, IClientService clientService)
    {
        _ticketService = ticketService;
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] TicketFilterDto filter, int page = 1)
    {
        var result = await _ticketService.GetTicketsAsync(
            filter, page, pageSize: 20, User.GetUserId(), User.GetRole());
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _ticketService.GetByIdAsync(id, User.GetUserId(), User.GetRole());
        if (ticket is null) return NotFound();
        return View(ticket);
    }

    [HttpGet]
    [RoleAuthorize(UserRole.Engineer, UserRole.ChiefEngineer, UserRole.Logist,
        UserRole.ManagerTimewise, UserRole.ManagerClient, UserRole.Moderator)]
    public async Task<IActionResult> Create()
    {
        ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync(User.GetUserId(), User.GetRole());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(UserRole.Engineer, UserRole.ChiefEngineer, UserRole.Logist,
        UserRole.ManagerTimewise, UserRole.ManagerClient, UserRole.Moderator)]
    public async Task<IActionResult> Create(CreateTicketDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync(User.GetUserId(), User.GetRole());
            return View(dto);
        }

        var id = await _ticketService.CreateAsync(dto, User.GetUserId());
        return RedirectToAction(nameof(Details), new { id });
    }
}
