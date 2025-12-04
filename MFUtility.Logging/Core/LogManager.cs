using System.Runtime.CompilerServices;
using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;
using MFUtility.Logging.Interfaces;
using MFUtility.Logging.Providers;
using MFUtility.Logging.Tools;

namespace MFUtility.Logging;

public class LogManager {
	private static readonly List<ILogProvider> _providers = new();
	private static bool _initialized = false;
	public static LogConfiguration Config { get; } = new();
	public static LogBuilder Configure() {
		return new LogBuilder();
	}
	//=====================================
	// 初始化
	//=====================================
	public static void Initialize(Action<LogConfiguration>? configure = null) {
		if (_initialized)
			return;
		configure?.Invoke(Config);

#if DEBUG
		Config.Output.ToConsole = true;
#endif




		// if (Config.Json.InheritFromFile) {
		// 	Config.Json.UseDateFolder = Config.Output.UseDateFolder;
		// 	Config.Json.SplitDaily = Config.Output.SplitDaily;
		// 	Config.Json.RootDirectory ??= Path.Combine(Config.Output.RootDirectory!, "json");
		// }

		var fileProvider = FileLogProvider.Instance;

		AddProvider(fileProvider);

		_initialized = true;
	}
	public static void AddProvider(ILogProvider provider) {
		if (!_providers.Contains(provider))
			_providers.Add(provider);
	}

	public static void ClearProviders() => _providers.Clear();


	public static void Log(
		LogLevel level,
		string message,
		Exception? ex,
		string? callerFile,
		string? callerMember,
		int callerLine) {
		if (!_initialized)
			Initialize(); // 自动加载配置 + provider
		var caller = new CallerInfo {
			File = callerFile,
			Member = callerMember,
			Line = callerLine
		};
		var info = LogFormatter.BuildCallerData(caller);
		
		foreach (var provider in _providers)
			provider.Log(level, message, ex, info);
	}

	// Convenience methods
	public static void Info(
		string msg,
		Exception? ex = null,
		[CallerFilePath] string? file = null,
		[CallerMemberName] string? member = null,
		[CallerLineNumber] int line = 0) {
		Log(LogLevel.Info, msg, ex, file, member, line);
	}

	public static void Debug(
		string msg,
		Exception? ex = null,
		[CallerFilePath] string? file = null,
		[CallerMemberName] string? member = null,
		[CallerLineNumber] int line = 0) {
		Log(LogLevel.Debug, msg, ex, file, member, line);
	}

	public static void Warning(
		string msg,
		Exception? ex = null,
		[CallerFilePath] string? file = null,
		[CallerMemberName] string? member = null,
		[CallerLineNumber] int line = 0) {
		Log(LogLevel.Warning, msg, ex, file, member, line);
	}

	public static void Error(
		string msg,
		Exception? ex = null,
		[CallerFilePath] string? file = null,
		[CallerMemberName] string? member = null,
		[CallerLineNumber] int line = 0) {
		Log(LogLevel.Error, msg, ex, file, member, line);
	}

	public static void Fatal(
		string msg,
		Exception? ex = null,
		[CallerFilePath] string? file = null,
		[CallerMemberName] string? member = null,
		[CallerLineNumber] int line = 0) {
		Log(LogLevel.Fatal, msg, ex, file, member, line);
	}


}