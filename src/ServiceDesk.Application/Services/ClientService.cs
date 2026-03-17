using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.Clients;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.Entities;
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

                // Клиент — только точки, для которых он создавал заявки
                UserRole.Client =>
                    query.Where(sp => _db.Tickets
                        .Any(t => t.CreatedByUserId == currentUserId &&
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

    public async Task<PagedResultDto<ClientListDto>> GetAllAsync(ClientFilterDto filter)
    {
        var query = _db.Clients.AsQueryable();

        // Фильтр по активности
        if (filter.IsActive.HasValue)
            query = query.Where(c => c.IsActive == filter.IsActive.Value);

        // Поиск по названию, ИНН, сети
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                (c.Inn != null && c.Inn.Contains(search)) ||
                (c.Network != null && c.Network.ToLower().Contains(search)) ||
                (c.Phone != null && c.Phone.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new ClientListDto
            {
                Id = c.Id,
                Name = c.Name,
                Inn = c.Inn,
                Network = c.Network,
                Phone = c.Phone,
                Email = c.Email,
                IsActive = c.IsActive,
                ServicePointsCount = c.ServicePoints.Count,
                ContactPersonsCount = c.ContactPersons.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<ClientListDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<ClientDetailDto?> GetByIdAsync(int id)
    {
        var client = await _db.Clients
            .Include(c => c.ServicePoints)
                .ThenInclude(sp => sp.Equipment)
            .Include(c => c.ContactPersons)
            .Include(c => c.Managers)
            .FirstOrDefaultAsync(c => c.Id == id);

        return client?.ToDetailDto();
    }

    public async Task<int> CreateAsync(CreateClientDto dto)
    {
        var client = new Client
        {
            Name = dto.Name,
            Inn = dto.Inn,
            Network = dto.Network,
            Phone = dto.Phone,
            Email = dto.Email,
            LegalAddress = dto.LegalAddress,
            IsActive = dto.IsActive
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return client.Id;
    }

    public async Task UpdateAsync(int id, CreateClientDto dto)
    {
        var client = await _db.Clients.FindAsync(id)
            ?? throw new KeyNotFoundException($"Клиент {id} не найден");

        client.Name = dto.Name;
        client.Inn = dto.Inn;
        client.Network = dto.Network;
        client.Phone = dto.Phone;
        client.Email = dto.Email;
        client.LegalAddress = dto.LegalAddress;
        client.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var client = await _db.Clients.FindAsync(id)
            ?? throw new KeyNotFoundException($"Клиент {id} не найден");

        client.IsActive = !client.IsActive;
        await _db.SaveChangesAsync();
    }

    public async Task<int> AddContactPersonAsync(int clientId, CreateContactPersonDto dto)
    {
        // Проверяем существование клиента
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId);
        if (!clientExists)
            throw new KeyNotFoundException($"Клиент {clientId} не найден");

        var contactPerson = new ContactPerson
        {
            ClientId = clientId,
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            Position = dto.Position
        };

        _db.ContactPersons.Add(contactPerson);
        await _db.SaveChangesAsync();
        return contactPerson.Id;
    }

    public async Task RemoveContactPersonAsync(int contactPersonId)
    {
        var contactPerson = await _db.ContactPersons.FindAsync(contactPersonId)
            ?? throw new KeyNotFoundException($"Контактное лицо {contactPersonId} не найдено");

        _db.ContactPersons.Remove(contactPerson);
        await _db.SaveChangesAsync();
    }
}
