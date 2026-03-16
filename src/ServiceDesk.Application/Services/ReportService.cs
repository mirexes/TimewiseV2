using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.DTOs.Reports;
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

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var stats = new DashboardStatsDto
        {
            TotalTickets = await _db.Tickets.CountAsync(),
            NewTickets = await _db.Tickets.CountAsync(t => t.Status == TicketStatus.New),
            InProgressTickets = await _db.Tickets.CountAsync(t => t.Status == TicketStatus.InProgress),
            CompletedTickets = await _db.Tickets.CountAsync(t =>
                t.Status == TicketStatus.Completed || t.Status == TicketStatus.CompletedRemotely),
            OverdueTickets = await _db.Tickets.CountAsync(t =>
                t.Deadline.HasValue && t.Deadline < DateTime.UtcNow &&
                t.Status != TicketStatus.Completed && t.Status != TicketStatus.CompletedRemotely &&
                t.Status != TicketStatus.Cancelled),
            TotalEquipment = await _db.Equipment.CountAsync(e => e.IsActive),
            TotalClients = await _db.Clients.CountAsync(c => c.IsActive),
            TotalEngineers = await _db.Users.CountAsync(u =>
                u.Role == UserRole.Engineer || u.Role == UserRole.Technician ||
                u.Role == UserRole.ChiefEngineer)
        };

        // Заявки по статусам для графика
        var statusGroups = await _db.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var g in statusGroups)
            stats.TicketsByStatus[g.Status.ToString()] = g.Count;

        // Заявки за последние 30 дней
        var from = DateTime.UtcNow.AddDays(-30);
        var dailyGroups = await _db.Tickets
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
            query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo.Value);

        var stats = await query
            .Where(t => t.AssignedEngineerId.HasValue)
            .GroupBy(t => new { t.AssignedEngineerId, t.AssignedEngineer!.LastName, t.AssignedEngineer.FirstName })
            .Select(g => new EngineerStatsDto
            {
                EngineerId = g.Key.AssignedEngineerId!.Value,
                EngineerName = g.Key.LastName + " " + g.Key.FirstName,
                TotalTickets = g.Count(),
                CompletedTickets = g.Count(t =>
                    t.Status == TicketStatus.Completed || t.Status == TicketStatus.CompletedRemotely),
                InProgressTickets = g.Count(t => t.Status == TicketStatus.InProgress)
            })
            .ToListAsync();

        return stats;
    }
}
