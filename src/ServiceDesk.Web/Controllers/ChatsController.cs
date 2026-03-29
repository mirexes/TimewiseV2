using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Контроллер страницы списка чатов и личных чатов
/// </summary>
[RoleAuthorize(UserRole.Technician, UserRole.Engineer, UserRole.ChiefEngineer,
    UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator)]
public class ChatsController : Controller
{
    private readonly IDirectChatService _directChatService;
    private readonly ICompanyChatService _companyChatService;

    public ChatsController(IDirectChatService directChatService, ICompanyChatService companyChatService)
    {
        _directChatService = directChatService;
        _companyChatService = companyChatService;
    }

    /// <summary>Список всех чатов пользователя</summary>
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Чаты";
        ViewBag.UserId = User.GetUserId();
        return View();
    }

    /// <summary>Страница конкретного чата</summary>
    [HttpGet]
    public async Task<IActionResult> Chat(int id)
    {
        var userId = User.GetUserId();
        var isMember = await _companyChatService.IsMemberAsync(id, userId);
        if (!isMember)
            return RedirectToAction("Index");

        ViewData["Title"] = "Чат";
        ViewBag.ChatId = id;
        ViewBag.UserId = userId;
        return View();
    }
}
