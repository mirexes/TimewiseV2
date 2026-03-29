namespace ServiceDesk.Core.DTOs.Chat;

/// <summary>
/// Элемент списка чатов
/// </summary>
public class ChatListItemDto
{
    public int ChatId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Initials { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public string? LastMessageSender { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsDirectChat { get; set; }
}
