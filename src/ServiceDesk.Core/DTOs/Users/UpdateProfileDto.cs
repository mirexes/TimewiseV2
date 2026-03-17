using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Users;

/// <summary>
/// DTO для редактирования профиля пользователем (без роли и статуса)
/// </summary>
public class UpdateProfileDto
{
    [Required(ErrorMessage = "Фамилия обязательна")]
    [MaxLength(100, ErrorMessage = "Максимум 100 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имя обязательно")]
    [MaxLength(100, ErrorMessage = "Максимум 100 символов")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Максимум 100 символов")]
    public string? MiddleName { get; set; }

    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
    public string? Company { get; set; }
}
