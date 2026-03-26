using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис работы с заявками
/// </summary>
public interface ITicketService
{
    Task<PagedResultDto<TicketListDto>> GetTicketsAsync(
        TicketFilterDto filter, int page, int pageSize, int currentUserId, UserRole currentUserRole);
    Task<TicketDetailDto?> GetByIdAsync(int id, int currentUserId, UserRole currentUserRole);
    Task<int> CreateAsync(CreateTicketDto dto, int currentUserId);
    Task SaveAttachmentsAsync(int ticketId, IEnumerable<TicketAttachmentFile> files);
    Task UpdateStatusAsync(UpdateTicketStatusDto dto, int currentUserId);
    Task SaveCompletionPhotosAsync(int ticketId, IEnumerable<TicketAttachmentFile> files);
    Task AssignEngineerAsync(int ticketId, int engineerId, int currentUserId);
    Task<IEnumerable<TicketListDto>> GetByEngineerAsync(int engineerId);
    /// <summary>Получить список специалистов (техники + инженеры + главные инженеры)</summary>
    Task<IEnumerable<EngineerSelectDto>> GetEngineersAsync();
    /// <summary>Привязать оборудование к заявке</summary>
    Task UpdateEquipmentAsync(int ticketId, int? equipmentId, int currentUserId);
}
