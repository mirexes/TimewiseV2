namespace ServiceDesk.Core.Entities;

/// <summary>
/// Подписка пользователя на Web Push уведомления
/// </summary>
public class PushSubscription : BaseEntity
{
    /// <summary>Endpoint подписки</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Ключ шифрования P256DH</summary>
    public string P256dh { get; set; } = string.Empty;

    /// <summary>Ключ аутентификации</summary>
    public string Auth { get; set; } = string.Empty;

    /// <summary>Пользователь-владелец подписки</summary>
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
}
