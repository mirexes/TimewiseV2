using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Mapping;
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

    public async Task<IEnumerable<EquipmentDto>> GetAllAsync()
    {
        var items = await _db.Equipment
            .Include(e => e.ServicePoint)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Model)
            .ToListAsync();

        return items.Select(e => e.ToDto());
    }

    public async Task<EquipmentDto?> GetByIdAsync(int id)
    {
        var equipment = await _db.Equipment
            .Include(e => e.ServicePoint)
            .FirstOrDefaultAsync(e => e.Id == id);

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
