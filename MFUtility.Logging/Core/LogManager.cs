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
	internal static LogConfiguration Config { get; } = new();
	public static LogBuilder Configure() {
		return new LogBuilder();
	}
	//=====================================
	// 初始化
	//=====================================
	internal static void Initialize(Action<LogConfiguration>? configure = null) {
		if (_initialized)
			return;
		configure?.Invoke(Config);


		

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

		var threadId = Environment.CurrentManagedThreadId;
		var caller = new CallerInfo {
			File = callerFile,
			Member = callerMember,
			Line = callerLine
		};
		var info = LogFormatter.BuildCallerData(caller);
		info.ThreadId = threadId.ToString();
		info.ThreadName = GetThreadName();
		info.ThreadType = GetThreadType();
		info.TaskId = GetTaskId();
		foreach (var provider in _providers)
			provider.Log(level, message, ex, info);
	}
	public static string GetTaskId() {
		int? taskId = Task.CurrentId;

		return taskId?.ToString() ?? "None";
	}
	public static string GetThreadType() {
		var t = Thread.CurrentThread;

		// ThreadPool 线程
		if (t.IsThreadPoolThread)
			return "ThreadPool";

		// Worker 线程（手动 new Thread 且 Background = true）
		if (t.IsBackground)
			return "Worker";

		// Foreground 线程（通常是程序主线程）
		return "Foreground";
	}
	public static string GetThreadName() {
		var t = Thread.CurrentThread;

		if (!string.IsNullOrEmpty(t.Name))
			return t.Name;

		// ThreadPool thread
		if (t.IsThreadPoolThread)
			return $"ThreadPool-{Environment.CurrentManagedThreadId}";

		// Worker thread
		if (t.IsBackground)
			return $"Background-{Environment.CurrentManagedThreadId}";

		// Default
		return $"Thread-{Environment.CurrentManagedThreadId}";
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