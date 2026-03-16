using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceDesk.Core.Interfaces.Services;

namespace ServiceDesk.Infrastructure.ExternalServices;

/// <summary>
/// Реализация отправки SMS через SMS.ru
/// </summary>
public class SmsRuService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsRuService> _logger;

    public SmsRuService(IConfiguration configuration, ILogger<SmsRuService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string phone, string message)
    {
        // В режиме разработки — просто логируем
        _logger.LogInformation("SMS на {Phone}: {Message}", phone, message);

        var apiKey = _configuration["Sms:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SMS API ключ не настроен. SMS не отправлено");
            return true; // В dev-режиме считаем успешным
        }

        // TODO: Реализовать реальную отправку через SMS.ru API
        await Task.CompletedTask;
        return true;
    }
}
