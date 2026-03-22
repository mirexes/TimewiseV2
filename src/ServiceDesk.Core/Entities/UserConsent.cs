using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Entities;

/// <summary>
/// Согласие пользователя на обработку персональных данных (ФЗ-152, ст. 9)
/// </summary>
public class UserConsent : BaseEntity
{
    /// <summary>Пользователь, давший согласие</summary>
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>Тип согласия</summary>
    public ConsentType ConsentType { get; set; }

    /// <summary>Версия текста согласия на момент подписания</summary>
    public string ConsentVersion { get; set; } = string.Empty;

    /// <summary>Текст согласия, с которым ознакомился пользователь</summary>
    public string ConsentText { get; set; } = string.Empty;

    /// <summary>Согласие дано</summary>
    public bool IsGranted { get; set; }

    /// <summary>Дата предоставления согласия</summary>
    public DateTime GrantedAt { get; set; }

    /// <summary>Дата отзыва согласия (null — действует)</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>IP-адрес при подписании</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent при подписании</summary>
    public string? UserAgent { get; set; }

    /// <summary>Активно ли согласие</summary>
    public bool IsActive => IsGranted && RevokedAt is null;
}
