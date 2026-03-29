using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.AuditLogs;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис журналирования действий
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(AuditAction action, string entityType, int entityId,
        string? oldValue, string? newValue, int userId)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            UserId = userId
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResultDto<AuditLogListDto>> GetLogsAsync(AuditLogFilterDto filter)
    {
        var query = _db.AuditLogs
            .Include(l => l.User)
            .AsQueryable();

        // Фильтр по действию
        if (filter.Action.HasValue)
            query = query.Where(l => l.Action == filter.Action.Value);

        // Фильтр по типу сущности
        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(l => l.EntityType == filter.EntityType);

        // Фильтр по дате
        if (filter.DateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(l => l.CreatedAt < filter.DateTo.Value.AddDays(1));

        // Поиск по имени пользователя, IP или значениям
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(l =>
                (l.User.LastName + " " + l.User.FirstName).ToLower().Contains(search) ||
                (l.IpAddress != null && l.IpAddress.Contains(search)) ||
                (l.OldValue != null && l.OldValue.ToLower().Contains(search)) ||
                (l.NewValue != null && l.NewValue.ToLower().Contains(search)) ||
                l.EntityId.ToString().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var items = logs.Select(l => l.ToListDto()).ToList();

        return new PagedResultDto<AuditLogListDto>(items, totalCount, filter.Page, filter.PageSize);
    }
}
