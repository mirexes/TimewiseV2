using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.Tickets;

/// <summary>
/// DTO для списка заявок (облегчённый)
/// </summary>
public class TicketListDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? EngineerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }

    /// <summary>Путь к фото АВР</summary>
    public string? AvrPhotoPath { get; set; }

    /// <summary>Фото акта выполненных работ</summary>
    public List<TicketPhotoDto> CompletionPhotos { get; set; } = new();

    /// <summary>Есть ли прикреплённый акт выполненных работ</summary>
    public bool HasWorkAct => !string.IsNullOrEmpty(AvrPhotoPath) || CompletionPhotos.Count > 0;

    /// <summary>Отображаемое название статуса</summary>
    public string StatusDisplayName => Status switch
    {
        TicketStatus.New => "Новая",
        TicketStatus.Processed => "Обработана",
        TicketStatus.CompletedRemotely => "Дистанционно",
        TicketStatus.PartsApproval => "Согласование",
        TicketStatus.RepairApproved => "Согласована",
        TicketStatus.DepartureConfirmation => "Подтверждение выезда",
        TicketStatus.EngineerEnRoute => "В пути",
        TicketStatus.InProgress => "Выполнение",
        TicketStatus.Completed => "Выполнена",
        TicketStatus.ContinuationRequired => "Продолжение",
        TicketStatus.Cancelled => "Отменена",
        _ => Status.ToString()
    };
}
