using System.Collections.Immutable;

namespace MFUtility.Logging;

public class LogConfigurator {
	public static void UseDefault() {
		LogManager.Options.TimeFormat = "yyyy-MM-dd HH:mm:ss";
		LogManager.Options.IncludeAssembly = true;
		LogManager.Options.IncludeClassName = true;
		LogManager.Options.IncludeLineNumber = true;
		LogManager.Options.ShowFieldTag = true;
		LogManager.Options.UseFieldBrackets = true;
		LogManager.Options.UseDateFolder = true;
		LogManager.Options.MinimumLevel = LogLevel.Debug;
		LogManager.Options.OutputToConsole = true;
		LogManager.Options.Async = true;
		LogManager.Options.LeftBracket = "(";
		LogManager.Options.RightBracket = ")";
		LogManager.Options.MessageUseBrackets = false;
		LogManager.Options.FieldOrder = new() {
			LogField.Time,
			LogField.Level,
			LogField.Assembly,
			LogField.ClassName,
			LogField.LineNumber,
			LogField.Message
		};
	}
	public static LogOptions Builder(Action<LogOptions> config) {
		config(LogManager.Options);
		return LogManager.Options;
	}

}