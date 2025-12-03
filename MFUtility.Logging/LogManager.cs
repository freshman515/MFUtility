using System.Runtime.CompilerServices;

namespace MFUtility.Logging;

public class LogManager {
	private static ILogProvider? _provider;
	public static LogOptions Options { get; } = new();
	static LogManager() {
#if DEBUG
		Options.OutputToConsole = true;
#else
		Options.OutputToConsole = false;
#endif
	}
	/// <summary>
	/// 设置外部日志实现，如果不设置则使用默认文件日志
	/// </summary>
	public static void SetProvider(ILogProvider provider) {
		_provider = provider;
	}
	internal static (string? assembly, string? className, int? line) GetCallerInfo() {
		if (!Options.IncludeAssembly && !Options.IncludeClassName && !Options.IncludeLineNumber)
			return (null, null, null);

		// 使用 Caller 信息，比 StackTrace 快
		var frame = new System.Diagnostics.StackTrace(2, true).GetFrame(0);

		string? asm = null;
		string? cls = null;
		int? line = null;

		var method = frame?.GetMethod();

		if (Options.IncludeAssembly)
			asm = method?.Module?.Assembly?.GetName()?.Name;

		if (Options.IncludeClassName)
			cls = method?.DeclaringType?.FullName;

		if (Options.IncludeLineNumber)
			line = frame?.GetFileLineNumber();

		return (asm, cls, line);
	}


	public static void Log(
		LogLevel level,
		string message,
		Exception? ex,
		string? callerFile,
		string? callerMember,
		int callerLine) {
		var caller = new CallerInfo {
			File = callerFile,
			Member = callerMember,
			Line = callerLine
		};

		if (_provider == null)
			FileLogProvider.Instance.Log(level, message, ex, caller);
		else
			_provider.Log(level, message, ex, caller);
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