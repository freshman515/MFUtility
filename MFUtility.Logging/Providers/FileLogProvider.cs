using System.Text;
using System.Threading.Channels;
using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;
using MFUtility.Logging.Interfaces;
using MFUtility.Logging.Tools;

namespace MFUtility.Logging.Providers;

public class FileLogProvider : ILogProvider {
	public static FileLogProvider Instance { get; } = new();

	private string _rootDirectory =
		Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

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

	public void Log(LogLevel level, string message, Exception? ex, CallerData info) {
		// 1. 日志级别过滤
		if (level < LogManager.Config.Level.Minimum)
			return;

		// 2. 构建目录和文件
		string directory = BuildDirectory();
		Directory.CreateDirectory(directory);

		string file = BuildLogFile(directory);

		// 3. 格式化文本
		string text = LogFormatter.Format(level, message, ex, info);

		// 4. 写文件：同步/异步二选一
		if (LogManager.Config.Output.Async) {
			EnsureAsyncWorker();
			_asyncChannel.Writer.TryWrite(new LogItem(file, text));
		} else {
			WriteToFile(file, text);
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
		if (string.IsNullOrWhiteSpace(LogManager.Config.Output.RootPath)) {
			_rootDirectory = Path.Combine(_rootDirectory, "logs");
		} else {
			_rootDirectory = LogManager.Config.Output.RootPath;
		}

		if (!LogManager.Config.Output.UseDateFolder)
			return _rootDirectory;
		
		return Path.Combine(
			_rootDirectory,
			DateTime.Now.Year.ToString(),
			DateTime.Now.Month.ToString("00")
		);
	}

	private string BuildLogFile(string directory) {
		if (LogManager.Config.Output.SplitDaily) {
			// logs/2025-12-03.log
			return Path.Combine(directory, $"{DateTime.Now:yyyy-MM-dd}.log");
		} else {
			// logs/log.log（不按天切文件）
			return Path.Combine(directory, "log.log");
		}
	}

	private void CheckFileSize(string file) {
		long limit = LogManager.Config.Output.MaxFileSizeMB;

		if (limit <= 0) return;

		if (File.Exists(file)) {
			long fileSizeMB = new FileInfo(file).Length / (1024 * 1024);

			if (fileSizeMB >= limit) {
				string newName = $"{file}.{DateTime.Now:HHmmss}.bak";
				File.Move(file, newName);
			}
		}
	}

}