using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Controllers.Api;

/// <summary>
/// API для чата заявки
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatApiController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IWebHostEnvironment _env;

    public ChatApiController(IChatService chatService, IWebHostEnvironment env)
    {
        _chatService = chatService;
        _env = env;
    }

    [HttpGet("{ticketId:int}")]
    public async Task<IActionResult> GetMessages(int ticketId)
    {
        var messages = await _chatService.GetMessagesAsync(ticketId);
        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
    {
        var message = await _chatService.SendMessageAsync(dto, User.GetUserId());
        return Ok(message);
    }

    /// <summary>
    /// Отправка сообщения с файлами (multipart/form-data)
    /// </summary>
    [HttpPost("send-with-files")]
    public async Task<IActionResult> SendWithFiles([FromForm] string? text, [FromForm] int ticketId, [FromForm] List<IFormFile> files)
    {
        var messageText = text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(messageText) && (files == null || files.Count == 0))
            return BadRequest(new { error = "Введите сообщение или прикрепите файл" });

        var savedFiles = new List<TicketAttachmentFile>();

        if (files is { Count: > 0 })
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "tickets");
            Directory.CreateDirectory(uploadsDir);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName);
                var uniqueName = $"{ticketId}_{Guid.NewGuid():N}{ext}";
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
        }

        var message = savedFiles.Count > 0
            ? await _chatService.SendMessageWithAttachmentsAsync(ticketId, User.GetUserId(), messageText, savedFiles)
            : await _chatService.SendMessageAsync(new SendMessageDto { Text = messageText, TicketId = ticketId }, User.GetUserId());

        return Ok(message);
    }

    [HttpPost("{ticketId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int ticketId)
    {
        await _chatService.MarkAsReadAsync(ticketId, User.GetUserId());
        return Ok(new { success = true });
    }
}
