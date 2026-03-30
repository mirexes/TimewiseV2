using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Filters;

/// <summary>
/// Глобальный фильтр: проверяет, что пользователь активен и сессия валидна.
/// При деактивации — отзывает все сессии и принудительно разлогинивает.
/// </summary>
public class DeactivatedUserFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;
    private readonly ISessionService _sessionService;

    public DeactivatedUserFilter(AppDbContext db, ISessionService sessionService)
    {
        _db = db;
        _sessionService = sessionService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var principal = context.HttpContext.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        // Пропускаем Logout, чтобы не зациклиться
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        if (controller == "Account" && (action == "Logout" || action == "Login"))
        {
            await next();
            return;
        }

        var userId = principal.GetUserId();
        var user = await _db.Users.FindAsync(userId);

        // Пользователь удалён или деактивирован — отзываем все сессии и разлогиниваем
        if (user is null || !user.IsActive)
        {
            await _sessionService.RevokeAllSessionsAsync(userId);
            await context.HttpContext.SignOutAsync("Cookies");
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Проверяем метку безопасности (изменилась при смене роли/деактивации)
        var stampClaim = principal.FindFirst("SecurityStamp")?.Value;
        if (stampClaim != null && stampClaim != user.SecurityStamp)
        {
            await _sessionService.RevokeAllSessionsAsync(userId);
            await context.HttpContext.SignOutAsync("Cookies");
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        await next();
    }
}
