using ServiceDesk.Core.DTOs.PersonalData;
using ServiceDesk.Core.Enums;

namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис управления персональными данными (ФЗ-152)
/// </summary>
public interface IPersonalDataService
{
    // === Согласия ===

    /// <summary>Получить все согласия пользователя</summary>
    Task<List<ConsentDto>> GetUserConsentsAsync(int userId);

    /// <summary>Предоставить согласие</summary>
    Task GrantConsentAsync(int userId, ConsentType consentType, string ipAddress, string userAgent);

    /// <summary>Отозвать согласие</summary>
    Task RevokeConsentAsync(int userId, ConsentType consentType);

    /// <summary>Проверить наличие активного согласия</summary>
    Task<bool> HasActiveConsentAsync(int userId, ConsentType consentType);

    // === Экспорт персональных данных (ст. 14) ===

    /// <summary>Экспорт всех ПД пользователя</summary>
    Task<PersonalDataExportDto> ExportPersonalDataAsync(int userId);

    // === Запросы субъекта ПД ===

    /// <summary>Создать запрос субъекта ПД</summary>
    Task<int> CreateRequestAsync(int userId, CreatePersonalDataRequestDto dto);

    /// <summary>Получить запросы пользователя</summary>
    Task<List<PersonalDataRequestDto>> GetUserRequestsAsync(int userId);

    /// <summary>Получить все запросы (для модератора)</summary>
    Task<List<PersonalDataRequestDto>> GetAllRequestsAsync();

    /// <summary>Получить запрос по ID</summary>
    Task<PersonalDataRequestDto?> GetRequestByIdAsync(int requestId);

    /// <summary>Обработать запрос (модератор)</summary>
    Task ProcessRequestAsync(int requestId, int moderatorId, ProcessPersonalDataRequestDto dto);

    // === Удаление / анонимизация ПД (ст. 21) ===

    /// <summary>Анонимизировать данные пользователя</summary>
    Task AnonymizeUserDataAsync(int userId, int moderatorId);
}
