namespace ServiceDesk.Core.Enums;

/// <summary>
/// Типы действий для журнала аудита
/// </summary>
public enum AuditAction
{
    /// <summary>Создание</summary>
    Created = 0,

    /// <summary>Обновление</summary>
    Updated = 1,

    /// <summary>Удаление</summary>
    Deleted = 2,

    /// <summary>Смена статуса</summary>
    StatusChanged = 3,

    /// <summary>Назначение специалиста</summary>
    Assigned = 4,

    /// <summary>Вход в систему</summary>
    Login = 5,

    /// <summary>Выход из системы</summary>
    Logout = 6,

    /// <summary>Согласование запчастей</summary>
    PartsApproved = 7,

    /// <summary>Отклонение запчастей</summary>
    PartsRejected = 8,

    /// <summary>Согласие на обработку ПД</summary>
    ConsentGranted = 9,

    /// <summary>Отзыв согласия на обработку ПД</summary>
    ConsentRevoked = 10,

    /// <summary>Запрос на доступ к ПД</summary>
    PersonalDataRequested = 11,

    /// <summary>Экспорт персональных данных</summary>
    PersonalDataExported = 12,

    /// <summary>Удаление персональных данных</summary>
    PersonalDataDeleted = 13
}
