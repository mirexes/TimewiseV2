using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers.Api;

/// <summary>
/// API группового чата компании
/// </summary>
[ApiController]
[Route("api/company-chat")]
[RoleAuthorize(UserRole.Technician, UserRole.Engineer, UserRole.ChiefEngineer,
    UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator)]
public class CompanyChatApiController : ControllerBase
{
    private readonly ICompanyChatService _companyChatService;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _db;

    public CompanyChatApiController(ICompanyChatService companyChatService, IWebHostEnvironment env, AppDbContext db)
    {
        _companyChatService = companyChatService;
        _env = env;
        _db = db;
    }

    [HttpGet("{chatId:int}/messages")]
    public async Task<IActionResult> GetMessages(int chatId)
    {
        var userId = User.GetUserId();
        if (!await _companyChatService.IsMemberAsync(chatId, userId))
            return Forbid();

        var messages = await _companyChatService.GetMessagesAsync(chatId);
        return Ok(messages);
    }

    [HttpPost("{chatId:int}/send")]
    public async Task<IActionResult> Send(int chatId, [FromBody] CompanyChatSendDto dto)
    {
        var userId = User.GetUserId();
        if (!await _companyChatService.IsMemberAsync(chatId, userId))
            return Forbid();

        var message = await _companyChatService.SendMessageAsync(chatId, userId, dto.Text, dto.ReplyToMessageId);
        return Ok(message);
    }

    [HttpPost("{chatId:int}/send-with-files")]
    public async Task<IActionResult> SendWithFiles(int chatId, [FromForm] string? text, [FromForm] List<IFormFile> files, [FromForm] int? replyToMessageId)
    {
        var userId = User.GetUserId();
        if (!await _companyChatService.IsMemberAsync(chatId, userId))
            return Forbid();

        var messageText = text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(messageText) && (files == null || files.Count == 0))
            return BadRequest(new { error = "Введите сообщение или прикрепите файл" });

        var savedFiles = new List<TicketAttachmentFile>();

        if (files is { Count: > 0 })
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "company-chat");
            Directory.CreateDirectory(uploadsDir);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName);
                var uniqueName = $"cc_{chatId}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                savedFiles.Add(new TicketAttachmentFile
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/company-chat/{uniqueName}",
                    ContentType = file.ContentType,
                    FileSize = file.Length
                });
            }
        }

        var message = savedFiles.Count > 0
            ? await _companyChatService.SendMessageWithAttachmentsAsync(chatId, userId, messageText, savedFiles, replyToMessageId)
            : await _companyChatService.SendMessageAsync(chatId, userId, messageText, replyToMessageId);

        return Ok(message);
    }

    [HttpPost("{chatId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int chatId)
    {
        var userId = User.GetUserId();
        if (!await _companyChatService.IsMemberAsync(chatId, userId))
            return Forbid();

        await _companyChatService.MarkAsReadAsync(chatId, userId);
        return Ok(new { success = true });
    }

    [HttpGet("{chatId:int}/members")]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> GetMembers(int chatId)
    {
        var members = await _companyChatService.GetMembersAsync(chatId);
        return Ok(members);
    }

    [HttpPost("{chatId:int}/members/{userId:int}")]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> AddMember(int chatId, int userId)
    {
        await _companyChatService.AddMemberAsync(chatId, userId);
        return Ok(new { success = true });
    }

    [HttpDelete("{chatId:int}/members/{userId:int}")]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> RemoveMember(int chatId, int userId)
    {
        await _companyChatService.RemoveMemberAsync(chatId, userId);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Список сотрудников, которых можно добавить в чат (не клиенты, не менеджеры клиентов, ещё не в чате)
    /// </summary>
    [HttpGet("{chatId:int}/available-users")]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> GetAvailableUsers(int chatId)
    {
        var existingMemberIds = await _db.CompanyChatMembers
            .Where(m => m.CompanyChatId == chatId)
            .Select(m => m.UserId)
            .ToListAsync();

        // Только сотрудники компании (без клиентов и менеджеров клиентов)
        var employeeRoles = new[]
        {
            UserRole.Technician, UserRole.Engineer, UserRole.ChiefEngineer,
            UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator
        };

        var users = await _db.Users
            .Where(u => u.IsActive && employeeRoles.Contains(u.Role) && !existingMemberIds.Contains(u.Id))
            .OrderBy(u => u.LastName)
            .Select(u => new { u.Id, FullName = u.LastName + " " + u.FirstName, u.Role })
            .ToListAsync();

        var result = users.Select(u => new { u.Id, u.FullName, RoleName = GetRoleName(u.Role) });
        return Ok(result);
    }

    private static string GetRoleName(UserRole role) => role switch
    {
        UserRole.Technician => "Техник",
        UserRole.Engineer => "Инженер",
        UserRole.ChiefEngineer => "Главный инженер",
        UserRole.Logist => "Логист",
        UserRole.ManagerTimewise => "Менеджер TW",
        UserRole.Moderator => "Модератор",
        _ => role.ToString()
    };
}

/// <summary>
/// DTO для отправки сообщения в групповой чат
/// </summary>
public class CompanyChatSendDto
{
    public string Text { get; set; } = string.Empty;
    public int? ReplyToMessageId { get; set; }
}
