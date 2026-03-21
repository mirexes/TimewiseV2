using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Просмотр фото ТТК настроек компании — доступно членам компании и административным ролям
/// </summary>
[RoleAuthorize(UserRole.Client, UserRole.ManagerClient,
    UserRole.ChiefEngineer, UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator)]
public class ClientTtkController : Controller
{
    private readonly IClientService _clientService;
    private readonly IUserService _userService;

    public ClientTtkController(IClientService clientService, IUserService userService)
    {
        _clientService = clientService;
        _userService = userService;
    }

    /// <summary>
    /// Просмотр фото ТТК настроек компании
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        var role = User.GetRole();
        var userId = User.GetUserId();

        // Клиент и Менеджер клиента могут смотреть только ТТК своей компании
        if (role is UserRole.Client or UserRole.ManagerClient)
        {
            var currentUser = await _userService.GetByIdAsync(userId);
            if (currentUser?.ClientId != id)
                return Forbid();
        }

        var client = await _clientService.GetByIdAsync(id);
        if (client is null) return NotFound();
        if (string.IsNullOrEmpty(client.TtkFilePath)) return NotFound();

        return View("~/Views/Clients/TtkPhoto.cshtml", client);
    }
}
