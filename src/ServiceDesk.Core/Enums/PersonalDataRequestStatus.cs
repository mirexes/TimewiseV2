namespace ServiceDesk.Core.Enums;

/// <summary>
/// Статусы запросов на обработку ПД
/// </summary>
public enum PersonalDataRequestStatus
{
    /// <summary>Новый запрос</summary>
    New = 0,

    /// <summary>В обработке</summary>
    InProgress = 1,

    /// <summary>Выполнен</summary>
    Completed = 2,

    /// <summary>Отклонён</summary>
    Rejected = 3
}
