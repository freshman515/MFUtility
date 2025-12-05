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
		var opt = LogManager.Config.File;

		if (level < LogManager.Config.Level.Minimum)
			return;

		string text = LogFormatter.Format(level, message, ex, info);

		// --- AppBase ---
		if (opt.EnableAppBasePath) {
			string dir = BuildDirectory(FileTargetType.AppBase, opt);
			string file = BuildLogFile(dir, opt);
			WriteText(opt, file, text);
		}

		// --- SolutionRoot ---
		if (opt.EnableSolutionPath) {
			string dir = BuildDirectory(FileTargetType.Solution, opt);
			string file = BuildLogFile(dir, opt);
			WriteText(opt, file, text);
		}

		// --- Absolute ---
		if (opt.EnableAbsolutePath) {
			string dir = BuildDirectory(FileTargetType.Absolute, opt);
			string file = BuildLogFile(dir, opt);
			WriteText(opt, file, text);
		}
	}
	private void WriteText(FileLoggingOptions opt, string file, string text) {
		if (opt.Async) {
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

	private string BuildDirectory(FileTargetType type, FileLoggingOptions opt) {
		string root;

		switch (type) {
			case FileTargetType.AppBase:
				root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
				                    opt.AppBasePath ?? "logs");
				break;

			case FileTargetType.Solution:
				var sln = GetSolutionRoot();
				root = Path.Combine(sln, opt.SolutionSubFolder ?? "logs");
				break;

			case FileTargetType.Absolute:
				if (string.IsNullOrWhiteSpace(opt.AbsolutePath))
					throw new InvalidOperationException("AbsolutePath is required.");
				root = opt.AbsolutePath!;
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(type));
		}

		// AppName
		if (opt.UseAppFolder)
			root = Path.Combine(root, GetAppName());

		// Date
		if (opt.UseDateFolder)
			root = Path.Combine(root, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("00"));

		Directory.CreateDirectory(root);
		return root;
	}
	private static string GetSolutionRoot() {
		var dir = AppDomain.CurrentDomain.BaseDirectory;

		while (dir != null) {
			if (Directory.GetFiles(dir, "*.sln").Any())
				return dir;

			dir = Directory.GetParent(dir)?.FullName;
		}

		return AppDomain.CurrentDomain.BaseDirectory;
	}
	public static string GetAppName() {
		return AppDomain.CurrentDomain.FriendlyName
			.Replace(".exe", "")
			.Replace(".dll", "");
	}
	private string BuildLogFile(string dir, FileLoggingOptions opt) {
		if (opt.SplitDaily)
			return Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd}.log");

		return Path.Combine(dir, "log.log");
	}

	private void CheckFileSize(string file) {
		long limit = LogManager.Config.File.MaxFileSizeMB;

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