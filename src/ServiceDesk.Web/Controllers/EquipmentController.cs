using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Equipment;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Управление оборудованием
/// </summary>
[RoleAuthorize]
public class EquipmentController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly IClientService _clientService;

    public EquipmentController(IEquipmentService equipmentService, IClientService clientService)
    {
        _equipmentService = equipmentService;
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _equipmentService.GetAllAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var equipment = await _equipmentService.GetByIdAsync(id);
        if (equipment is null) return NotFound();

        ViewBag.History = await _equipmentService.GetRepairHistoryAsync(id);
        return View(equipment);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEquipmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync();
            return View(dto);
        }

        var id = await _equipmentService.CreateAsync(dto);
        return RedirectToAction(nameof(Details), new { id });
    }
}
