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
    private readonly IWebHostEnvironment _env;

    public ClientsController(IClientService clientService, IWebHostEnvironment env)
    {
        _clientService = clientService;
        _env = env;
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
    public async Task<IActionResult> Create(CreateClientDto dto, IFormFile? ttkPhoto)
    {
        if (!ModelState.IsValid) return View(dto);
        var ttkFilePath = await SaveTtkPhotoAsync(ttkPhoto);
        var id = await _clientService.CreateAsync(dto, ttkFilePath);
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
        ViewBag.TtkFilePath = client.TtkFilePath;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateClientDto dto, IFormFile? ttkPhoto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ClientId = id;
            return View(dto);
        }

        var ttkFilePath = await SaveTtkPhotoAsync(ttkPhoto);
        await _clientService.UpdateAsync(id, dto, ttkFilePath);
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

    /// <summary>Сохранение файла фото ТТК на диск</summary>
    private async Task<string?> SaveTtkPhotoAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "ttk");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var uniqueName = $"ttk_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, uniqueName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/ttk/{uniqueName}";
    }
}
