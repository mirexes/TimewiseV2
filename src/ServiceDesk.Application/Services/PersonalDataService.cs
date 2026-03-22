using Microsoft.EntityFrameworkCore;
using ServiceDesk.Core.DTOs.PersonalData;
using ServiceDesk.Core.Entities;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Infrastructure.Data;

namespace ServiceDesk.Application.Services;

/// <summary>
/// Сервис управления персональными данными (ФЗ-152)
/// </summary>
public class PersonalDataService : IPersonalDataService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    /// <summary>Текущая версия текста согласия</summary>
    private const string CurrentConsentVersion = "1.0";

    public PersonalDataService(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    // === Согласия ===

    public async Task<List<ConsentDto>> GetUserConsentsAsync(int userId)
    {
        return await _db.UserConsents
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.GrantedAt)
            .Select(c => new ConsentDto
            {
                Id = c.Id,
                ConsentType = c.ConsentType,
                ConsentTypeName = GetConsentTypeName(c.ConsentType),
                ConsentVersion = c.ConsentVersion,
                IsGranted = c.IsGranted,
                GrantedAt = c.GrantedAt,
                RevokedAt = c.RevokedAt,
                IsActive = c.IsGranted && c.RevokedAt == null
            })
            .ToListAsync();
    }

    public async Task GrantConsentAsync(int userId, ConsentType consentType, string ipAddress, string userAgent)
    {
        // Отзываем предыдущие согласия этого типа
        var existing = await _db.UserConsents
            .Where(c => c.UserId == userId && c.ConsentType == consentType && c.RevokedAt == null)
            .ToListAsync();

        foreach (var old in existing)
            old.RevokedAt = DateTime.UtcNow;

        var consent = new UserConsent
        {
            UserId = userId,
            ConsentType = consentType,
            ConsentVersion = CurrentConsentVersion,
            ConsentText = GetConsentText(consentType),
            IsGranted = true,
            GrantedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _db.UserConsents.Add(consent);

        // Обновляем дату согласия в профиле пользователя
        if (consentType == ConsentType.PersonalDataProcessing)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is not null)
                user.PersonalDataConsentAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.ConsentGranted, "UserConsent", consent.Id,
            null, $"Тип: {GetConsentTypeName(consentType)}, версия: {CurrentConsentVersion}", userId);
    }

    public async Task RevokeConsentAsync(int userId, ConsentType consentType)
    {
        var consents = await _db.UserConsents
            .Where(c => c.UserId == userId && c.ConsentType == consentType && c.RevokedAt == null)
            .ToListAsync();

        foreach (var consent in consents)
            consent.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        foreach (var consent in consents)
        {
            await _audit.LogAsync(AuditAction.ConsentRevoked, "UserConsent", consent.Id,
                $"Тип: {GetConsentTypeName(consentType)}", null, userId);
        }
    }

    public async Task<bool> HasActiveConsentAsync(int userId, ConsentType consentType)
    {
        return await _db.UserConsents
            .AnyAsync(c => c.UserId == userId
                        && c.ConsentType == consentType
                        && c.IsGranted
                        && c.RevokedAt == null);
    }

    // === Экспорт ===

    public async Task<PersonalDataExportDto> ExportPersonalDataAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) throw new InvalidOperationException("Пользователь не найден");

        var consents = await GetUserConsentsAsync(userId);

        await _audit.LogAsync(AuditAction.PersonalDataExported, "AppUser", userId, null, null, userId);

        return new PersonalDataExportDto
        {
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            Company = user.Company,
            Role = GetRoleName(user.Role),
            RegisteredAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Consents = consents
        };
    }

    // === Запросы субъекта ПД ===

    public async Task<int> CreateRequestAsync(int userId, CreatePersonalDataRequestDto dto)
    {
        var request = new PersonalDataRequest
        {
            UserId = userId,
            RequestType = dto.RequestType,
            Description = dto.Description,
            Status = PersonalDataRequestStatus.New,
            Deadline = DateTime.UtcNow.AddDays(30) // ФЗ-152: 30 дней на обработку
        };

        _db.PersonalDataRequests.Add(request);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.PersonalDataRequested, "PersonalDataRequest", request.Id,
            null, $"Тип: {GetRequestTypeName(dto.RequestType)}", userId);

        return request.Id;
    }

    public async Task<List<PersonalDataRequestDto>> GetUserRequestsAsync(int userId)
    {
        return await _db.PersonalDataRequests
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();
    }

    public async Task<List<PersonalDataRequestDto>> GetAllRequestsAsync()
    {
        return await _db.PersonalDataRequests
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapToDto(r))
            .ToListAsync();
    }

    public async Task<PersonalDataRequestDto?> GetRequestByIdAsync(int requestId)
    {
        var request = await _db.PersonalDataRequests
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        return request is null ? null : MapToDto(request);
    }

    public async Task ProcessRequestAsync(int requestId, int moderatorId, ProcessPersonalDataRequestDto dto)
    {
        var request = await _db.PersonalDataRequests.FindAsync(requestId);
        if (request is null) throw new InvalidOperationException("Запрос не найден");

        request.Status = dto.Status;
        request.Response = dto.Response;
        request.ProcessedByUserId = moderatorId;
        request.ProcessedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // === Анонимизация ===

    public async Task AnonymizeUserDataAsync(int userId, int moderatorId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) throw new InvalidOperationException("Пользователь не найден");

        // Сохраняем данные для аудита перед анонимизацией
        var oldData = $"ФИО: {user.FullName}, Телефон: {user.Phone}, Email: {user.Email}";

        // Анонимизация данных
        user.FirstName = "Удалён";
        user.LastName = "Пользователь";
        user.MiddleName = null;
        user.Phone = $"deleted_{userId}_{DateTime.UtcNow.Ticks}";
        user.Email = null;
        user.Company = null;
        user.AvatarUrl = null;
        user.IsActive = false;
        user.SmsCode = null;

        // Отзываем все согласия
        var consents = await _db.UserConsents
            .Where(c => c.UserId == userId && c.RevokedAt == null)
            .ToListAsync();
        foreach (var consent in consents)
            consent.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(AuditAction.PersonalDataDeleted, "AppUser", userId,
            oldData, "Данные анонимизированы по запросу субъекта ПД", moderatorId);
    }

    // === Вспомогательные методы ===

    private static PersonalDataRequestDto MapToDto(PersonalDataRequest r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        UserName = r.User.FullName,
        UserPhone = r.User.Phone,
        RequestType = r.RequestType,
        RequestTypeName = GetRequestTypeName(r.RequestType),
        Status = r.Status,
        StatusName = GetRequestStatusName(r.Status),
        Description = r.Description,
        Response = r.Response,
        ProcessedByName = r.ProcessedByUser?.FullName,
        CreatedAt = r.CreatedAt,
        ProcessedAt = r.ProcessedAt,
        Deadline = r.Deadline
    };

    internal static string GetConsentTypeName(ConsentType type) => type switch
    {
        ConsentType.PersonalDataProcessing => "Обработка персональных данных",
        ConsentType.SmsNotifications => "SMS-уведомления",
        ConsentType.PushNotifications => "Push-уведомления",
        ConsentType.ThirdPartyTransfer => "Передача данных третьим лицам",
        _ => type.ToString()
    };

    private static string GetRequestTypeName(PersonalDataRequestType type) => type switch
    {
        PersonalDataRequestType.Access => "Доступ к данным",
        PersonalDataRequestType.Rectification => "Исправление данных",
        PersonalDataRequestType.Deletion => "Удаление данных",
        PersonalDataRequestType.ConsentWithdrawal => "Отзыв согласия",
        _ => type.ToString()
    };

    private static string GetRequestStatusName(PersonalDataRequestStatus status) => status switch
    {
        PersonalDataRequestStatus.New => "Новый",
        PersonalDataRequestStatus.InProgress => "В обработке",
        PersonalDataRequestStatus.Completed => "Выполнен",
        PersonalDataRequestStatus.Rejected => "Отклонён",
        _ => status.ToString()
    };

    private static string GetRoleName(UserRole role) => role switch
    {
        UserRole.Technician => "Техник",
        UserRole.Engineer => "Инженер",
        UserRole.ChiefEngineer => "Главный инженер",
        UserRole.Logist => "Логист",
        UserRole.ManagerTimewise => "Менеджер Timewise",
        UserRole.ManagerClient => "Менеджер клиента",
        UserRole.Client => "Клиент",
        UserRole.Moderator => "Модератор",
        _ => role.ToString()
    };

    private static string GetConsentText(ConsentType type) => type switch
    {
        ConsentType.PersonalDataProcessing =>
            "Я даю согласие на обработку моих персональных данных (ФИО, номер телефона, адрес электронной почты) " +
            "оператору ООО «TEAMWISE» в соответствии с Федеральным законом от 27.07.2006 № 152-ФЗ " +
            "«О персональных данных» в целях предоставления сервиса управления заявками на обслуживание оборудования. " +
            "Согласие действует до момента его отзыва.",
        ConsentType.SmsNotifications =>
            "Я даю согласие на получение SMS-уведомлений о статусе заявок и иных сервисных сообщений " +
            "на указанный мной номер телефона.",
        ConsentType.PushNotifications =>
            "Я даю согласие на получение Push-уведомлений в браузере о статусе заявок и иных сервисных сообщений.",
        ConsentType.ThirdPartyTransfer =>
            "Я даю согласие на передачу моих персональных данных третьим лицам (сервисным организациям, подрядчикам) " +
            "в объёме, необходимом для выполнения заявок на обслуживание оборудования.",
        _ => string.Empty
    };
}
