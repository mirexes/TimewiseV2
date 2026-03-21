using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Application.Helpers;
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
    private readonly IChatService _chatService;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public TicketsController(
        ITicketService ticketService,
        IClientService clientService,
        IChatService chatService,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        _ticketService = ticketService;
        _clientService = clientService;
        _chatService = chatService;
        _env = env;
        _config = config;
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
        UserRole.ManagerTimewise, UserRole.ManagerClient, UserRole.Moderator, UserRole.Client)]
    public async Task<IActionResult> Create()
    {
        ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync(User.GetUserId(), User.GetRole());
        ViewBag.YandexMapsApiKey = _config["YandexMaps:ApiKey"] ?? "";

        // Специалисты доступны всем, кроме клиента и менеджера клиента
        var role = User.GetRole();
        if (role is not UserRole.Client and not UserRole.ManagerClient)
            ViewBag.Engineers = await _ticketService.GetEngineersAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(UserRole.Engineer, UserRole.ChiefEngineer, UserRole.Logist,
        UserRole.ManagerTimewise, UserRole.ManagerClient, UserRole.Moderator, UserRole.Client)]
    public async Task<IActionResult> Create(CreateTicketDto dto, List<IFormFile>? attachments)
    {
        // ServicePointId теперь nullable — убираем ложную ошибку биндинга
        ModelState.Remove("ServicePointId");

        // Валидация: нужна либо существующая точка, либо новый адрес
        if (!dto.ServicePointId.HasValue && string.IsNullOrWhiteSpace(dto.NewAddress))
            ModelState.AddModelError("ServicePointId", "Выберите точку обслуживания или укажите новый адрес на карте");

        if (!ModelState.IsValid)
        {
            ViewBag.ServicePoints = await _clientService.GetServicePointsForSelectAsync(User.GetUserId(), User.GetRole());
            ViewBag.YandexMapsApiKey = _config["YandexMaps:ApiKey"] ?? "";
            var role = User.GetRole();
            if (role is not UserRole.Client and not UserRole.ManagerClient)
                ViewBag.Engineers = await _ticketService.GetEngineersAsync();
            return View(dto);
        }

        var id = await _ticketService.CreateAsync(dto, User.GetUserId());

        // Сохранение вложений (фото/видео)
        if (attachments is { Count: > 0 })
        {
            var savedFiles = new List<TicketAttachmentFile>();
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "tickets");
            Directory.CreateDirectory(uploadsDir);

            foreach (var file in attachments)
            {
                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName);
                var uniqueName = $"{id}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                savedFiles.Add(new TicketAttachmentFile
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/tickets/{uniqueName}",
                    ContentType = file.ContentType,
                    FileSize = file.Length
                });
            }

            if (savedFiles.Count > 0)
            {
                await _ticketService.SaveAttachmentsAsync(id, savedFiles);

                // Создаём сообщение в чате с вложениями
                await _chatService.AddMessageWithAttachmentsAsync(
                    id, User.GetUserId(), "Прикреплённые файлы при создании заявки", savedFiles);
            }
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
