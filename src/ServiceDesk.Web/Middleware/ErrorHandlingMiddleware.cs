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
            await HandleException(context, 404, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Недопустимая операция: {Message}", ex.Message);
            await HandleException(context, 400, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка");
            await HandleException(context, 500, "Внутренняя ошибка сервера");
        }
    }

    private static async Task HandleException(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;

        // Для API-запросов возвращаем JSON, для остальных — редирект на страницу ошибки
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new { error = message }));
        }
        else
        {
            context.Response.Redirect($"/Home/Error?code={statusCode}");
        }
    }
}
