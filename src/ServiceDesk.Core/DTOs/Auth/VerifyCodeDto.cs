using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.DTOs.Auth;

/// <summary>
/// DTO для проверки SMS-кода
/// </summary>
public class VerifyCodeDto
{
    [Required(ErrorMessage = "Укажите номер телефона")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите код")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "Код должен быть 4-6 символов")]
    public string Code { get; set; } = string.Empty;
}
