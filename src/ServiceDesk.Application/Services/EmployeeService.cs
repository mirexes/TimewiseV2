using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.DTOs.Reports;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис учёта наработки сотрудников (часы работы и время в дороге).
/// Показатели восстанавливаются по журналу смены статусов заявок (AuditLog):
/// — время в статусе «Выполнение» (InProgress) считается отработанным;
/// — время в статусе «Техник в пути» (EngineerEnRoute) считается временем в дороге.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;

    public EmployeeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EmployeeWorkReportDto> GetMonthlyWorkStatsAsync(int year, int month)
    {
        // Защита от некорректных значений из строки запроса
        if (month < 1 || month > 12) month = DateTime.UtcNow.Month;
        if (year < 2000 || year > 9999) year = DateTime.UtcNow.Year;

        // Период всегда начинается с 1-го числа месяца
        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);
        var now = DateTime.UtcNow;
        // Для незавершённого (открытого) интервала текущего статуса считаем до «сейчас»,
        // но не выходя за пределы месяца
        var effectiveEnd = now < periodEnd ? now : periodEnd;

        // Записи смены статусов заявок за месяц
        var logs = await _db.AuditLogs
            .Where(a => a.EntityType == "Ticket" && a.Action == AuditAction.StatusChanged
                && a.CreatedAt >= periodStart && a.CreatedAt < periodEnd)
            .Select(a => new { a.EntityId, a.NewValue, a.CreatedAt })
            .ToListAsync();

        // Сопоставление «заявка → назначенный исполнитель»
        var ticketIds = logs.Select(l => l.EntityId).Distinct().ToList();
        var ticketEngineer = await _db.Tickets
            .Where(t => ticketIds.Contains(t.Id) && t.AssignedEngineerId.HasValue)
            .Select(t => new { t.Id, EngineerId = t.AssignedEngineerId!.Value })
            .ToDictionaryAsync(t => t.Id, t => t.EngineerId);

        // Завершённые за месяц заявки (по назначенному исполнителю)
        var completed = await _db.Tickets
            .Where(t => t.AssignedEngineerId.HasValue
                && (t.Status == TicketStatus.Completed || t.Status == TicketStatus.CompletedRemotely)
                && t.WorkCompletedAt.HasValue
                && t.WorkCompletedAt >= periodStart && t.WorkCompletedAt < periodEnd)
            .GroupBy(t => t.AssignedEngineerId!.Value)
            .Select(g => new { EngineerId = g.Key, Count = g.Count() })
            .ToListAsync();

        // Базовый состав — выездной персонал (показываем всегда, даже без наработки)
        var baseEmployees = await _db.Users
            .Where(u => u.IsActive && (u.Role == UserRole.Technician
                || u.Role == UserRole.Engineer || u.Role == UserRole.ChiefEngineer))
            .Select(u => new { u.Id, u.LastName, u.FirstName, u.MiddleName, u.Role })
            .ToListAsync();

        // Идентификаторы всех, у кого есть активность за месяц (любая роль исполнителя)
        var activeIds = new HashSet<int>(ticketEngineer.Values);
        activeIds.UnionWith(completed.Select(c => c.EngineerId));

        // Добираем данные тех исполнителей, кого нет в базовом составе
        var baseIds = baseEmployees.Select(e => e.Id).ToHashSet();
        var extraIds = activeIds.Where(id => !baseIds.Contains(id)).ToList();
        var extraUsers = await _db.Users
            .Where(u => extraIds.Contains(u.Id))
            .Select(u => new { u.Id, u.LastName, u.FirstName, u.MiddleName, u.Role })
            .ToListAsync();

        var stats = baseEmployees.Concat(extraUsers).ToDictionary(
            u => u.Id,
            u => new EmployeeWorkStatsDto
            {
                EmployeeId = u.Id,
                EmployeeName = $"{u.LastName} {u.FirstName} {u.MiddleName}".Trim(),
                RoleName = RoleName(u.Role)
            });

        // Реконструкция временных интервалов по каждой заявке
        foreach (var group in logs.GroupBy(l => l.EntityId))
        {
            if (!ticketEngineer.TryGetValue(group.Key, out var engineerId)) continue;
            if (!stats.TryGetValue(engineerId, out var s)) continue;

            var events = group.OrderBy(e => e.CreatedAt).ToList();
            var handled = false;

            for (int i = 0; i < events.Count; i++)
            {
                if (!Enum.TryParse<TicketStatus>(events[i].NewValue, out var segStatus))
                    continue;

                var segStart = events[i].CreatedAt;
                // Конец интервала — следующая смена статуса либо «сейчас» для текущего статуса
                var segEnd = i + 1 < events.Count ? events[i + 1].CreatedAt : effectiveEnd;
                if (segEnd <= segStart) continue;

                var hours = (segEnd - segStart).TotalHours;

                if (segStatus == TicketStatus.EngineerEnRoute)
                {
                    s.TravelHours += hours;
                    handled = true;
                }
                else if (segStatus == TicketStatus.InProgress)
                {
                    s.WorkedHours += hours;
                    handled = true;
                }
            }

            if (handled) s.HandledTickets++;
        }

        // Количество завершённых заявок за месяц
        foreach (var c in completed)
            if (stats.TryGetValue(c.EngineerId, out var s))
                s.CompletedTickets = c.Count;

        // Округление до десятых
        foreach (var s in stats.Values)
        {
            s.WorkedHours = Math.Round(s.WorkedHours, 1);
            s.TravelHours = Math.Round(s.TravelHours, 1);
        }

        return new EmployeeWorkReportDto
        {
            Year = year,
            Month = month,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Employees = stats.Values
                .OrderByDescending(s => s.WorkedHours)
                .ThenBy(s => s.EmployeeName)
                .ToList()
        };
    }

    /// <summary>Человекочитаемое название должности</summary>
    private static string RoleName(UserRole role) => role switch
    {
        UserRole.Technician => "Техник",
        UserRole.Engineer => "Инженер",
        UserRole.ChiefEngineer => "Главный инженер",
        UserRole.Logist => "Логист",
        UserRole.ManagerTimewise => "Менеджер Timewise",
        UserRole.Client => "Клиент",
        UserRole.ManagerClient => "Менеджер клиента",
        UserRole.Moderator => "Модератор",
        _ => role.ToString()
    };
}
