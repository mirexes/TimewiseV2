namespace ServiceDesk.Core.Enums;

/// <summary>
/// Тип заявки
/// </summary>
public enum TicketType
{
    /// <summary>Ремонт</summary>
    Repair = 0,

    /// <summary>Техническое обслуживание</summary>
    Maintenance = 1,

    /// <summary>Установка</summary>
    Installation = 2,

    /// <summary>Демонтаж</summary>
    Dismantling = 3,

    /// <summary>Поставка</summary>
    Delivery = 4,

    /// <summary>Консультация</summary>
    Consultation = 5
}
