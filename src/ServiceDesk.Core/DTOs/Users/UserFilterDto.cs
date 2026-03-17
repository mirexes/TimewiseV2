using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Users;

/// <summary>
/// Фильтр для списка пользователей
/// </summary>
public class UserFilterDto
{
    public string? Search { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
