using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Core.Enums;

/// <summary>
/// Приоритет заявки
/// </summary>
public enum TicketPriority
{
    /// <summary>Низкий</summary>
    [Display(Name = "Низкий")]
    Low = 0,

    /// <summary>Обычный</summary>
    [Display(Name = "Обычный")]
    Normal = 1,

    /// <summary>Высокий</summary>
    [Display(Name = "Высокий")]
    High = 2,

    /// <summary>Критичный</summary>
    [Display(Name = "Критичный")]
    Critical = 3
}
