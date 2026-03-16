namespace ServiceDesk.Core.Enums;

/// <summary>
/// Роли пользователей (8 ролей)
/// </summary>
public enum UserRole
{
    /// <summary>Техник — выездной, видит только свои заявки</summary>
    Technician = 0,

    /// <summary>Инженер — аналогично технику</summary>
    Engineer = 1,

    /// <summary>Главный инженер — видит все заявки, переназначает</summary>
    ChiefEngineer = 2,

    /// <summary>Логист — создание, обработка, назначение</summary>
    Logist = 3,

    /// <summary>Менеджер Timewise — согласование, отчёты</summary>
    ManagerTimewise = 4,

    /// <summary>Клиент — только свои заявки (роль по умолчанию)</summary>
    Client = 5,

    /// <summary>Менеджер клиента — привязан к сети, согласует запчасти</summary>
    ManagerClient = 6,

    /// <summary>Модератор — полный доступ</summary>
    Moderator = 7
}
