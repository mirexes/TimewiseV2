using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Auth;

/// <summary>
/// DTO для запроса SMS-кода
/// </summary>
public class SendCodeDto
{
    [Required(ErrorMessage = "Укажите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер телефона")]
    public string Phone { get; set; } = string.Empty;
}
