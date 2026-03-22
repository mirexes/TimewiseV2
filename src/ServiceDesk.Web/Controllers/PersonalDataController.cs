using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.DTOs.PersonalData;
using ServiceDesk.Core.Enums;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;
using ServiceDesk.Web.Filters;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Управление персональными данными (ФЗ-152)
/// </summary>
[RoleAuthorize]
public class PersonalDataController : Controller
{
    private readonly IPersonalDataService _personalDataService;

    public PersonalDataController(IPersonalDataService personalDataService)
    {
        _personalDataService = personalDataService;
    }

    /// <summary>Страница управления согласиями и ПД</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        ViewBag.Consents = await _personalDataService.GetUserConsentsAsync(userId);
        ViewBag.Requests = await _personalDataService.GetUserRequestsAsync(userId);
        return View();
    }

    /// <summary>Предоставление согласия</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantConsent(ConsentType consentType)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();
        await _personalDataService.GrantConsentAsync(User.GetUserId(), consentType, ip, ua);

        // После первичного согласия перенаправляем на дашборд
        if (consentType == ConsentType.PersonalDataProcessing)
            return RedirectToAction("Dashboard", "Home");

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Отзыв согласия</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeConsent(ConsentType consentType)
    {
        await _personalDataService.RevokeConsentAsync(User.GetUserId(), consentType);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Экспорт персональных данных (ст. 14 ФЗ-152)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportData()
    {
        var data = await _personalDataService.ExportPersonalDataAsync(User.GetUserId());
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"personal_data_{DateTime.UtcNow:yyyyMMdd}.json");
    }

    /// <summary>Создание запроса субъекта ПД</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRequest(CreatePersonalDataRequestDto dto)
    {
        await _personalDataService.CreateRequestAsync(User.GetUserId(), dto);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Страница первичного согласия (после регистрации)</summary>
    [HttpGet]
    public IActionResult Consent()
    {
        return View();
    }

    /// <summary>Политика конфиденциальности</summary>
    [HttpGet]
    public IActionResult PrivacyPolicy()
    {
        return View();
    }

    // === Модератор: панель управления запросами ПД ===

    /// <summary>Все запросы субъектов ПД (модератор)</summary>
    [HttpGet]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> Requests()
    {
        var requests = await _personalDataService.GetAllRequestsAsync();
        return View(requests);
    }

    /// <summary>Детали запроса (модератор)</summary>
    [HttpGet]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> RequestDetails(int id)
    {
        var request = await _personalDataService.GetRequestByIdAsync(id);
        if (request is null) return NotFound();
        return View(request);
    }

    /// <summary>Обработка запроса (модератор)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(UserRole.Moderator)]
    public async Task<IActionResult> ProcessRequest(int id, ProcessPersonalDataRequestDto dto)
    {
        await _personalDataService.ProcessRequestAsync(id, User.GetUserId(), dto);

        // Если это запрос на удаление и он одобрен — анонимизируем данные
        var request = await _personalDataService.GetRequestByIdAsync(id);
        if (request is not null
            && request.RequestType == PersonalDataRequestType.Deletion
            && dto.Status == PersonalDataRequestStatus.Completed)
        {
            await _personalDataService.AnonymizeUserDataAsync(request.UserId, User.GetUserId());
        }

        return RedirectToAction(nameof(Requests));
    }
}
