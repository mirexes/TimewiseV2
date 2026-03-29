using ServiceDesk.Core.DTOs.AuditLogs;
using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.DTOs.Clients;
using ServiceDesk.Core.DTOs.Equipment;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.DTOs.Users;
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
            Latitude = ticket.ServicePoint?.Latitude,
            Longitude = ticket.ServicePoint?.Longitude,
            EquipmentId = ticket.EquipmentId,
            EquipmentModel = ticket.Equipment?.Model,
            EquipmentSerialNumber = ticket.Equipment?.SerialNumber,
            CreatedByName = ticket.CreatedByUser?.FullName ?? "",
            AvrPhotoPath = ticket.AvrPhotoPath,
            CompletionPhotos = ticket.CompletionPhotos?.Select(p => new TicketPhotoDto
            {
                Id = p.Id,
                FileName = p.FileName,
                FilePath = p.FilePath,
                FileSize = p.FileSize
            }).ToList() ?? new()
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
            PhotoPath = equipment.PhotoPath,
            IsCompanyOwned = equipment.IsCompanyOwned,
            IsActive = equipment.IsActive,
            ServicePointId = equipment.ServicePointId,
            ServicePointName = equipment.ServicePoint?.Name ?? "",
            ServicePointAddress = equipment.ServicePoint?.Address ?? ""
        };
    }

    /// <summary>Client → ClientDetailDto</summary>
    public static ClientDetailDto ToDetailDto(this Client client)
    {
        return new ClientDetailDto
        {
            Id = client.Id,
            Name = client.Name,
            Inn = client.Inn,
            Network = client.Network,
            Phone = client.Phone,
            Email = client.Email,
            LegalAddress = client.LegalAddress,
            IsActive = client.IsActive,
            TtkFilePath = client.TtkFilePath,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
            ServicePoints = client.ServicePoints?.Select(sp => new ClientServicePointDto
            {
                Id = sp.Id,
                Name = sp.Name,
                Address = sp.Address,
                Region = sp.Region,
                IsActive = sp.IsActive,
                EquipmentCount = sp.Equipment?.Count ?? 0
            }).ToList() ?? new(),
            ContactPersons = client.ContactPersons?.Select(cp => new ClientContactPersonDto
            {
                Id = cp.Id,
                FullName = cp.FullName,
                Phone = cp.Phone,
                Email = cp.Email,
                Position = cp.Position
            }).ToList() ?? new(),
            Managers = client.Managers?.Select(m => new ClientManagerDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Phone = m.Phone,
                Email = m.Email
            }).ToList() ?? new()
        };
    }

    /// <summary>AppUser → UserListDto</summary>
    public static UserListDto ToListDto(this AppUser user)
    {
        return new UserListDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            Role = user.Role,
            Company = user.Company,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>AppUser → UserDetailDto</summary>
    public static UserDetailDto ToDetailDto(this AppUser user)
    {
        return new UserDetailDto
        {
            Id = user.Id,
            LastName = user.LastName,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            Role = user.Role,
            Company = user.Company,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            ClientId = user.ClientId,
            ClientName = user.Client?.Name,
            AvatarUrl = user.AvatarUrl,
            AssignedTicketsCount = user.AssignedTickets?.Count ?? 0
        };
    }

    /// <summary>AuditLog → AuditLogListDto</summary>
    public static AuditLogListDto ToListDto(this AuditLog log)
    {
        return new AuditLogListDto
        {
            Id = log.Id,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            OldValue = log.OldValue,
            NewValue = log.NewValue,
            UserId = log.UserId,
            UserFullName = log.User?.FullName ?? "",
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt
        };
    }

    /// <summary>ChatMessage → ChatMessageDto</summary>
    public static ChatMessageDto ToDto(this ChatMessage message)
    {
        var sender = message.Sender;
        var initials = "";
        if (sender != null)
        {
            var first = !string.IsNullOrEmpty(sender.FirstName) ? sender.FirstName[0].ToString() : "";
            var last = !string.IsNullOrEmpty(sender.LastName) ? sender.LastName[0].ToString() : "";
            initials = (last + first).ToUpper();
        }

        return new ChatMessageDto
        {
            Id = message.Id,
            Text = message.Text,
            SenderId = message.SenderId,
            SenderName = sender?.FullName ?? "",
            SenderAvatarUrl = sender?.AvatarUrl,
            SenderInitials = initials,
            CreatedAt = message.CreatedAt,
            IsRead = message.IsRead,
            ReplyToMessageId = message.ReplyToMessageId,
            ReplyTo = message.ReplyToMessage != null ? new ChatReplyDto
            {
                Id = message.ReplyToMessage.Id,
                Text = message.ReplyToMessage.Text.Length > 100
                    ? message.ReplyToMessage.Text[..100] + "…"
                    : message.ReplyToMessage.Text,
                SenderName = message.ReplyToMessage.Sender?.FullName ?? ""
            } : null,
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
