using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace ServiceDesk.Web.Auth;

/// <summary>
/// Подключает DatabaseTicketStore к CookieAuthenticationOptions без BuildServiceProvider
/// </summary>
public class ConfigureCookieTicketStore : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _ticketStore;

    public ConfigureCookieTicketStore(ITicketStore ticketStore)
    {
        _ticketStore = ticketStore;
    }

    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        options.SessionStore = _ticketStore;
    }
}
