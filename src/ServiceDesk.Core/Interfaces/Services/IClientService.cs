using ServiceDesk.Core.DTOs.Common;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис работы с клиентами
/// </summary>
public interface IClientService
{
    Task<IEnumerable<SelectOptionDto>> GetClientsForSelectAsync();
    Task<IEnumerable<SelectOptionDto>> GetServicePointsForSelectAsync(int? clientId = null);
}
