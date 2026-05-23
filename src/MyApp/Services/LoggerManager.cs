using NLog;
using ILogger = NLog.ILogger;

namespace MyApp.Services;

public class LoggerManager : ILoggerManager
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private static readonly ILogger _infoLogger = LogManager.GetLogger("InfoLog");

    public void LogDebug(string message) => _logger.Debug(message);
    public void LogError(string message) => _logger.Error(message);
    public void LogError(Exception ex) => _logger.Error(ex);
    public void LogInfo(string message) => _infoLogger.Info(message);
    public void LogWarn(string message) => _logger.Warn(message);
}
