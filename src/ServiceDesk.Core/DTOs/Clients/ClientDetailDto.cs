namespace ServiceDesk.Core.DTOs.Clients;

/// <summary>
/// DTO клиента с детальной информацией
/// </summary>
public class ClientDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Inn { get; set; }
    public string? Network { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LegalAddress { get; set; }
    public bool IsActive { get; set; }
    public string? TtkFilePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Точки обслуживания</summary>
    public List<ClientServicePointDto> ServicePoints { get; set; } = new();

    /// <summary>Контактные лица</summary>
    public List<ClientContactPersonDto> ContactPersons { get; set; } = new();

    /// <summary>Менеджеры клиента</summary>
    public List<ClientManagerDto> Managers { get; set; } = new();
}

/// <summary>
/// Точка обслуживания клиента (вложенное DTO)
/// </summary>
public class ClientServicePointDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Region { get; set; }
    public bool IsActive { get; set; }
    public int EquipmentCount { get; set; }
}

/// <summary>
/// Контактное лицо клиента (вложенное DTO)
/// </summary>
public class ClientContactPersonDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Position { get; set; }
}

/// <summary>
/// Менеджер клиента (вложенное DTO)
/// </summary>
public class ClientManagerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}
