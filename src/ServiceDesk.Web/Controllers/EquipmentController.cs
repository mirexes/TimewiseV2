using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Equipment;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;
using Microsoft.AspNetCore.Hosting;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Управление оборудованием
/// </summary>
[RoleAuthorize]
public class EquipmentController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly IClientService _clientService;
    private readonly IWebHostEnvironment _env;

    public EquipmentController(IEquipmentService equipmentService, IClientService clientService, IWebHostEnvironment env)
    {
        _equipmentService = equipmentService;
        _clientService = clientService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _equipmentService.GetPagedAsync(
            User.GetUserId(), User.GetRole(), search, page, pageSize: 24);

        ViewBag.Search = search;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var equipment = await _equipmentService.GetByIdAsync(id, User.GetUserId(), User.GetRole());
        if (equipment is null) return NotFound();

        ViewBag.History = await _equipmentService.GetRepairHistoryAsync(id);
        return View(equipment);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync(User.GetUserId(), User.GetRole());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEquipmentDto dto, IFormFile? photo)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync(User.GetUserId(), User.GetRole());
            return View(dto);
        }

        var id = await _equipmentService.CreateAsync(dto);

        if (photo is { Length: > 0 })
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "equipment");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(photo.FileName);
            var uniqueName = $"{id}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            await _equipmentService.SetPhotoAsync(id, $"/uploads/equipment/{uniqueName}");
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
