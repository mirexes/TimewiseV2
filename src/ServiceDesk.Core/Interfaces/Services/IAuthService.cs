using ServiceDesk.Core.DTOs.Auth;
using ServiceDesk.Core.Entities;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис аутентификации по SMS-коду
/// </summary>
public interface IAuthService
{
    Task<bool> SendCodeAsync(SendCodeDto dto);
    Task<AppUser?> VerifyCodeAsync(VerifyCodeDto dto);

    /// <summary>
    /// Получение текущего SMS-кода по номеру телефона (только для разработки)
    /// </summary>
    Task<string?> GetSmsCodeAsync(string phone);
}
