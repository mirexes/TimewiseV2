namespace ServiceDesk.Core.DTOs.Tickets;

/// <summary>
/// Данные сохранённого файла-вложения к заявке
/// </summary>
public class TicketAttachmentFile
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
