using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.DTOs.Equipment;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Application.Mapping;

/// <summary>
/// Ручной маппинг сущностей в DTO (без AutoMapper)
/// </summary>
public static class MappingExtensions
{
    /// <summary>Ticket → TicketListDto</summary>
    public static TicketListDto ToListDto(this Ticket ticket)
    {
        return new TicketListDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Type = ticket.Type,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Description = ticket.Description,
            Address = ticket.ServicePoint?.Address ?? "",
            EngineerName = ticket.AssignedEngineer?.FullName,
            CreatedAt = ticket.CreatedAt,
            Deadline = ticket.Deadline
        };
    }

    /// <summary>Ticket → TicketDetailDto</summary>
    public static TicketDetailDto ToDetailDto(this Ticket ticket)
    {
        return new TicketDetailDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Type = ticket.Type,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Description = ticket.Description,
            Comment = ticket.Comment,
            Deadline = ticket.Deadline,
            WorkStartedAt = ticket.WorkStartedAt,
            WorkCompletedAt = ticket.WorkCompletedAt,
            CreatedAt = ticket.CreatedAt,
            AssignedEngineerId = ticket.AssignedEngineerId,
            EngineerName = ticket.AssignedEngineer?.FullName,
            ServicePointId = ticket.ServicePointId,
            ServicePointName = ticket.ServicePoint?.Name ?? "",
            Address = ticket.ServicePoint?.Address ?? "",
            EquipmentId = ticket.EquipmentId,
            EquipmentModel = ticket.Equipment?.Model,
            EquipmentSerialNumber = ticket.Equipment?.SerialNumber,
            CreatedByName = ticket.CreatedByUser?.FullName ?? "",
            AvrPhotoPath = ticket.AvrPhotoPath
        };
    }

    /// <summary>Equipment → EquipmentDto</summary>
    public static EquipmentDto ToDto(this Core.Entities.Equipment equipment)
    {
        return new EquipmentDto
        {
            Id = equipment.Id,
            Model = equipment.Model,
            SerialNumber = equipment.SerialNumber,
            InstalledAt = equipment.InstalledAt,
            Description = equipment.Description,
            IsActive = equipment.IsActive,
            ServicePointId = equipment.ServicePointId,
            ServicePointName = equipment.ServicePoint?.Name ?? "",
            ServicePointAddress = equipment.ServicePoint?.Address ?? ""
        };
    }

    /// <summary>ChatMessage → ChatMessageDto</summary>
    public static ChatMessageDto ToDto(this ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            Text = message.Text,
            SenderId = message.SenderId,
            SenderName = message.Sender?.FullName ?? "",
            CreatedAt = message.CreatedAt,
            IsRead = message.IsRead,
            Attachments = message.Attachments?.Select(a => new ChatAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                FileSize = a.FileSize
            }).ToList() ?? new()
        };
    }
}
