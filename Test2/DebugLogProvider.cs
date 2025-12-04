using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;
using MFUtility.Logging.Interfaces;
using MFUtility.Logging.Tools;

namespace Test2;

public class DebugLogProvider : ILogProvider {

	public void Log(LogLevel level, string message, Exception? ex, CallerData info) {
		string text = LogFormatter.Format(level, message, ex, info);
		System.Diagnostics.Debug.Write(text);
	}
}