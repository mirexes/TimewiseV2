namespace ServiceDesk.Core.Enums;

/// <summary>
/// Статусы заявки (11 статусов)
/// </summary>
public enum TicketStatus
{
    /// <summary>Новая</summary>
    New = 0,

    /// <summary>Обработана</summary>
    Processed = 1,

    /// <summary>Выполнена дистанционно</summary>
    CompletedRemotely = 2,

    /// <summary>Согласование запчастей</summary>
    PartsApproval = 3,

    /// <summary>Ремонт согласован</summary>
    RepairApproved = 4,

    /// <summary>Подтверждение выезда</summary>
    DepartureConfirmation = 5,

    /// <summary>Техник в пути</summary>
    EngineerEnRoute = 6,

    /// <summary>Выполнение</summary>
    InProgress = 7,

    /// <summary>Выполнена полностью</summary>
    Completed = 8,

    /// <summary>Требуется продолжение работ</summary>
    ContinuationRequired = 9,

    /// <summary>Отменена</summary>
    Cancelled = 10
}
