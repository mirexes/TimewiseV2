namespace ServiceDesk.Core.DTOs.Common;

/// <summary>
/// Элемент для выпадающих списков
/// </summary>
public class SelectOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
