using System.Security.Claims;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Web.Extensions;

/// <summary>
/// Расширения для работы с ClaimsPrincipal
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Получить ID текущего пользователя</summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? int.Parse(claim.Value) : 0;
    }

    /// <summary>Получить роль текущего пользователя</summary>
    public static UserRole GetRole(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.Role);
        return claim is not null && Enum.TryParse<UserRole>(claim.Value, out var role)
            ? role
            : UserRole.Client;
    }

    /// <summary>Получить ФИО текущего пользователя</summary>
    public static string GetFullName(this ClaimsPrincipal user)
    {
        return user.FindFirst("FullName")?.Value ?? "Пользователь";
    }
}
