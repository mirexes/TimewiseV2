using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Clients;

/// <summary>
/// DTO для создания/редактирования контактного лица
/// </summary>
public class CreateContactPersonDto
{
    [Required(ErrorMessage = "Укажите ФИО")]
    [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите телефон")]
    [MaxLength(20, ErrorMessage = "Максимум 20 символов")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Максимум 255 символов")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
    public string? Position { get; set; }
}
