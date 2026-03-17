using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Users;

/// <summary>
/// DTO пользователя для списка
/// </summary>
public class UserListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public string? Company { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
