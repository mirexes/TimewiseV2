using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers.Api;

/// <summary>
/// API для списка чатов и создания личных чатов
/// </summary>
[ApiController]
[Route("api/chats")]
[RoleAuthorize(UserRole.Technician, UserRole.Engineer, UserRole.ChiefEngineer,
    UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator)]
public class ChatsApiController : ControllerBase
{
    private readonly IDirectChatService _directChatService;

    public ChatsApiController(IDirectChatService directChatService)
    {
        _directChatService = directChatService;
    }

    /// <summary>Получить список всех чатов текущего пользователя</summary>
    [HttpGet]
    public async Task<IActionResult> GetChats()
    {
        var userId = User.GetUserId();
        var chats = await _directChatService.GetUserChatsAsync(userId);
        return Ok(chats);
    }

    /// <summary>Создать или получить существующий личный чат с пользователем</summary>
    [HttpPost("direct/{targetUserId:int}")]
    public async Task<IActionResult> CreateDirectChat(int targetUserId)
    {
        var userId = User.GetUserId();
        if (userId == targetUserId)
            return BadRequest(new { error = "Нельзя создать чат с самим собой" });

        var chatId = await _directChatService.GetOrCreateDirectChatAsync(userId, targetUserId);
        return Ok(new { chatId });
    }

    /// <summary>Список сотрудников, с которыми можно начать чат</summary>
    [HttpGet("employees")]
    public async Task<IActionResult> GetAvailableEmployees()
    {
        var userId = User.GetUserId();
        var employees = await _directChatService.GetAvailableEmployeesAsync(userId);
        return Ok(employees);
    }
}
