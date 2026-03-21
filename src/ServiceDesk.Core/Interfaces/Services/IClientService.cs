using ServiceDesk.Core.DTOs.Clients;
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

    Task<PagedResultDto<ClientListDto>> GetAllAsync(ClientFilterDto filter);
    Task<ClientDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateClientDto dto, string? ttkFilePath = null);
    Task UpdateAsync(int id, CreateClientDto dto, string? ttkFilePath = null);
    Task<string?> GetTtkFilePathAsync(int clientId);
    Task ToggleActiveAsync(int id);

    Task<int> AddContactPersonAsync(int clientId, CreateContactPersonDto dto);
    Task RemoveContactPersonAsync(int contactPersonId);
}
