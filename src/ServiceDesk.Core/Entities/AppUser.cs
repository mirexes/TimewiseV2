using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Entities;

/// <summary>
/// Пользователь системы
/// </summary>
public class AppUser : BaseEntity
{
    /// <summary>Фамилия</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Имя</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Отчество</summary>
    public string? MiddleName { get; set; }

    /// <summary>Номер телефона (логин)</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Email</summary>
    public string? Email { get; set; }

    /// <summary>Роль пользователя</summary>
    public UserRole Role { get; set; } = UserRole.Client;

    /// <summary>Название компании (для клиентов)</summary>
    public string? Company { get; set; }

    /// <summary>Активен ли аккаунт</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Последний вход</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>SMS-код для авторизации</summary>
    public string? SmsCode { get; set; }

    /// <summary>Время отправки SMS-кода</summary>
    public DateTime? SmsCodeSentAt { get; set; }

    /// <summary>Количество попыток ввода кода</summary>
    public int SmsAttempts { get; set; }

    /// <summary>Время блокировки SMS</summary>
    public DateTime? SmsBlockedUntil { get; set; }

    /// <summary>Путь к аватарке пользователя</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Привязка к клиенту (для менеджера клиента)</summary>
    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    /// <summary>Полное ФИО</summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>Дата согласия на обработку ПД (ФЗ-152)</summary>
    public DateTime? PersonalDataConsentAt { get; set; }

    // Навигационные свойства
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
    public ICollection<PersonalDataRequest> PersonalDataRequests { get; set; } = new List<PersonalDataRequest>();
}
