using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Tickets;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис работы с заявками
/// </summary>
public interface ITicketService
{
    Task<PagedResultDto<TicketListDto>> GetTicketsAsync(TicketFilterDto filter, int page, int pageSize);
    Task<TicketDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateTicketDto dto, int currentUserId);
    Task UpdateStatusAsync(UpdateTicketStatusDto dto, int currentUserId);
    Task AssignEngineerAsync(int ticketId, int engineerId, int currentUserId);
    Task<IEnumerable<TicketListDto>> GetByEngineerAsync(int engineerId);
}
