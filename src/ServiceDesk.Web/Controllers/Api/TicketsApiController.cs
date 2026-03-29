using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Core.DTOs.Equipment;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Enums;
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
    private readonly IEquipmentService _equipmentService;
    private readonly IWebHostEnvironment _env;

    public TicketsApiController(ITicketService ticketService, IEquipmentService equipmentService, IWebHostEnvironment env)
    {
        _ticketService = ticketService;
        _equipmentService = equipmentService;
        _env = env;
    }

    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromForm] int ticketId, [FromForm] int newStatus, [FromForm] string? comment, List<IFormFile>? photos)
    {
        var dto = new UpdateTicketStatusDto
        {
            TicketId = ticketId,
            NewStatus = (TicketStatus)newStatus,
            Comment = comment
        };

        await _ticketService.UpdateStatusAsync(dto, User.GetUserId());

        // Сохраняем фото акта при завершении заявки
        if (photos is { Count: > 0 } &&
            dto.NewStatus is TicketStatus.Completed or TicketStatus.CompletedRemotely)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "completion-photos");
            Directory.CreateDirectory(uploadsDir);

            var photoFiles = new List<TicketAttachmentFile>();
            foreach (var photo in photos)
            {
                var ext = Path.GetExtension(photo.FileName);
                var uniqueName = $"{ticketId}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, uniqueName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await photo.CopyToAsync(stream);

                photoFiles.Add(new TicketAttachmentFile
                {
                    FileName = photo.FileName,
                    FilePath = $"/uploads/completion-photos/{uniqueName}",
                    ContentType = photo.ContentType,
                    FileSize = photo.Length
                });
            }

            await _ticketService.SaveCompletionPhotosAsync(ticketId, photoFiles);
        }

        return Ok(new { success = true });
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignEngineer([FromBody] AssignEngineerRequest request)
    {
        if (!PermissionChecker.CanAssignEngineer(User.GetRole()))
            return Forbid();

        await _ticketService.AssignEngineerAsync(request.TicketId, request.EngineerId, User.GetUserId());
        return Ok(new { success = true });
    }

    [HttpGet("engineers")]
    public async Task<IActionResult> GetEngineers()
    {
        if (!PermissionChecker.CanAssignEngineer(User.GetRole()))
            return Forbid();

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

    /// <summary>Получить оборудование по точке обслуживания</summary>
    [HttpGet("equipment/by-service-point/{servicePointId}")]
    public async Task<IActionResult> GetEquipmentByServicePoint(int servicePointId)
    {
        if (!PermissionChecker.CanEditEquipment(User.GetRole()))
            return Forbid();

        var items = await _equipmentService.GetByServicePointAsync(servicePointId);
        return Ok(items);
    }

    /// <summary>Привязать оборудование к заявке</summary>
    [HttpPost("equipment/assign")]
    public async Task<IActionResult> AssignEquipment([FromBody] AssignEquipmentRequest request)
    {
        if (!PermissionChecker.CanEditEquipment(User.GetRole()))
            return Forbid();

        await _ticketService.UpdateEquipmentAsync(request.TicketId, request.EquipmentId, User.GetUserId());
        return Ok(new { success = true });
    }

    /// <summary>Создать новое оборудование и привязать к заявке</summary>
    [HttpPost("equipment/create-and-assign")]
    public async Task<IActionResult> CreateAndAssignEquipment([FromBody] CreateAndAssignEquipmentRequest request)
    {
        if (!PermissionChecker.CanEditEquipment(User.GetRole()))
            return Forbid();

        var dto = new CreateEquipmentDto
        {
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            ServicePointId = request.ServicePointId
        };

        var equipmentId = await _equipmentService.CreateAsync(dto, User.GetUserId());
        await _ticketService.UpdateEquipmentAsync(request.TicketId, equipmentId, User.GetUserId());
        return Ok(new { success = true, equipmentId });
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

/// <summary>
/// Запрос на привязку оборудования
/// </summary>
public class AssignEquipmentRequest
{
    public int TicketId { get; set; }
    public int? EquipmentId { get; set; }
}

/// <summary>
/// Запрос на создание и привязку нового оборудования
/// </summary>
public class CreateAndAssignEquipmentRequest
{
    public int TicketId { get; set; }
    public int ServicePointId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
}
