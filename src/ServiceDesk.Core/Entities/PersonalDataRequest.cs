using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Entities;

/// <summary>
/// Запрос субъекта ПД на доступ / исправление / удаление данных (ФЗ-152, ст. 14–17)
/// </summary>
public class PersonalDataRequest : BaseEntity
{
    /// <summary>Пользователь-заявитель</summary>
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>Тип запроса</summary>
    public PersonalDataRequestType RequestType { get; set; }

    /// <summary>Статус запроса</summary>
    public PersonalDataRequestStatus Status { get; set; } = PersonalDataRequestStatus.New;

    /// <summary>Описание запроса от пользователя</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Ответ оператора</summary>
    public string? Response { get; set; }

    /// <summary>Кто обработал запрос (модератор)</summary>
    public int? ProcessedByUserId { get; set; }
    public AppUser? ProcessedByUser { get; set; }

    /// <summary>Дата обработки</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Крайний срок обработки (30 дней по ФЗ-152)</summary>
    public DateTime Deadline { get; set; }
}
