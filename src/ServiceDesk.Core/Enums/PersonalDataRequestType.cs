namespace ServiceDesk.Core.Enums;

/// <summary>
/// Типы запросов субъекта ПД (ФЗ-152, ст. 14–17)
/// </summary>
public enum PersonalDataRequestType
{
    /// <summary>Запрос на доступ к данным (ст. 14)</summary>
    Access = 0,

    /// <summary>Запрос на исправление данных (ст. 14)</summary>
    Rectification = 1,

    /// <summary>Запрос на удаление данных (ст. 14)</summary>
    Deletion = 2,

    /// <summary>Отзыв согласия (ст. 9)</summary>
    ConsentWithdrawal = 3
}
