using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Mapping;
using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.DTOs.Tickets;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис группового чата компании
/// </summary>
public class CompanyChatService : ICompanyChatService
{
    private readonly AppDbContext _db;

    public CompanyChatService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetOrCreateChatAsync()
    {
        var chat = await _db.CompanyChats.FirstOrDefaultAsync(c => c.IsActive && !c.IsDirectChat);
        if (chat != null)
            return chat.Id;

        // Создаём единственный групповой чат компании
        chat = new CompanyChat { Name = "Чат компании" };
        _db.CompanyChats.Add(chat);
        await _db.SaveChangesAsync();
        return chat.Id;
    }

    public async Task<bool> IsMemberAsync(int chatId, int userId)
    {
        return await _db.CompanyChatMembers
            .AnyAsync(m => m.CompanyChatId == chatId && m.UserId == userId);
    }

    public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int chatId)
    {
        var messages = await _db.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.ReplyToMessage)
                .ThenInclude(r => r!.Sender)
            .Where(m => m.CompanyChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => m.ToDto());
    }

    public async Task<ChatMessageDto> SendMessageAsync(int chatId, int senderId, string text, int? replyToMessageId = null)
    {
        var message = new ChatMessage
        {
            Text = text,
            CompanyChatId = chatId,
            SenderId = senderId,
            ReplyToMessageId = replyToMessageId
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        await _db.Entry(message).Reference(m => m.Sender).LoadAsync();
        if (message.ReplyToMessageId.HasValue)
        {
            await _db.Entry(message).Reference(m => m.ReplyToMessage).LoadAsync();
            if (message.ReplyToMessage != null)
                await _db.Entry(message.ReplyToMessage).Reference(m => m.Sender).LoadAsync();
        }
        return message.ToDto();
    }

    public async Task<ChatMessageDto> SendMessageWithAttachmentsAsync(int chatId, int senderId, string text, IEnumerable<TicketAttachmentFile> files, int? replyToMessageId = null)
    {
        var message = new ChatMessage
        {
            Text = text,
            CompanyChatId = chatId,
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

        await _db.Entry(message).Reference(m => m.Sender).LoadAsync();
        if (message.ReplyToMessageId.HasValue)
        {
            await _db.Entry(message).Reference(m => m.ReplyToMessage).LoadAsync();
            if (message.ReplyToMessage != null)
                await _db.Entry(message.ReplyToMessage).Reference(m => m.Sender).LoadAsync();
        }
        return message.ToDto();
    }

    public async Task MarkAsReadAsync(int chatId, int userId)
    {
        var unread = await _db.ChatMessages
            .Where(m => m.CompanyChatId == chatId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var m in unread)
            m.IsRead = true;

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<CompanyChatMemberDto>> GetMembersAsync(int chatId)
    {
        return await _db.CompanyChatMembers
            .Include(m => m.User)
            .Where(m => m.CompanyChatId == chatId)
            .Select(m => new CompanyChatMemberDto
            {
                UserId = m.UserId,
                FullName = m.User.FullName,
                Role = m.User.Role,
                AvatarUrl = m.User.AvatarUrl
            })
            .ToListAsync();
    }

    public async Task AddMemberAsync(int chatId, int userId)
    {
        var exists = await _db.CompanyChatMembers
            .AnyAsync(m => m.CompanyChatId == chatId && m.UserId == userId);

        if (exists) return;

        _db.CompanyChatMembers.Add(new CompanyChatMember
        {
            CompanyChatId = chatId,
            UserId = userId
        });
        await _db.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int chatId, int userId)
    {
        var member = await _db.CompanyChatMembers
            .FirstOrDefaultAsync(m => m.CompanyChatId == chatId && m.UserId == userId);

        if (member == null) return;

        _db.CompanyChatMembers.Remove(member);
        await _db.SaveChangesAsync();
    }
}
