using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Clients;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Управление клиентами (организациями)
/// </summary>
[RoleAuthorize(UserRole.ChiefEngineer, UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator)]
public class ClientsController : Controller
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ClientFilterDto filter)
    {
        var result = await _clientService.GetAllAsync(filter);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client is null) return NotFound();
        return View(client);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClientDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var id = await _clientService.CreateAsync(dto);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client is null) return NotFound();

        var dto = new CreateClientDto
        {
            Name = client.Name,
            Inn = client.Inn,
            Network = client.Network,
            Phone = client.Phone,
            Email = client.Email,
            LegalAddress = client.LegalAddress,
            IsActive = client.IsActive
        };

        ViewBag.ClientId = id;
        ViewBag.ClientName = client.Name;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ClientId = id;
            return View(dto);
        }

        await _clientService.UpdateAsync(id, dto);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _clientService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddContactPerson(int id, CreateContactPersonDto dto)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id });
        await _clientService.AddContactPersonAsync(id, dto);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveContactPerson(int id, int contactPersonId)
    {
        await _clientService.RemoveContactPersonAsync(contactPersonId);
        return RedirectToAction(nameof(Details), new { id });
    }
}
