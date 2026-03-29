using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Equipment;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис работы с оборудованием
/// </summary>
public interface IEquipmentService
{
    Task<IEnumerable<EquipmentDto>> GetAllAsync(int currentUserId, UserRole currentUserRole);
    Task<PagedResultDto<EquipmentDto>> GetPagedAsync(int currentUserId, UserRole currentUserRole, string? search, int page, int pageSize);
    Task<EquipmentDto?> GetByIdAsync(int id, int currentUserId, UserRole currentUserRole);
    Task<int> CreateAsync(CreateEquipmentDto dto);
    Task UpdateAsync(int id, CreateEquipmentDto dto);
    Task<IEnumerable<EquipmentDto>> GetByServicePointAsync(int servicePointId);
    Task<IEnumerable<EquipmentHistoryDto>> GetRepairHistoryAsync(int equipmentId);
}
