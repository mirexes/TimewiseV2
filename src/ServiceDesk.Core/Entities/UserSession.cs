namespace ServiceDesk.Core.Entities;

/// <summary>
/// Сессия пользователя — хранит аутентификационный тикет в БД.
/// Кука содержит только SessionToken, все данные — в базе.
/// </summary>
public class UserSession : BaseEntity
{
    /// <summary>Уникальный токен сессии (хранится в куке)</summary>
    public string SessionToken { get; set; } = string.Empty;

    /// <summary>Сериализованный AuthenticationTicket (Base64)</summary>
    public byte[] TicketData { get; set; } = Array.Empty<byte>();

    /// <summary>Время истечения сессии</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Время последнего запроса</summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>IP-адрес клиента при создании сессии</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent браузера</summary>
    public string? UserAgent { get; set; }

    /// <summary>Сессия отозвана (принудительный разлогин)</summary>
    public bool IsRevoked { get; set; }

    /// <summary>Привязка к пользователю</summary>
    public int UserId { get; set; }
    public AppUser? User { get; set; }
}
