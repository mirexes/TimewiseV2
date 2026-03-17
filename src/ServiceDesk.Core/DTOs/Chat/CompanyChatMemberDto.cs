using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Chat;

/// <summary>
/// DTO участника группового чата компании
/// </summary>
public class CompanyChatMemberDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
}
