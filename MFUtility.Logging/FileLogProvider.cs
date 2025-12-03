using System.Text;
using System.Threading.Channels;

namespace MFUtility.Logging;

public class FileLogProvider : ILogProvider {
	public static FileLogProvider Instance { get; } = new();

	private string _logDirectory =
		Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

	private readonly object _lock = new();

	// 日志条目，用于异步队列
	private readonly Channel<LogItem> _asyncChannel =
		Channel.CreateUnbounded<LogItem>();

	private bool _isAsyncWorkerStarted = false;

	private readonly struct LogItem {
		public LogItem(string file, string text) {
			File = file;
			Text = text;
		}

		public string File { get; }
		public string Text { get; }
	}

	/// <summary>
	/// 设置日志根目录
	/// </summary>
	public void SetDirectory(string path) {
		Directory.CreateDirectory(path);
		_logDirectory = path;
	}

	public void Log(LogLevel level, string message, Exception? ex, CallerInfo caller) {
		// 1. 日志级别过滤
		if (level < LogManager.Options.MinimumLevel)
			return;

		// 2. 构建目录和文件
		string directory = BuildDirectory();
		Directory.CreateDirectory(directory);

		string file = BuildLogFile(directory);

		// 3. 格式化文本
		string text = Format(level, message, ex, caller);

		// 4. 写文件：同步/异步二选一
		if (LogManager.Options.Async) {
			EnsureAsyncWorker();
			_asyncChannel.Writer.TryWrite(new LogItem(file, text));
		} else {
			WriteToFile(file, text);
		}

		// 5. 控制台输出（可选）
		if (LogManager.Options.OutputToConsole) {
			Console.Write(text);
		}
	}

	// ================== 异步后台线程 ==================

	private void EnsureAsyncWorker() {
		if (_isAsyncWorkerStarted) return;

		_isAsyncWorkerStarted = true;

		Task.Run(async () => {
			await foreach (var item in _asyncChannel.Reader.ReadAllAsync()) {
				WriteToFile(item.File, item.Text);
			}
		});
	}

	private void WriteToFile(string file, string text) {
		lock (_lock) {
			CheckFileSize(file);
			File.AppendAllText(file, text, Encoding.UTF8);
		}
	}

	// ================== 路径 & 文件 ==================

	private string BuildDirectory() {
		if (!LogManager.Options.UseDateFolder)
			return _logDirectory;

		// logs/2025/12/
		return Path.Combine(
			_logDirectory,
			DateTime.Now.Year.ToString(),
			DateTime.Now.Month.ToString("00"));
	}

	private string BuildLogFile(string directory) {
		if (LogManager.Options.SplitDaily) {
			// logs/2025-12-03.log
			return Path.Combine(directory, $"{DateTime.Now:yyyy-MM-dd}.log");
		} else {
			// logs/log.log（不按天切文件）
			return Path.Combine(directory, "log.log");
		}
	}

	private void CheckFileSize(string file) {
		long limit = LogManager.Options.MaxFileSizeMB;

		if (limit <= 0) return;

		if (File.Exists(file)) {
			long fileSizeMB = new FileInfo(file).Length / (1024 * 1024);

			if (fileSizeMB >= limit) {
				string newName = $"{file}.{DateTime.Now:HHmmss}.bak";
				File.Move(file, newName, overwrite: true);
			}
		}
	}

	// ================== 调用方信息 & 格式 ==================

	private (string assembly, string className) GetCallerType() {
		var trace = new System.Diagnostics.StackTrace(true);

		// 寻找第一个不属于 LogManager 或 FileLogProvider 的帧
		for (int i = 0; i < trace.FrameCount; i++) {
			var method = trace.GetFrame(i)?.GetMethod();
			var declaring = method?.DeclaringType;

			if (declaring == null)
				continue;

			if (declaring == typeof(LogManager) || declaring == typeof(FileLogProvider))
				continue;

			// 找到业务代码所在类
			return (
				declaring.Assembly.GetName().Name!,
				declaring.Name!
			);
		}

		return ("Unknown", "Unknown");
	}

	private string Format(LogLevel level, string message, Exception? ex, CallerInfo caller) {
		var (asm, cls) = GetCallerType();
		string ts = DateTime.Now.ToString(LogManager.Options.TimeFormat);

		bool tag = LogManager.Options.ShowFieldTag;
		bool brackets = LogManager.Options.UseFieldBrackets;

		var parts = new List<string>();

		foreach (var field in LogManager.Options.FieldOrder) {
			switch (field) {
				case LogField.Time:
					parts.Add($"[{ts}]");
					break;

				case LogField.Level:
					parts.Add($"[{level}]");
					break;

				case LogField.Assembly:
					if (LogManager.Options.IncludeAssembly)
						parts.Add(FormatField("ASM", asm, tag, brackets));
					break;

				case LogField.ClassName:
					if (LogManager.Options.IncludeClassName)
						parts.Add(FormatField("CLASS", cls, tag, brackets));
					break;

				case LogField.LineNumber:
					if (LogManager.Options.IncludeLineNumber)
						parts.Add(FormatField("LINE", caller.Line.ToString(), tag, brackets));
					break;

				case LogField.Message:
					if (LogManager.Options.MessageUseBrackets)
						parts.Add(FormatField("MSG", message, false, true));
					else
						parts.Add(message);
					break;
			}
		}

		// 拼装
		string line = string.Join(" ", parts) + Environment.NewLine;

		if (ex != null) {
			if (LogManager.Options.ExceptionNewLine) {
				// 默认：异常换行
				line += ex + Environment.NewLine;
			} else {
				// 不换行模式：压缩异常为一行
				string exOneLine = ex.ToString()
					.Replace("\r", " ")
					.Replace("\n", " ");

				line = line.TrimEnd('\n', '\r');
				line += " " + exOneLine + Environment.NewLine;
			}
		}

		return line;
	}
	private string FormatField(string tagName, string value, bool tag, bool brackets) {
		string content = tag ? $"{tagName}:{value}" : value;

		// 用户关闭括号 → 完全不加括号
		if (!brackets)
			return content;

		string left = LogManager.Options.LeftBracket;
		string right = LogManager.Options.RightBracket;

		// 用户启用括号，但没有提供括号 → 不使用默认括号，直接返回内容
		if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(right))
			return content;

		return $"{left}{content}{right}";
	}
	private static void AppendMeta(
		StringBuilder sb,
		bool include,
		string tagName,
		string? value,
		bool tag,
		bool brackets) {
		if (!include || string.IsNullOrEmpty(value)) return;

		string content = tag ? $"{tagName}:{value}" : value;

		if (brackets)
			sb.Append($" [{content}]");
		else
			sb.Append($" {content}");
	}
}