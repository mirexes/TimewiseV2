using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.Entities;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Web.Auth;

/// <summary>
/// Хранилище аутентификационных тикетов в БД.
/// Кука содержит только SessionToken (ключ), данные сессии — в таблице UserSessions.
/// </summary>
public class DatabaseTicketStore : ITicketStore
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseTicketStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

        var sessionToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var userId = GetUserId(ticket);
        var httpContext = httpContextAccessor.HttpContext;

        var session = new UserSession
        {
            SessionToken = sessionToken,
            TicketData = SerializeTicket(ticket),
            ExpiresAt = ticket.Properties.ExpiresUtc?.UtcDateTime ?? DateTime.UtcNow.AddDays(365),
            LastActivityAt = DateTime.UtcNow,
            UserId = userId,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua
                ? (ua.Length > 512 ? ua[..512] : ua)
                : null
        };

        db.UserSessions.Add(session);
        await db.SaveChangesAsync();

        return sessionToken;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var session = await db.UserSessions.FirstOrDefaultAsync(s => s.SessionToken == key);
        if (session is null) return;

        session.TicketData = SerializeTicket(ticket);
        session.ExpiresAt = ticket.Properties.ExpiresUtc?.UtcDateTime ?? DateTime.UtcNow.AddDays(365);
        session.LastActivityAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var session = await db.UserSessions.FirstOrDefaultAsync(s => s.SessionToken == key);

        // Сессия не найдена, отозвана или просрочена
        if (session is null || session.IsRevoked || session.ExpiresAt < DateTime.UtcNow)
            return null;

        // Обновляем время последней активности
        session.LastActivityAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return DeserializeTicket(session.TicketData);
    }

    public async Task RemoveAsync(string key)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var session = await db.UserSessions.FirstOrDefaultAsync(s => s.SessionToken == key);
        if (session is null) return;

        db.UserSessions.Remove(session);
        await db.SaveChangesAsync();
    }

    private static int GetUserId(AuthenticationTicket ticket)
    {
        var userIdClaim = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : 0;
    }

    private static byte[] SerializeTicket(AuthenticationTicket ticket)
    {
        return TicketSerializer.Default.Serialize(ticket);
    }

    private static AuthenticationTicket? DeserializeTicket(byte[] data)
    {
        return TicketSerializer.Default.Deserialize(data);
    }
}
