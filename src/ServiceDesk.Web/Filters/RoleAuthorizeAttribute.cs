using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceDesk.Core.Enums;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Filters;

/// <summary>
/// Атрибут авторизации по ролям.
/// Без параметров — доступ для всех авторизованных.
/// С ролями — доступ только для указанных ролей.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole[] _roles;

    public RoleAuthorizeAttribute(params UserRole[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Проверяем аутентификацию
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Если роли не указаны — доступ для всех авторизованных
        if (_roles.Length == 0)
            return;

        var userRole = context.HttpContext.User.GetRole();
        if (!_roles.Contains(userRole))
        {
            context.Result = new ForbidResult();
        }
    }
}
