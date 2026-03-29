using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Equipment;
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

    public async Task<int> CreateAsync(CreateEquipmentDto dto)
    {
        var equipment = new Core.Entities.Equipment
        {
            Model = dto.Model,
            SerialNumber = dto.SerialNumber,
            InstalledAt = dto.InstalledAt,
            Description = dto.Description,
            ServicePointId = dto.ServicePointId
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
}
