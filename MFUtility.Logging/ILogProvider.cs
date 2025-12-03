namespace MFUtility.Logging;

public interface ILogProvider
{
    void Log(LogLevel level, string message, Exception? ex, CallerInfo caller);
}