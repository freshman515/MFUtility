using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Interfaces;

public interface ILogProvider
{
    void Log(LogLevel level, string message, Exception? ex, LogInfo info);
}