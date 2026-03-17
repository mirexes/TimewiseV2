namespace ServiceDesk.Core.DTOs.Clients;

/// <summary>
/// Фильтр для списка клиентов
/// </summary>
public class ClientFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
