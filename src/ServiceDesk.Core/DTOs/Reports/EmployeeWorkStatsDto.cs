namespace ServiceDesk.Core.DTOs.Reports;

/// <summary>
/// Статистика наработки одного сотрудника за месяц
/// </summary>
public class EmployeeWorkStatsDto
{
    /// <summary>Идентификатор сотрудника</summary>
    public int EmployeeId { get; set; }

    /// <summary>ФИО сотрудника</summary>
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>Должность (роль) сотрудника</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>Отработано часов (время в статусе «Выполнение»)</summary>
    public double WorkedHours { get; set; }

    /// <summary>Часов в дороге (время в статусе «Техник в пути»)</summary>
    public double TravelHours { get; set; }

    /// <summary>Завершённых заявок за месяц</summary>
    public int CompletedTickets { get; set; }

    /// <summary>Заявок с активностью за месяц (выезд или работа)</summary>
    public int HandledTickets { get; set; }
}

/// <summary>
/// Отчёт по наработке сотрудников за календарный месяц
/// </summary>
public class EmployeeWorkReportDto
{
    /// <summary>Год отчётного периода</summary>
    public int Year { get; set; }

    /// <summary>Месяц отчётного периода (1–12)</summary>
    public int Month { get; set; }

    /// <summary>Начало периода (всегда 1-е число месяца, UTC)</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Конец периода (1-е число следующего месяца, UTC)</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>Список сотрудников с показателями наработки</summary>
    public List<EmployeeWorkStatsDto> Employees { get; set; } = new();

    /// <summary>Суммарно отработано часов всеми сотрудниками</summary>
    public double TotalWorkedHours => Math.Round(Employees.Sum(e => e.WorkedHours), 1);

    /// <summary>Суммарно часов в дороге у всех сотрудников</summary>
    public double TotalTravelHours => Math.Round(Employees.Sum(e => e.TravelHours), 1);
}
