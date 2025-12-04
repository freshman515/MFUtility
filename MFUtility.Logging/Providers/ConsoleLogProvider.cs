using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;
using MFUtility.Logging.Interfaces;
using MFUtility.Logging.Tools;

namespace MFUtility.Logging.Providers;

public class ConsoleLogProvider : ILogProvider {
	public void Log(LogLevel level, string message, Exception? ex, CallerData info) {
		string text = LogFormatter.Format(level, message, ex, info);

		if (LogManager.Config.Output.ConsoleColor) {
			Console.ForegroundColor = GetColor(level);
		}
		Console.Write(text);
		Console.ResetColor();
	}

	private ConsoleColor GetColor(LogLevel level) {
		return level switch {
			LogLevel.Debug => ConsoleColor.Cyan,
			LogLevel.Info => ConsoleColor.Green,
			LogLevel.Warning => ConsoleColor.Yellow,
			LogLevel.Error => ConsoleColor.Red,
			LogLevel.Fatal => ConsoleColor.Magenta,
			_ => ConsoleColor.White
		};
	}
}