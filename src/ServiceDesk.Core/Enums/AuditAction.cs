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
    PartsRejected = 8
}
