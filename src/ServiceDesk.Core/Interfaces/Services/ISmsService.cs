namespace ServiceDesk.Core.Interfaces.Services;

/// <summary>
/// Сервис отправки SMS (SMS.ru / SMSC.ru)
/// </summary>
public interface ISmsService
{
    Task<bool> SendAsync(string phone, string message);
}
