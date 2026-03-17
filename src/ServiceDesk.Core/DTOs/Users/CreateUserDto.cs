using System.ComponentModel.DataAnnotations;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Users;

/// <summary>
/// DTO для создания/редактирования пользователя
/// </summary>
public class CreateUserDto
{
    [Required(ErrorMessage = "Фамилия обязательна")]
    [MaxLength(100, ErrorMessage = "Максимум 100 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имя обязательно")]
    [MaxLength(100, ErrorMessage = "Максимум 100 символов")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Максимум 100 символов")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Телефон обязателен")]
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Роль обязательна")]
    public UserRole Role { get; set; } = UserRole.Client;

    [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
    public string? Company { get; set; }

    /// <summary>Привязка к клиенту (для менеджера клиента)</summary>
    public int? ClientId { get; set; }

    public bool IsActive { get; set; } = true;
}
