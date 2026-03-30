using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Helpers;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.Common;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис работы с заявками
/// </summary>
public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly INotificationService _notifications;

    public TicketService(AppDbContext db, IAuditService audit, INotificationService notifications)
    {
        _db = db;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<PagedResultDto<TicketListDto>> GetTicketsAsync(
        TicketFilterDto filter, int page, int pageSize, int currentUserId, UserRole currentUserRole)
    {
        var query = _db.Tickets
            .Include(t => t.ServicePoint)
            .Include(t => t.AssignedEngineer)
            .AsQueryable();

        // Фильтрация по роли — ограничиваем видимость заявок
        if (!PermissionChecker.CanViewAllTickets(currentUserRole))
        {
            query = currentUserRole switch
            {
                // Техник и Инженер видят только назначенные на них заявки
                UserRole.Technician or UserRole.Engineer =>
                    query.Where(t => t.AssignedEngineerId == currentUserId),

                // Клиент видит только свои заявки
                UserRole.Client =>
                    query.Where(t => t.CreatedByUserId == currentUserId),

                // Менеджер клиента видит заявки точек своей организации
                UserRole.ManagerClient =>
                    query.Where(t => t.ServicePoint.ClientId ==
                        _db.Users.Where(u => u.Id == currentUserId)
                            .Select(u => u.ClientId).FirstOrDefault()),

                _ => query
            };
        }

        // Фильтрация
        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.EngineerId.HasValue)
            query = query.Where(t => t.AssignedEngineerId == filter.EngineerId.Value);

        if (!string.IsNullOrEmpty(filter.Region))
            query = query.Where(t => t.ServicePoint.Region == filter.Region);

        if (!string.IsNullOrEmpty(filter.Network))
            query = query.Where(t => t.ServicePoint.Network == filter.Network);

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo.Value);

        if (!string.IsNullOrEmpty(filter.SearchQuery))
        {
            var sq = filter.SearchQuery.ToLower();
            query = query.Where(t =>
                t.TicketNumber.ToLower().Contains(sq) ||
                t.Description.ToLower().Contains(sq) ||
                t.ServicePoint.Address.ToLower().Contains(sq));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<TicketListDto>(
            items.Select(t => t.ToListDto()), totalCount, page, pageSize);
    }

    public async Task<TicketDetailDto?> GetByIdAsync(int id, int currentUserId, UserRole currentUserRole)
    {
        var ticket = await _db.Tickets
            .Include(t => t.ServicePoint)
            .Include(t => t.AssignedEngineer)
            .Include(t => t.Equipment)
            .Include(t => t.CreatedByUser)
            .Include(t => t.CompletionPhotos)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null) return null;

        // Проверка доступа к конкретной заявке
        if (!PermissionChecker.CanViewAllTickets(currentUserRole))
        {
            var hasAccess = currentUserRole switch
            {
                UserRole.Technician or UserRole.Engineer =>
                    ticket.AssignedEngineerId == currentUserId,
                UserRole.Client =>
                    ticket.CreatedByUserId == currentUserId,
                UserRole.ManagerClient =>
                    ticket.ServicePoint?.ClientId ==
                        await _db.Users.Where(u => u.Id == currentUserId)
                            .Select(u => u.ClientId).FirstOrDefaultAsync(),
                _ => false
            };

            if (!hasAccess) return null;
        }

        var dto = ticket.ToDetailDto();
        dto.AllowedTransitions = TicketStatusTransitions.GetAllowedTransitions(ticket.Status);
        dto.CanAssignEngineer = PermissionChecker.CanAssignEngineer(currentUserRole);
        dto.CanEditEquipment = PermissionChecker.CanEditEquipment(currentUserRole);
        dto.CanEdit = ticket.CreatedByUserId == currentUserId
            && ticket.Status is not TicketStatus.Completed
                and not TicketStatus.CompletedRemotely
                and not TicketStatus.Cancelled;
        return dto;
    }

    public async Task<int> CreateAsync(CreateTicketDto dto, int currentUserId)
    {
        // Если указан новый адрес — создаём точку обслуживания
        var servicePointId = dto.ServicePointId ?? 0;
        if (servicePointId == 0 && !string.IsNullOrWhiteSpace(dto.NewAddress))
        {
            double.TryParse(dto.Latitude, CultureInfo.InvariantCulture, out var lat);
            double.TryParse(dto.Longitude, CultureInfo.InvariantCulture, out var lng);

            // Определяем ClientId: от пользователя, или «Без организации»
            var clientId = await _db.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => u.ClientId)
                .FirstOrDefaultAsync();

            if (clientId is null or 0)
            {
                // Ищем или создаём клиента-заглушку
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
            var ad = _db.ServicePoints.FirstOrDefault(n=>n.Address== dto.NewAddress);
            if (ad == null)
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
                servicePointId = ad.Id;
            }
        }

        // Генерация номера заявки
        var count = await _db.Tickets.CountAsync() + 1;
        var ticketNumber = $"SD-{DateTime.UtcNow:yyyyMM}-{count:D4}";

        var ticket = new Ticket
        {
            TicketNumber = ticketNumber,
            Type = dto.Type,
            Status = TicketStatus.New,
            Priority = dto.Priority,
            Description = dto.Description,
            ServicePointId = servicePointId,
            EquipmentId = dto.EquipmentId,
            AssignedEngineerId = dto.AssignedEngineerId,
            Deadline = dto.Deadline,
            CreatedByUserId = currentUserId
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.Created, "Ticket", ticket.Id,
            null, ticketNumber, currentUserId);

        return ticket.Id;
    }

    public async Task UpdateAsync(EditTicketDto dto, int currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Заявка {dto.Id} не найдена");

        // Только создатель может редактировать
        if (ticket.CreatedByUserId != currentUserId)
            throw new UnauthorizedAccessException("Редактировать заявку может только её создатель");

        // Нельзя редактировать завершённые и отменённые заявки
        if (ticket.Status is TicketStatus.Completed or TicketStatus.CompletedRemotely or TicketStatus.Cancelled)
            throw new InvalidOperationException("Нельзя редактировать завершённую или отменённую заявку");

        // Обработка точки обслуживания
        if (dto.ServicePointId.HasValue && dto.ServicePointId.Value > 0)
        {
            ticket.ServicePointId = dto.ServicePointId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.NewAddress))
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

            var existing = await _db.ServicePoints
                .FirstOrDefaultAsync(n => n.Address == dto.NewAddress);
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
                ticket.ServicePointId = newPoint.Id;
            }
            else
            {
                ticket.ServicePointId = existing.Id;
            }
        }

        ticket.Type = dto.Type;
        ticket.Priority = dto.Priority;
        ticket.Description = dto.Description;
        ticket.Deadline = dto.Deadline;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.Updated, "Ticket", ticket.Id,
            null, ticket.TicketNumber, currentUserId);
    }

    public async Task SaveAttachmentsAsync(int ticketId, IEnumerable<TicketAttachmentFile> files)
    {
        foreach (var file in files)
        {
            var attachment = new TicketAttachment
            {
                TicketId = ticketId,
                FileName = file.FileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            };
            _db.TicketAttachments.Add(attachment);
        }
        await _db.SaveChangesAsync();
    }

    public async Task SaveCompletionPhotosAsync(int ticketId, IEnumerable<TicketAttachmentFile> files)
    {
        foreach (var file in files)
        {
            var photo = new TicketPhoto
            {
                TicketId = ticketId,
                FileName = file.FileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            };
            _db.TicketPhotos.Add(photo);
        }
        await _db.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(UpdateTicketStatusDto dto, int currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(dto.TicketId)
            ?? throw new KeyNotFoundException($"Заявка {dto.TicketId} не найдена");

        if (!TicketStatusTransitions.IsAllowed(ticket.Status, dto.NewStatus))
            throw new InvalidOperationException(
                $"Переход {ticket.Status} → {dto.NewStatus} недопустим");

        var oldStatus = ticket.Status;
        ticket.Status = dto.NewStatus;
        ticket.Comment = dto.Comment;

        if (dto.NewStatus == TicketStatus.InProgress)
            ticket.WorkStartedAt = DateTime.UtcNow;

        if (dto.NewStatus is TicketStatus.Completed or TicketStatus.CompletedRemotely)
            ticket.WorkCompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.StatusChanged, "Ticket", ticket.Id,
            oldStatus.ToString(), dto.NewStatus.ToString(), currentUserId);

        await _notifications.OnTicketStatusChangedAsync(ticket, oldStatus);
    }

    public async Task AssignEngineerAsync(int ticketId, int engineerId, int currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId)
            ?? throw new KeyNotFoundException($"Заявка {ticketId} не найдена");

        var oldEngineer = ticket.AssignedEngineerId?.ToString();
        ticket.AssignedEngineerId = engineerId;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.Assigned, "Ticket", ticket.Id,
            oldEngineer, engineerId.ToString(), currentUserId);

        // Уведомляем назначенного специалиста
        await _notifications.OnTicketAssignedAsync(ticket);
    }

    public async Task<IEnumerable<EngineerSelectDto>> GetEngineersAsync()
    {
        return await _db.Users
            .Where(u => u.IsActive &&
                (u.Role == UserRole.Technician || u.Role == UserRole.Engineer || u.Role == UserRole.ChiefEngineer || u.Role == UserRole.Logist || u.Role == UserRole.ManagerTimewise || u.Role == UserRole.Moderator))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new EngineerSelectDto
            {
                Id = u.Id,
                FullName = (u.LastName + " " + u.FirstName + " " + (u.MiddleName ?? "")).Trim(),
                Role = u.Role == UserRole.Technician ? "Техник"
                    : u.Role == UserRole.Engineer ? "Инженер"
                    : "Главный инженер"
            })
            .ToListAsync();
    }

    public async Task UpdateEquipmentAsync(int ticketId, int? equipmentId, int currentUserId)
    {
        var ticket = await _db.Tickets.FindAsync(ticketId)
            ?? throw new KeyNotFoundException($"Заявка {ticketId} не найдена");

        var oldEquipmentId = ticket.EquipmentId?.ToString();
        ticket.EquipmentId = equipmentId;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.Updated, "Ticket", ticket.Id,
            oldEquipmentId, equipmentId?.ToString(), currentUserId);
    }

    public async Task<IEnumerable<TicketListDto>> GetByEngineerAsync(int engineerId)
    {
        var tickets = await _db.Tickets
            .Include(t => t.ServicePoint)
            .Include(t => t.AssignedEngineer)
            .Where(t => t.AssignedEngineerId == engineerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tickets.Select(t => t.ToListDto());
    }
}
