using Microsoft.AspNetCore.Mvc;
using ServiceDesk.Core.Interfaces.Services;
using ServiceDesk.Web.Extensions;

namespace ServiceDesk.Web.Controllers.Api;

/// <summary>
/// API для AJAX-операций с клиентами
/// </summary>
[ApiController]
[Route("api/clients")]
public class ClientsApiController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsApiController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>Получить точки обслуживания по клиенту</summary>
    [HttpGet("{clientId}/service-points")]
    public async Task<IActionResult> GetServicePointsByClient(int clientId)
    {
        var points = await _clientService.GetServicePointsForSelectAsync(
            User.GetUserId(), User.GetRole(), clientId);
        return Ok(points);
    }
}
