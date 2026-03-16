using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Chat;
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

    public ChatApiController(IChatService chatService)
    {
        _chatService = chatService;
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

    [HttpPost("{ticketId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int ticketId)
    {
        await _chatService.MarkAsReadAsync(ticketId, User.GetUserId());
        return Ok(new { success = true });
    }
}
