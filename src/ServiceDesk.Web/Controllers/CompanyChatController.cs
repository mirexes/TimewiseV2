using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Контроллер страницы группового чата компании
/// </summary>
[RoleAuthorize(UserRole.Technician, UserRole.Engineer, UserRole.ChiefEngineer,
    UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator)]
public class CompanyChatController : Controller
{
    private readonly ICompanyChatService _companyChatService;

    public CompanyChatController(ICompanyChatService companyChatService)
    {
        _companyChatService = companyChatService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var chatId = await _companyChatService.GetOrCreateChatAsync();
        var userId = User.GetUserId();
        var isMember = await _companyChatService.IsMemberAsync(chatId, userId);

        if (!isMember)
            return View("NotMember");

        ViewData["Title"] = "Чат компании";
        ViewBag.ChatId = chatId;
        ViewBag.UserId = userId;
        ViewBag.IsModerator = User.GetRole() == UserRole.Moderator;

        return View();
    }

    /// <summary>
    /// Управление участниками чата (только модератор)
    /// </summary>
    [HttpGet]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> Members()
    {
        var chatId = await _companyChatService.GetOrCreateChatAsync();
        var members = await _companyChatService.GetMembersAsync(chatId);

        ViewData["Title"] = "Участники чата компании";
        ViewBag.ChatId = chatId;

        return View(members);
    }
}
