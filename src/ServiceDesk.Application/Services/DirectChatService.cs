using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.DTOs.Chat;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис личных чатов между сотрудниками
/// </summary>
public class DirectChatService : IDirectChatService
{
    private readonly AppDbContext _db;

    /// <summary>Роли сотрудников компании (без клиентов)</summary>
    private static readonly UserRole[] EmployeeRoles =
    {
        UserRole.Technician, UserRole.Engineer, UserRole.ChiefEngineer,
        UserRole.Logist, UserRole.ManagerTimewise, UserRole.Moderator
    };

    public DirectChatService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ChatListItemDto>> GetUserChatsAsync(int userId)
    {
        // Все чаты, где пользователь является участником
        var chatIds = await _db.CompanyChatMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.CompanyChatId)
            .ToListAsync();

        if (chatIds.Count == 0)
            return Enumerable.Empty<ChatListItemDto>();

        var chats = await _db.CompanyChats
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Messages.OrderByDescending(msg => msg.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
            .Where(c => chatIds.Contains(c.Id) && c.IsActive)
            .ToListAsync();

        var result = new List<ChatListItemDto>();

        foreach (var chat in chats)
        {
            var lastMessage = chat.Messages.FirstOrDefault();
            var unreadCount = await _db.ChatMessages
                .CountAsync(m => m.CompanyChatId == chat.Id && m.SenderId != userId && !m.IsRead);

            string name;
            string initials;
            string? avatarUrl = null;

            if (chat.IsDirectChat)
            {
                // Для личного чата — имя собеседника
                var otherMember = chat.Members.FirstOrDefault(m => m.UserId != userId)?.User;
                if (otherMember != null)
                {
                    name = otherMember.FullName;
                    avatarUrl = otherMember.AvatarUrl;
                    var first = !string.IsNullOrEmpty(otherMember.FirstName) ? otherMember.FirstName[0].ToString() : "";
                    var last = !string.IsNullOrEmpty(otherMember.LastName) ? otherMember.LastName[0].ToString() : "";
                    initials = (last + first).ToUpper();
                }
                else
                {
                    name = "Чат";
                    initials = "?";
                }
            }
            else
            {
                // Групповой чат компании
                name = chat.Name;
                initials = "ЧК";
            }

            result.Add(new ChatListItemDto
            {
                ChatId = chat.Id,
                Name = name,
                AvatarUrl = avatarUrl,
                Initials = initials,
                LastMessage = lastMessage?.Text,
                LastMessageSender = lastMessage?.Sender?.FullName,
                LastMessageAt = lastMessage?.CreatedAt,
                UnreadCount = unreadCount,
                IsDirectChat = chat.IsDirectChat
            });
        }

        // Сортируем: сначала чаты с последними сообщениями
        return result.OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue);
    }

    public async Task<int> GetOrCreateDirectChatAsync(int userId1, int userId2)
    {
        // Ищем существующий личный чат между этими пользователями
        var existingChat = await _db.CompanyChats
            .Include(c => c.Members)
            .Where(c => c.IsDirectChat && c.IsActive)
            .Where(c => c.Members.Any(m => m.UserId == userId1) && c.Members.Any(m => m.UserId == userId2))
            .FirstOrDefaultAsync();

        if (existingChat != null)
            return existingChat.Id;

        // Создаём новый личный чат
        var user2 = await _db.Users.FindAsync(userId2);
        var chat = new CompanyChat
        {
            Name = user2?.FullName ?? "Личный чат",
            IsActive = true,
            IsDirectChat = true
        };

        _db.CompanyChats.Add(chat);
        await _db.SaveChangesAsync();

        // Добавляем обоих участников
        _db.CompanyChatMembers.Add(new CompanyChatMember { CompanyChatId = chat.Id, UserId = userId1 });
        _db.CompanyChatMembers.Add(new CompanyChatMember { CompanyChatId = chat.Id, UserId = userId2 });
        await _db.SaveChangesAsync();

        return chat.Id;
    }

    public async Task<IEnumerable<CompanyChatMemberDto>> GetAvailableEmployeesAsync(int currentUserId)
    {
        return await _db.Users
            .Where(u => u.IsActive && EmployeeRoles.Contains(u.Role) && u.Id != currentUserId)
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Select(u => new CompanyChatMemberDto
            {
                UserId = u.Id,
                FullName = u.FullName,
                Role = u.Role,
                AvatarUrl = u.AvatarUrl
            })
            .ToListAsync();
    }
}
