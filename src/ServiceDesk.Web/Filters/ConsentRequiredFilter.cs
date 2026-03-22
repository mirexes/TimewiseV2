using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Filters;

/// <summary>
/// Глобальный фильтр: проверяет наличие согласия на обработку ПД (ФЗ-152).
/// Если согласие не дано — перенаправляет на страницу согласия.
/// </summary>
public class ConsentRequiredFilter : IAsyncActionFilter
{
    private readonly IPersonalDataService _personalDataService;

    public ConsentRequiredFilter(IPersonalDataService personalDataService)
    {
        _personalDataService = personalDataService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        // Проверяем только для авторизованных пользователей
        if (user.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        // Пропускаем контроллеры, которые должны работать без согласия
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();

        var allowedRoutes = new[]
        {
            ("Account", "Logout"),
            ("PersonalData", "Consent"),
            ("PersonalData", "GrantConsent"),
            ("PersonalData", "PrivacyPolicy")
        };

        if (allowedRoutes.Any(r => r.Item1 == controller && r.Item2 == action))
        {
            await next();
            return;
        }

        var userId = user.GetUserId();
        var hasConsent = await _personalDataService.HasActiveConsentAsync(userId, ConsentType.PersonalDataProcessing);

        if (!hasConsent)
        {
            context.Result = new RedirectToActionResult("Consent", "PersonalData", null);
            return;
        }

        await next();
    }
}
