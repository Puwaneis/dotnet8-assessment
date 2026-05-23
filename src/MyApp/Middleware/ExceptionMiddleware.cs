using MyApp.Common;
using MyApp.Services;
using System.Net;
using System.Text.Json;

namespace MyApp.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerManager _logger;

    public ExceptionMiddleware(RequestDelegate next, ILoggerManager logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = Constants.STATUS_ERROR,
                message = Constants.EXCEPTION_MESSAGE
            }));
        }
        else if (!context.Response.HasStarted)
        {
            context.Response.Redirect("/Error");
        }
    }
}
