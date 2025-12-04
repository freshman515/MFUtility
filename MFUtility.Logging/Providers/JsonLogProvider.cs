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


	public void Log(LogLevel level, string message, Exception? ex, CallerData info) {
		if (level < LogManager.Config.Level.Minimum)
			return;
		var dir = BuildDirectory();
		Directory.CreateDirectory(dir);
		var path = BuildLogFile(dir);

		var cfg = LogManager.Config.Json;

		var settings = new JsonSerializerSettings {
			NullValueHandling = cfg.IgnoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include
		};
		var log = LogFormatter.BuildJsonObject(level, message, ex, info);

		string json = JsonConvert.SerializeObject(
			log,
			cfg.Indented ? Formatting.Indented : Formatting.None,
			settings
		);

		lock (_lock) {
			CheckFileSize(path);

			if (!cfg.UseJsonArray) {
				// jsonl 模式
				File.AppendAllText(path, json + Environment.NewLine);
				return;
			}

			WriteAsJsonArray(path, json);
		}
	}


	// ================================
	// 目录与文件构建（仿 FileLogProvider）
	// ================================
	private string BuildDirectory() {
		if (string.IsNullOrWhiteSpace(LogManager.Config.Json.RootPath)) {
			_rootDirectory = Path.Combine(_rootDirectory, "logs");
		} else {
			_rootDirectory = LogManager.Config.Json.RootPath;
		}
		if (!LogManager.Config.Json.UseDateFolder)
			return _rootDirectory;

		return Path.Combine(
			_rootDirectory,
			DateTime.Now.Year.ToString(),
			DateTime.Now.Month.ToString("00")
		);
	}

	private string BuildLogFile(string directory) {
		var cfg = LogManager.Config.Json;
		string extension = cfg.UseJsonArray ? ".json" : ".jsonl";

		// 文件名：按天 or 单一文件
		string fileName = cfg.SplitDaily
			? $"{DateTime.Now:yyyy-MM-dd}{extension}"
			: $"log{extension}";

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