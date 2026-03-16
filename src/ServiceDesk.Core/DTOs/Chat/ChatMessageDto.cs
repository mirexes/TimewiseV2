namespace ServiceDesk.Core.DTOs.Chat;

/// <summary>
/// DTO сообщения чата
/// </summary>
public class ChatMessageDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public List<ChatAttachmentDto> Attachments { get; set; } = new();
}

public class ChatAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
