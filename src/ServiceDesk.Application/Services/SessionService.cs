using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Управление сессиями пользователей в БД
/// </summary>
public class SessionService : ISessionService
{
    private readonly AppDbContext _db;

    public SessionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserSession>> GetActiveSessionsAsync(int userId)
    {
        return await _db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync();
    }

    public async Task RevokeSessionAsync(int sessionId)
    {
        var session = await _db.UserSessions.FindAsync(sessionId);
        if (session is null) return;

        session.IsRevoked = true;
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllSessionsAsync(int userId)
    {
        await _db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRevoked, true));
    }

    public async Task RevokeOtherSessionsAsync(int userId, string currentSessionToken)
    {
        await _db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.SessionToken != currentSessionToken)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRevoked, true));
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        await _db.UserSessions
            .Where(s => s.ExpiresAt < DateTime.UtcNow || s.IsRevoked)
            .ExecuteDeleteAsync();
    }
}
