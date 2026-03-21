using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.DTOs.Auth;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис аутентификации по SMS-коду
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ISmsService _sms;

    public AuthService(AppDbContext db, ISmsService sms)
    {
        _db = db;
        _sms = sms;
    }

    public async Task<bool> SendCodeAsync(SendCodeDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone);

        // Если пользователя нет — создаём с ролью Клиент
        if (user is null)
        {
            user = new AppUser
            {
                Phone = dto.Phone,
                FirstName = "Новый",
                LastName = "Пользователь",
                Role = UserRole.Client
            };
            _db.Users.Add(user);
        }

        // Проверка блокировки
        if (user.SmsBlockedUntil.HasValue && user.SmsBlockedUntil > DateTime.UtcNow)
            return false;

        // Проверка частоты отправки (не чаще 1 раз/мин)
        if (user.SmsCodeSentAt.HasValue &&
            (DateTime.UtcNow - user.SmsCodeSentAt.Value).TotalSeconds < 60)
            return false;

        // Генерация кода
        var code = Random.Shared.Next(1000, 9999).ToString();
        user.SmsCode = code;
        user.SmsCodeSentAt = DateTime.UtcNow;
        user.SmsAttempts = 0;

        await _db.SaveChangesAsync();
        await _sms.SendAsync(dto.Phone, $"Код для входа в ServiceDesk: {code}");

        return true;
    }

    public async Task<AppUser?> VerifyCodeAsync(VerifyCodeDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone);
        if (user is null) return null;

        // Проверка блокировки
        if (user.SmsBlockedUntil.HasValue && user.SmsBlockedUntil > DateTime.UtcNow)
            return null;

        // Проверка количества попыток
        user.SmsAttempts++;
        if (user.SmsAttempts > 5)
        {
            user.SmsBlockedUntil = DateTime.UtcNow.AddMinutes(20);
            await _db.SaveChangesAsync();
            return null;
        }

        // Проверка кода
        if (user.SmsCode != dto.Code)
        {
            await _db.SaveChangesAsync();
            return null;
        }

        // Успешная авторизация
        user.SmsCode = null;
        user.SmsAttempts = 0;
        user.SmsBlockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<string?> GetSmsCodeAsync(string phone)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        return user?.SmsCode;
    }
}
