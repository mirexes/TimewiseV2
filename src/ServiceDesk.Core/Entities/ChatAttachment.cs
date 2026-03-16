namespace ServiceDesk.Core.Entities;

/// <summary>
/// Вложение к сообщению чата
/// </summary>
public class ChatAttachment : BaseEntity
{
    /// <summary>Имя файла</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Путь к файлу на сервере</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>MIME-тип</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Размер файла в байтах</summary>
    public long FileSize { get; set; }

    public int ChatMessageId { get; set; }
    public ChatMessage ChatMessage { get; set; } = null!;
}
