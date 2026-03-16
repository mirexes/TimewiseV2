namespace ServiceDesk.Web.Middleware;

/// <summary>
/// Middleware для обработки необработанных исключений
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ресурс не найден");
            context.Response.StatusCode = 404;
            context.Response.Redirect("/Home/Error?code=404");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Недопустимая операция: {Message}", ex.Message);
            context.Response.StatusCode = 400;
            context.Response.Redirect("/Home/Error?code=400");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка");
            context.Response.StatusCode = 500;
            context.Response.Redirect("/Home/Error");
        }
    }
}
