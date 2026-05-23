namespace MyApp.Services;

public class NoOpEmailSender(ILoggerManager logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body)
    {
        logger.LogInfo($"[NoOpEmailSender] To: {to} | Subject: {subject} | Body: {body}");
        return Task.CompletedTask;
    }
}
