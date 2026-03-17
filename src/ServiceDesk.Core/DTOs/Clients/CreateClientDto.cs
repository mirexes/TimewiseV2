using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Clients;

/// <summary>
/// DTO для создания/редактирования клиента
/// </summary>
public class CreateClientDto
{
    [Required(ErrorMessage = "Укажите наименование организации")]
    [MaxLength(300, ErrorMessage = "Максимум 300 символов")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(12, ErrorMessage = "ИНН не более 12 символов")]
    public string? Inn { get; set; }

    [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
    public string? Network { get; set; }

    [MaxLength(20, ErrorMessage = "Максимум 20 символов")]
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    public string? Phone { get; set; }

    [MaxLength(255, ErrorMessage = "Максимум 255 символов")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string? Email { get; set; }

    [MaxLength(500, ErrorMessage = "Максимум 500 символов")]
    public string? LegalAddress { get; set; }

    public bool IsActive { get; set; } = true;
}
