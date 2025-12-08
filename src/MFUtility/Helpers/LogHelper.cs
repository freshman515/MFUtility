using NLog;

namespace MFUtility.Helpers;
 public static class LogHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void LogInfo(string message) => _logger.Info(message);
        public static void LogDebug(string message) => _logger.Debug(message);
        public static void LogWarn(string message) => _logger.Warn(message);
        public static void LogError(string message) => _logger.Error(message);
        public static void LogError(string message, Exception ex) => _logger.Error(ex, message);
        public static void LogFatal(string message) => _logger.Fatal(message);
        public static void LogFatal(string message, Exception ex) => _logger.Fatal(ex, message);
    }