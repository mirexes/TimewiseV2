using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Users;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Управление пользователями (только Модератор)
/// </summary>
[RoleAuthorize(UserRole.Moderator)]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IClientService _clientService;

    public UsersController(IUserService userService, IClientService clientService)
    {
        _userService = userService;
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(UserFilterDto filter)
    {
        var result = await _userService.GetAllAsync(filter);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = await _clientService.GetClientsForSelectAsync();
        return View(new CreateUserDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!await _userService.IsPhoneUniqueAsync(dto.Phone))
            ModelState.AddModelError(nameof(dto.Phone), "Пользователь с таким телефоном уже существует");

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = await _clientService.GetClientsForSelectAsync();
            return View(dto);
        }

        var id = await _userService.CreateAsync(dto);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();

        var dto = new CreateUserDto
        {
            LastName = user.LastName,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            Phone = user.Phone,
            Email = user.Email,
            Role = user.Role,
            Company = user.Company,
            ClientId = user.ClientId,
            IsActive = user.IsActive
        };

        ViewBag.UserId = id;
        ViewBag.UserName = user.FullName;
        ViewBag.Clients = await _clientService.GetClientsForSelectAsync();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateUserDto dto)
    {
        if (!await _userService.IsPhoneUniqueAsync(dto.Phone, id))
            ModelState.AddModelError(nameof(dto.Phone), "Пользователь с таким телефоном уже существует");

        if (!ModelState.IsValid)
        {
            ViewBag.UserId = id;
            ViewBag.Clients = await _clientService.GetClientsForSelectAsync();
            return View(dto);
        }

        await _userService.UpdateAsync(id, dto);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _userService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Details), new { id });
    }
}
