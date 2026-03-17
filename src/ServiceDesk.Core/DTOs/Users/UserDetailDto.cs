using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Users;

/// <summary>
/// DTO пользователя с детальной информацией
/// </summary>
public class UserDetailDto
{
    public int Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public string? Company { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Привязка к клиенту (для менеджера клиента)</summary>
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }

    /// <summary>Путь к аватарке</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Количество назначенных заявок</summary>
    public int AssignedTicketsCount { get; set; }
}
