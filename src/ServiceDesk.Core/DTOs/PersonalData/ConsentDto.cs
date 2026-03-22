using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.DTOs.PersonalData;

/// <summary>
/// Информация о согласии пользователя
/// </summary>
public class ConsentDto
{
    public int Id { get; set; }
    public ConsentType ConsentType { get; set; }
    public string ConsentTypeName { get; set; } = string.Empty;
    public string ConsentVersion { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Запрос на предоставление согласия
/// </summary>
public class GrantConsentDto
{
    public ConsentType ConsentType { get; set; }
    public bool Agree { get; set; }
}

/// <summary>
/// Информация о персональных данных пользователя для экспорта
/// </summary>
public class PersonalDataExportDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<ConsentDto> Consents { get; set; } = new();
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Запрос субъекта ПД
/// </summary>
public class PersonalDataRequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public PersonalDataRequestType RequestType { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
    public PersonalDataRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Response { get; set; }
    public string? ProcessedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime Deadline { get; set; }
    public bool IsOverdue => Status is PersonalDataRequestStatus.New or PersonalDataRequestStatus.InProgress
                             && DateTime.UtcNow > Deadline;
}

/// <summary>
/// Создание запроса субъекта ПД
/// </summary>
public class CreatePersonalDataRequestDto
{
    public PersonalDataRequestType RequestType { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Обработка запроса субъекта ПД (модератор)
/// </summary>
public class ProcessPersonalDataRequestDto
{
    public PersonalDataRequestStatus Status { get; set; }
    public string Response { get; set; } = string.Empty;
}
