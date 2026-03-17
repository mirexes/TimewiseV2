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
    private readonly IWebHostEnvironment _env;

    public UsersController(IUserService userService, IClientService clientService, IWebHostEnvironment env)
    {
        _userService = userService;
        _clientService = clientService;
        _env = env;
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
        ViewBag.AvatarUrl = user.AvatarUrl;
        ViewBag.Clients = await _clientService.GetClientsForSelectAsync();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateUserDto dto, IFormFile? avatar, bool removeAvatar = false)
    {
        if (!await _userService.IsPhoneUniqueAsync(dto.Phone, id))
            ModelState.AddModelError(nameof(dto.Phone), "Пользователь с таким телефоном уже существует");

        if (!ModelState.IsValid)
        {
            ViewBag.UserId = id;
            ViewBag.AvatarUrl = (await _userService.GetByIdAsync(id))?.AvatarUrl;
            ViewBag.Clients = await _clientService.GetClientsForSelectAsync();
            return View(dto);
        }

        await _userService.UpdateAsync(id, dto);

        // Обработка аватара
        if (removeAvatar)
        {
            await DeleteAvatarFileAsync(id);
            await _userService.UpdateAvatarAsync(id, null);
        }
        else if (avatar is { Length: > 0 })
        {
            await DeleteAvatarFileAsync(id);
            var avatarUrl = await SaveAvatarAsync(id, avatar);
            await _userService.UpdateAvatarAsync(id, avatarUrl);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Сохранение файла аватара на диск
    /// </summary>
    private async Task<string> SaveAvatarAsync(int userId, IFormFile file)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{userId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/avatars/{fileName}";
    }

    /// <summary>
    /// Удаление старого файла аватара
    /// </summary>
    private async Task DeleteAvatarFileAsync(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (string.IsNullOrEmpty(user?.AvatarUrl)) return;

        var filePath = Path.Combine(_env.WebRootPath, user.AvatarUrl.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _userService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Details), new { id });
    }
}
