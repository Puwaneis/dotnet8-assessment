namespace MyApp.Middleware;

public static class ExceptionMiddlewareExtensions
{
    public static void UseCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
    }
}
