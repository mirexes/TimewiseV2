namespace ServiceDesk.Core.Enums;

/// <summary>
/// Типы согласий на обработку персональных данных (ФЗ-152)
/// </summary>
public enum ConsentType
{
    /// <summary>Обработка персональных данных</summary>
    PersonalDataProcessing = 0,

    /// <summary>Получение SMS-уведомлений</summary>
    SmsNotifications = 1,

    /// <summary>Получение Push-уведомлений</summary>
    PushNotifications = 2,

    /// <summary>Передача данных третьим лицам</summary>
    ThirdPartyTransfer = 3
}
