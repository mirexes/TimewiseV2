using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Equipment;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис работы с оборудованием
/// </summary>
public class EquipmentService : IEquipmentService
{
    private readonly AppDbContext _db;

    public EquipmentService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Применяет фильтрацию оборудования по роли пользователя
    /// </summary>
    private IQueryable<Core.Entities.Equipment> ApplyRoleFilter(
        IQueryable<Core.Entities.Equipment> query, int currentUserId, UserRole currentUserRole)
    {
        if (PermissionChecker.CanViewAllTickets(currentUserRole))
            return query;

        return currentUserRole switch
        {
            // Техник/Инженер — оборудование точек, на которых у них есть заявки
            UserRole.Technician or UserRole.Engineer =>
                query.Where(e => _db.Tickets
                    .Any(t => t.AssignedEngineerId == currentUserId &&
                              t.ServicePointId == e.ServicePointId)),

            // Клиент — оборудование точек, для которых он создавал заявки
            UserRole.Client =>
                query.Where(e => _db.Tickets
                    .Any(t => t.CreatedByUserId == currentUserId &&
                              t.ServicePointId == e.ServicePointId)),

            // Менеджер клиента — оборудование точек своей организации
            UserRole.ManagerClient =>
                query.Where(e => e.ServicePoint.ClientId ==
                    _db.Users.Where(u => u.Id == currentUserId)
                        .Select(u => u.ClientId).FirstOrDefault()),

            _ => query
        };
    }

    public async Task<IEnumerable<EquipmentDto>> GetAllAsync(int currentUserId, UserRole currentUserRole)
    {
        var query = _db.Equipment
            .Include(e => e.ServicePoint)
            .Where(e => e.IsActive)
            .AsQueryable();

        query = ApplyRoleFilter(query, currentUserId, currentUserRole);

        var items = await query
            .OrderBy(e => e.Model)
            .ToListAsync();

        return items.Select(e => e.ToDto());
    }

    public async Task<PagedResultDto<EquipmentDto>> GetPagedAsync(
        int currentUserId, UserRole currentUserRole, string? search, int page, int pageSize)
    {
        var query = _db.Equipment
            .Include(e => e.ServicePoint)
            .Where(e => e.IsActive)
            .AsQueryable();

        query = ApplyRoleFilter(query, currentUserId, currentUserRole);

        // Поиск по серийному номеру
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.SerialNumber.Contains(search));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.Model)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<EquipmentDto>(
            items.Select(e => e.ToDto()), totalCount, page, pageSize);
    }

    public async Task<EquipmentDto?> GetByIdAsync(int id, int currentUserId, UserRole currentUserRole)
    {
        var query = _db.Equipment
            .Include(e => e.ServicePoint)
            .Where(e => e.Id == id);

        query = ApplyRoleFilter(query, currentUserId, currentUserRole);

        var equipment = await query.FirstOrDefaultAsync();
        return equipment?.ToDto();
    }

    public async Task<int> CreateAsync(CreateEquipmentDto dto, int currentUserId)
    {
        var servicePointId = dto.ServicePointId;

        // Если точка не выбрана, но указан новый адрес — создаём точку обслуживания
        if (servicePointId == 0 && !string.IsNullOrWhiteSpace(dto.NewAddress))
        {
            double.TryParse(dto.Latitude, CultureInfo.InvariantCulture, out var lat);
            double.TryParse(dto.Longitude, CultureInfo.InvariantCulture, out var lng);

            var clientId = await _db.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => u.ClientId)
                .FirstOrDefaultAsync();

            if (clientId is null or 0)
            {
                var defaultClient = await _db.Clients
                    .FirstOrDefaultAsync(c => c.Name == "Без организации");
                if (defaultClient == null)
                {
                    defaultClient = new Client { Name = "Без организации", IsActive = true };
                    _db.Clients.Add(defaultClient);
                    await _db.SaveChangesAsync();
                }
                clientId = defaultClient.Id;
            }

            var existing = await _db.ServicePoints.FirstOrDefaultAsync(n => n.Address == dto.NewAddress);
            if (existing == null)
            {
                var newPoint = new ServicePoint
                {
                    Name = dto.NewAddress,
                    Address = dto.NewAddress,
                    Latitude = lat != 0 ? lat : null,
                    Longitude = lng != 0 ? lng : null,
                    IsActive = true,
                    ClientId = clientId.Value
                };
                _db.ServicePoints.Add(newPoint);
                await _db.SaveChangesAsync();
                servicePointId = newPoint.Id;
            }
            else
            {
                servicePointId = existing.Id;
            }
        }

        var equipment = new Core.Entities.Equipment
        {
            Model = dto.Model,
            SerialNumber = dto.SerialNumber,
            InstalledAt = dto.InstalledAt,
            Description = dto.Description,
            ServicePointId = servicePointId
        };

        _db.Equipment.Add(equipment);
        await _db.SaveChangesAsync();
        return equipment.Id;
    }

    public async Task UpdateAsync(int id, CreateEquipmentDto dto)
    {
        var equipment = await _db.Equipment.FindAsync(id)
            ?? throw new KeyNotFoundException($"Оборудование {id} не найдено");

        equipment.Model = dto.Model;
        equipment.SerialNumber = dto.SerialNumber;
        equipment.InstalledAt = dto.InstalledAt;
        equipment.Description = dto.Description;
        equipment.ServicePointId = dto.ServicePointId;

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<EquipmentDto>> GetByServicePointAsync(int servicePointId)
    {
        var items = await _db.Equipment
            .Include(e => e.ServicePoint)
            .Where(e => e.ServicePointId == servicePointId && e.IsActive)
            .ToListAsync();

        return items.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<EquipmentHistoryDto>> GetRepairHistoryAsync(int equipmentId)
    {
        var tickets = await _db.Tickets
            .Include(t => t.AssignedEngineer)
            .Where(t => t.EquipmentId == equipmentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(t => new EquipmentHistoryDto
        {
            TicketId = t.Id,
            TicketNumber = t.TicketNumber,
            Status = t.Status.ToString(),
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            CompletedAt = t.WorkCompletedAt,
            EngineerName = t.AssignedEngineer?.FullName
        });
    }

    public async Task SetPhotoAsync(int equipmentId, string photoPath)
    {
        var equipment = await _db.Equipment.FindAsync(equipmentId)
            ?? throw new KeyNotFoundException($"Оборудование {equipmentId} не найдено");

        equipment.PhotoPath = photoPath;
        await _db.SaveChangesAsync();
    }
}
