using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Core.DTOs.Reports;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис отчётов и статистики
/// </summary>
public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Возвращает базовый запрос заявок, отфильтрованный по роли пользователя
    /// </summary>
    private IQueryable<Ticket> GetTicketsForUser(int currentUserId, UserRole currentUserRole)
    {
        var query = _db.Tickets.AsQueryable();

        if (PermissionChecker.CanViewAllTickets(currentUserRole))
            return query;

        return currentUserRole switch
        {
            UserRole.Technician or UserRole.Engineer =>
                query.Where(t => t.AssignedEngineerId == currentUserId),
            UserRole.Client =>
                query.Where(t => t.CreatedByUserId == currentUserId),
            UserRole.ManagerClient =>
                query.Where(t => t.ServicePoint.ClientId ==
                    _db.Users.Where(u => u.Id == currentUserId)
                        .Select(u => u.ClientId).FirstOrDefault()),
            _ => query
        };
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(int currentUserId, UserRole currentUserRole)
    {
        var tickets = GetTicketsForUser(currentUserId, currentUserRole);

        var stats = new DashboardStatsDto
        {
            TotalTickets = await tickets.CountAsync(),
            NewTickets = await tickets.CountAsync(t => t.Status == TicketStatus.New),
            InProgressTickets = await tickets.CountAsync(t => t.Status == TicketStatus.InProgress),
            CompletedTickets = await tickets.CountAsync(t =>
                t.Status == TicketStatus.Completed || t.Status == TicketStatus.CompletedRemotely),
            OverdueTickets = await tickets.CountAsync(t =>
                t.Deadline.HasValue && t.Deadline < DateTime.UtcNow &&
                t.Status != TicketStatus.Completed && t.Status != TicketStatus.CompletedRemotely &&
                t.Status != TicketStatus.Cancelled),
            TotalEquipment = await _db.Equipment.CountAsync(e => e.IsActive && e.ClientId== currentUserId),
            TotalClients = await _db.Clients.CountAsync(c => c.IsActive),
            TotalEngineers = await _db.Users.CountAsync(u =>
                u.Role == UserRole.Engineer || u.Role == UserRole.Technician ||
                u.Role == UserRole.ChiefEngineer)
        };
        if(currentUserRole!=UserRole.Moderator )
        {
            stats.TotalClients = 0;
            stats.TotalEngineers = 0;
        }

        // Заявки по статусам для графика
        var statusGroups = await tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var g in statusGroups)
            stats.TicketsByStatus[g.Status.ToString()] = g.Count;

        // Заявки за последние 30 дней
        var from = DateTime.UtcNow.AddDays(-30);
        var dailyGroups = await tickets
            .Where(t => t.CreatedAt >= from)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync();

        foreach (var g in dailyGroups)
            stats.TicketsByDay[g.Date.ToString("dd.MM")] = g.Count;

        return stats;
    }

    public async Task<IEnumerable<EngineerStatsDto>> GetEngineerStatsAsync(ReportFilterDto filter)
    {
        var query = _db.Tickets.AsQueryable();

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value.Date);
        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt < filter.DateTo.Value.Date.AddDays(1));

        // Группировка только по идентификатору специалиста (без навигационных свойств в ключе)
        var stats = await query
            .Where(t => t.AssignedEngineerId.HasValue)
            .GroupBy(t => t.AssignedEngineerId!.Value)
            .Select(g => new EngineerStatsDto
            {
                EngineerId = g.Key,
                TotalTickets = g.Count(),
                CompletedTickets = g.Count(t =>
                    t.Status == TicketStatus.Completed || t.Status == TicketStatus.CompletedRemotely),
                InProgressTickets = g.Count(t => t.Status == TicketStatus.InProgress)
            })
            .ToListAsync();

        // Загрузка имён специалистов отдельным запросом
        var engineerIds = stats.Select(s => s.EngineerId).ToList();
        var engineerNames = await _db.Users
            .Where(u => engineerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.LastName, u.FirstName })
            .ToDictionaryAsync(u => u.Id, u => u.LastName + " " + u.FirstName);

        foreach (var s in stats)
        {
            s.EngineerName = engineerNames.GetValueOrDefault(s.EngineerId, "Неизвестный");
        }

        return stats;
    }
}
