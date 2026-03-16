using ServiceDesk.Core.DTOs.Equipment;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис работы с оборудованием
/// </summary>
public interface IEquipmentService
{
    Task<IEnumerable<EquipmentDto>> GetAllAsync();
    Task<EquipmentDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateEquipmentDto dto);
    Task UpdateAsync(int id, CreateEquipmentDto dto);
    Task<IEnumerable<EquipmentDto>> GetByServicePointAsync(int servicePointId);
    Task<IEnumerable<EquipmentHistoryDto>> GetRepairHistoryAsync(int equipmentId);
}
