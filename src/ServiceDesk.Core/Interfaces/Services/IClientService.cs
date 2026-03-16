using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис работы с клиентами
/// </summary>
public interface IClientService
{
    Task<IEnumerable<SelectOptionDto>> GetClientsForSelectAsync();
    Task<IEnumerable<SelectOptionDto>> GetServicePointsForSelectAsync(
        int currentUserId, UserRole currentUserRole, int? clientId = null);
}
