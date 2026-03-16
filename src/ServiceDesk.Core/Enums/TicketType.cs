using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.Enums;

/// <summary>
/// Тип заявки
/// </summary>
public enum TicketType
{
    /// <summary>Ремонт</summary>
    [Display(Name = "Ремонт")]
    Repair = 0,

    /// <summary>Техническое обслуживание</summary>
    [Display(Name = "Техническое обслуживание")]
    Maintenance = 1,

    /// <summary>Установка</summary>
    [Display(Name = "Установка")]
    Installation = 2,

    /// <summary>Демонтаж</summary>
    [Display(Name = "Демонтаж")]
    Dismantling = 3,

    /// <summary>Поставка</summary>
    [Display(Name = "Поставка")]
    Delivery = 4,

    /// <summary>Консультация</summary>
    [Display(Name = "Консультация")]
    Consultation = 5
}
