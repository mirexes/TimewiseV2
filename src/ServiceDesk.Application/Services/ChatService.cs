using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис чата заявки
/// </summary>
public class ChatService : IChatService
{
    private readonly AppDbContext _db;

    public ChatService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int ticketId)
    {
        var messages = await _db.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.ReplyToMessage)
                .ThenInclude(r => r!.Sender)
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => m.ToDto());
    }

    public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto, int senderId)
    {
        var message = new ChatMessage
        {
            Text = dto.Text,
            TicketId = dto.TicketId,
            SenderId = senderId,
            ReplyToMessageId = dto.ReplyToMessageId
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        // Подгружаем связанные данные для ответа
        await _db.Entry(message).Reference(m => m.Sender).LoadAsync();
        if (message.ReplyToMessageId.HasValue)
        {
            await _db.Entry(message).Reference(m => m.ReplyToMessage).LoadAsync();
            if (message.ReplyToMessage != null)
                await _db.Entry(message.ReplyToMessage).Reference(m => m.Sender).LoadAsync();
        }
        return message.ToDto();
    }

    public async Task MarkAsReadAsync(int ticketId, int userId)
    {
        var unread = await _db.ChatMessages
            .Where(m => m.TicketId == ticketId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var m in unread)
            m.IsRead = true;

        await _db.SaveChangesAsync();
    }

    public async Task AddMessageWithAttachmentsAsync(int ticketId, int senderId, string text, IEnumerable<TicketAttachmentFile> files)
    {
        var message = new ChatMessage
        {
            Text = text,
            TicketId = ticketId,
            SenderId = senderId
        };

        foreach (var file in files)
        {
            message.Attachments.Add(new ChatAttachment
            {
                FileName = file.FileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            });
        }

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();
    }

    public async Task<ChatMessageDto> SendMessageWithAttachmentsAsync(int ticketId, int senderId, string text, IEnumerable<TicketAttachmentFile> files, int? replyToMessageId = null)
    {
        var message = new ChatMessage
        {
            Text = text,
            TicketId = ticketId,
            SenderId = senderId,
            ReplyToMessageId = replyToMessageId
        };

        foreach (var file in files)
        {
            message.Attachments.Add(new ChatAttachment
            {
                FileName = file.FileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                FileSize = file.FileSize
            });
        }

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        // Подгружаем связанные данные для ответа
        await _db.Entry(message).Reference(m => m.Sender).LoadAsync();
        if (message.ReplyToMessageId.HasValue)
        {
            await _db.Entry(message).Reference(m => m.ReplyToMessage).LoadAsync();
            if (message.ReplyToMessage != null)
                await _db.Entry(message.ReplyToMessage).Reference(m => m.Sender).LoadAsync();
        }
        return message.ToDto();
    }
}
