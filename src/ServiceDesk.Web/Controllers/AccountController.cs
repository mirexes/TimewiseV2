using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.Auth;
using ServiceDesk.Core.Interfaces.Services;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Авторизация по SMS-коду
/// </summary>
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated ?? false)
            return RedirectToAction("Dashboard", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(SendCodeDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _authService.SendCodeAsync(dto);
        if (!result)
        {
            ModelState.AddModelError("", "Слишком частые запросы. Попробуйте позже");
            return View(dto);
        }

        return RedirectToAction(nameof(VerifyCode), new { phone = dto.Phone });
    }

    [HttpGet]
    public IActionResult VerifyCode(string phone)
    {
        return View(new VerifyCodeDto { Phone = phone });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(VerifyCodeDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _authService.VerifyCodeAsync(dto);
        if (user is null)
        {
            ModelState.AddModelError("", "Неверный код или превышено количество попыток");
            return View(dto);
        }

        // Создаём Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Phone),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("FullName", user.FullName)
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);
        return RedirectToAction("Dashboard", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
