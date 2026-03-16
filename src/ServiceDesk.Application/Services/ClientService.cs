using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис работы с клиентами
/// </summary>
public class ClientService : IClientService
{
    private readonly AppDbContext _db;

    public ClientService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<SelectOptionDto>> GetClientsForSelectAsync()
    {
        return await _db.Clients
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectOptionDto { Id = c.Id, Text = c.Name })
            .ToListAsync();
    }

    public async Task<IEnumerable<SelectOptionDto>> GetServicePointsForSelectAsync(
        int currentUserId, UserRole currentUserRole, int? clientId = null)
    {
        var query = _db.ServicePoints.Where(sp => sp.IsActive);

        if (clientId.HasValue)
            query = query.Where(sp => sp.ClientId == clientId.Value);

        // Фильтрация по роли пользователя
        if (!PermissionChecker.CanViewAllTickets(currentUserRole))
        {
            query = currentUserRole switch
            {
                // Техник/Инженер — только точки, на которых у них есть заявки
                UserRole.Technician or UserRole.Engineer =>
                    query.Where(sp => _db.Tickets
                        .Any(t => t.AssignedEngineerId == currentUserId &&
                                  t.ServicePointId == sp.Id)),

                // Клиент — точки своей организации + точки, где создавал заявки
                UserRole.Client =>
                    query.Where(sp =>
                        sp.ClientId ==
                            _db.Users.Where(u => u.Id == currentUserId)
                                .Select(u => u.ClientId).FirstOrDefault() ||
                        _db.Tickets.Any(t => t.CreatedByUserId == currentUserId &&
                                              t.ServicePointId == sp.Id)),

                // Менеджер клиента — только точки своей организации
                UserRole.ManagerClient =>
                    query.Where(sp => sp.ClientId ==
                        _db.Users.Where(u => u.Id == currentUserId)
                            .Select(u => u.ClientId).FirstOrDefault()),

                _ => query
            };
        }

        return await query
            .OrderBy(sp => sp.Name)
            .Select(sp => new SelectOptionDto
            {
                Id = sp.Id,
                Text = $"{sp.Name} — {sp.Address}"
            })
            .ToListAsync();
    }
}
