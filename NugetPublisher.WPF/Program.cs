using System.Diagnostics;
using System.Xml.Linq;

namespace NugetPublisher.WPF;

public class Program {
	public static int Main(string[] args) {
		try {
			Console.WriteLine("🚀 NuGet 自动发布工具");

			string projectPath = GetArgValue(args, "-p")
			                     ?? @"D:\pragma\Utilities\XMFUtility\MFUtility.WPF\MFUtility.WPF.csproj";

			if (!File.Exists(projectPath))
				throw new FileNotFoundException($"未找到项目文件: {projectPath}");

			// ✅ 自动递增版本号（如果未手动指定）
			string version = GetArgValue(args, "-v");
			if (version == null) {
				version = AutoIncrementVersion(projectPath);
				Console.WriteLine($"🔢 自动生成新版本号: {version}");
			}

			string source = GetArgValue(args, "-s") ?? "https://api.nuget.org/v3/index.json";
			string apiKey = GetArgValue(args, "-k") ?? GetApiKeyFromEnv();

			if (string.IsNullOrEmpty(apiKey))
				throw new("⚠️ 未设置 NUGET_API_KEY 环境变量");

			Console.WriteLine($"📦 项目: {projectPath}");
			Console.WriteLine($"🔖 版本: {version}");
			Console.WriteLine($"🌐 源: {source}");

			UpdateProjectVersion(projectPath, version);
			RunCommand("dotnet", $"pack \"{projectPath}\" -c Release -o ./nupkg");

			string nupkgFile = Directory.GetFiles("./nupkg", $"*.{version}.nupkg").FirstOrDefault()
			                   ?? throw new FileNotFoundException($"未找到打包文件 (*.nupkg)");

			Console.WriteLine($"✅ 打包成功: {nupkgFile}");

			RunCommand("dotnet",
				$"nuget push \"{nupkgFile}\" --source \"{source}\" --api-key {apiKey} --skip-duplicate");

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("🎉 发布成功！");
			Console.ResetColor();
			return 0;
		} catch (Exception ex) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"❌ 发布失败: {ex.Message}");
			Console.ResetColor();
			return -1;
		}
	}

	private static string? GetArgValue(string[] args, string key) {
		int i = Array.IndexOf(args, key);
		return (i >= 0 && i + 1 < args.Length) ? args[i + 1] : null;
	}

	private static string GetApiKeyFromEnv() =>
		Environment.GetEnvironmentVariable("NUGET_API_KEY") ?? "";

	private static string GetVersionFromProject(string csprojPath) {
		var xml = XDocument.Load(csprojPath);
		var ver = xml.Descendants("Version").FirstOrDefault()?.Value;
		return string.IsNullOrWhiteSpace(ver) ? "1.0.0" : ver;
	}

	/// <summary>
	/// 自动递增补丁版本号（1.0.5 → 1.0.6）
	/// </summary>
	private static string AutoIncrementVersion(string csprojPath) {
		string currentVersion = GetVersionFromProject(csprojPath);

		var parts = currentVersion.Split('.');
		if (parts.Length < 3)
			parts = new[] { "1", "0", "0" }; // 默认起始

		if (int.TryParse(parts[^1], out int patch))
			parts[^1] = (patch + 1).ToString();
		else
			parts[^1] = "1";

		return string.Join(".", parts);
	}

	private static void UpdateProjectVersion(string csprojPath, string newVersion) {
		var xml = XDocument.Load(csprojPath);
		var versionNode = xml.Descendants("Version").FirstOrDefault();

		if (versionNode == null) {
			var group = xml.Descendants("PropertyGroup").FirstOrDefault();
			group?.Add(new XElement("Version", newVersion));
		} else versionNode.Value = newVersion;

		xml.Save(csprojPath);
		Console.WriteLine($"📝 更新版本号至 {newVersion}");
	}

	private static void RunCommand(string file, string args) {
		Console.WriteLine($"▶️ {file} {args}");

		var psi = new ProcessStartInfo {
			FileName = file,
			Arguments = args,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		var proc = Process.Start(psi)!;
		proc.OutputDataReceived += (_, e) => {
			if (e.Data != null) Console.WriteLine(e.Data);
		};
		proc.ErrorDataReceived += (_, e) => {
			if (e.Data != null) Console.WriteLine(e.Data);
		};
		proc.BeginOutputReadLine();
		proc.BeginErrorReadLine();
		proc.WaitForExit();

		if (proc.ExitCode != 0)
			throw new Exception($"命令执行失败 ({file} {args})");
	}
}