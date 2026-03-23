using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Auth;

/// <summary>
/// DTO для запроса SMS-кода
/// </summary>
public class SendCodeDto
{
    [Required(ErrorMessage = "Укажите номер телефона")]
    [RegularExpression(@"^\+\d{10,15}$", ErrorMessage = "Номер должен начинаться с + и содержать код страны, например +79618934486")]
    public string Phone { get; set; } = string.Empty;
}
