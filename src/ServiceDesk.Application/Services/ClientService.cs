using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.DTOs.Common;
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

    public async Task<IEnumerable<SelectOptionDto>> GetServicePointsForSelectAsync(int? clientId = null)
    {
        var query = _db.ServicePoints.Where(sp => sp.IsActive);

        if (clientId.HasValue)
            query = query.Where(sp => sp.ClientId == clientId.Value);

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
