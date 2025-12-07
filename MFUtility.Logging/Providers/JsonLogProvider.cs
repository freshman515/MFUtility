using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;
using MFUtility.Logging.Interfaces;
using MFUtility.Logging.Tools;
using Newtonsoft.Json;

namespace MFUtility.Logging.Providers;

public class JsonLogProvider : ILogProvider {
	private string _rootDirectory =
		Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
	private readonly object _lock = new();


	public void Log(LogLevel level, string message, Exception? ex, LogInfo info) {
		if (level < LogManager.Config.Level.Minimum)
			return;

		var cfg = LogManager.Config.Json;

		// ---------- 保留你原来的 JSON 生成逻辑 ----------
		var settings = new JsonSerializerSettings {
			NullValueHandling = cfg.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include
		};

		var logObj = LogFormatter.BuildJsonObject(level, message, ex, info);

		string json = JsonConvert.SerializeObject(
			logObj,
			cfg.Indented ? Formatting.Indented : Formatting.None,
			settings
		);

		// =============== 多目标输出（你要的部分） ===============

		if (cfg.EnableAppBasePath) {
			var dir = BuildDirectory(FileTargetType.AppBase, cfg);
			var file = BuildLogFile(dir, cfg);
			WriteJson(file, json, cfg);
		}

		if (cfg.EnableSolutionPath) {
			var dir = BuildDirectory(FileTargetType.Solution, cfg);
			var file = BuildLogFile(dir, cfg);
			WriteJson(file, json, cfg);
		}

		if (cfg.EnableAbsolutePath) {
			var dir = BuildDirectory(FileTargetType.Absolute, cfg);
			var file = BuildLogFile(dir, cfg);
			WriteJson(file, json, cfg);
		}
	}
	private void WriteJson(string file, string json, JsonLoggingOptions cfg) {
		lock (_lock) {
			CheckFileSize(file);

			if (!cfg.UseJsonArray) {
				File.AppendAllText(file, json + Environment.NewLine);
				return;
			}

			WriteAsJsonArray(file, json);
		}
	}


	private string BuildDirectory(FileTargetType type, JsonLoggingOptions opt) {
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

	private string BuildLogFile(string directory, JsonLoggingOptions cfg) {
		string ext = cfg.UseJsonArray ? ".json" : ".jsonl";

		string fileName = cfg.SplitDaily
			? $"{DateTime.Now:yyyy-MM-dd}{ext}"
			: $"log{ext}";

		return Path.Combine(directory, fileName);
	}


	private void CheckFileSize(string file) {
		long limit = LogManager.Config.Json.MaxFileSizeMB;
		if (limit <= 0) return;

		if (!File.Exists(file)) return;

		long fileSizeMB = new FileInfo(file).Length / (1024 * 1024);
		if (fileSizeMB >= limit) {
			string newName = $"{file}.{DateTime.Now:HHmmss}.bak";
			File.Move(file, newName);
		}
	}



	// ================================
	// JSON 数组写入
	// ================================
	private void WriteAsJsonArray(string file, string json) {
		// 缩进 JSON 对象（4 空格）
		string indentedJson = "    " + json.Replace("\n", "\n    ");

		if (!File.Exists(file)) {
			File.WriteAllText(file,
			                  $@"[
{indentedJson}
]");
			return;
		}

		string content = File.ReadAllText(file).Trim();

		if (content.EndsWith("]")) {
			string before = content.Substring(0, content.Length - 1).Trim();

			if (!before.EndsWith("["))
				before += ",";

			File.WriteAllText(file,
			                  $@"{before}
{indentedJson}
]");
		} else {
			// 修复坏文件
			File.WriteAllText(file,
			                  $@"[
{indentedJson}
]");
		}
	}


}