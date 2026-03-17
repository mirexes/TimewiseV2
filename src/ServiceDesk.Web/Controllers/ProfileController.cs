using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Users;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Редактирование собственного профиля пользователем
/// </summary>
[RoleAuthorize]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _env;

    public ProfileController(IUserService userService, IWebHostEnvironment env)
    {
        _userService = userService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userService.GetByIdAsync(User.GetUserId());
        if (user is null) return NotFound();

        var dto = new UpdateProfileDto
        {
            LastName = user.LastName,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            Email = user.Email,
            Company = user.Company
        };

        ViewBag.AvatarUrl = user.AvatarUrl;
        ViewBag.RoleName = GetRoleName(user.Role);
        ViewBag.Phone = user.Phone;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProfileDto dto, IFormFile? avatar, bool removeAvatar = false)
    {
        var userId = User.GetUserId();

        if (!ModelState.IsValid)
        {
            var user = await _userService.GetByIdAsync(userId);
            ViewBag.AvatarUrl = user?.AvatarUrl;
            ViewBag.RoleName = GetRoleName(user!.Role);
            ViewBag.Phone = user.Phone;
            return View(dto);
        }

        await _userService.UpdateProfileAsync(userId, dto);

        // Обработка аватара
        if (removeAvatar)
        {
            await DeleteAvatarFileAsync(userId);
            await _userService.UpdateAvatarAsync(userId, null);
        }
        else if (avatar is { Length: > 0 })
        {
            await DeleteAvatarFileAsync(userId);
            var avatarUrl = await SaveAvatarAsync(userId, avatar);
            await _userService.UpdateAvatarAsync(userId, avatarUrl);
        }

        return RedirectToAction(nameof(Edit));
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

    private static string GetRoleName(Core.Enums.UserRole role) => role switch
    {
        Core.Enums.UserRole.Technician => "Техник",
        Core.Enums.UserRole.Engineer => "Инженер",
        Core.Enums.UserRole.ChiefEngineer => "Главный инженер",
        Core.Enums.UserRole.Logist => "Логист",
        Core.Enums.UserRole.ManagerTimewise => "Менеджер Timewise",
        Core.Enums.UserRole.ManagerClient => "Менеджер клиента",
        Core.Enums.UserRole.Client => "Клиент",
        Core.Enums.UserRole.Moderator => "Модератор",
        _ => role.ToString()
    };
}
