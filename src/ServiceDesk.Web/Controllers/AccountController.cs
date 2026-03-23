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
    private readonly IWebHostEnvironment _env;

    public AccountController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
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
    public async Task<IActionResult> VerifyCode(string phone)
    {
        // В режиме разработки показываем код на странице
        if (true)//_env.IsDevelopment())
        {
            var code = await _authService.GetSmsCodeAsync(phone);
            ViewBag.DevSmsCode = code;
        }

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
            new("FullName", user.FullName),
            new("SecurityStamp", user.SecurityStamp)
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
