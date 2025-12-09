// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Xml.Linq;

namespace NugetPublisher;

internal abstract class NugetPublisher {
	public static int Main(string[] args) {
		try {
			Console.WriteLine("🚀 NuGet 自动发布工具");

			var projects = new Dictionary<int, string> {
				{ 1, @"D:\pragma\Utilities\XMFUtility\src\MFUtility\MFUtility.csproj" },
				{ 2, @"D:\pragma\Utilities\XMFUtility\src\MFUtility.Common\MFUtility.Common.csproj" },
				{ 3, @"D:\pragma\Utilities\XMFUtility\src\MFUtility.WPF\MFUtility.WPF.csproj" },
				{ 4, @"D:\pragma\Utilities\XMFUtility\src\MFUtility.Logging\MFUtility.Logging.csproj" },
				{ 5, @"D:\pragma\Utilities\XMFUtility\src\MFUtility.Bus\MFUtility.Bus.csproj" },
				{ 6, @"D:\pragma\Utilities\XMFUtility\src\MFUtility.Ioc\MFUtility.Ioc.csproj" },
				{ 7, @"D:\pragma\Utilities\XMFUtility\src\MFUtility.Mvvm.Wpf.Framework\MFUtility.Mvvm.Wpf.Framework.csproj" },
				{ 8, @"D:\pragma\Utilities\XMFUtility\src\MFUtility.Mvvm.Wpf.Toolkit\MFUtility.Mvvm.Wpf.Toolkit.csproj" },
			};

			string? argProject = GetArgValue(args, "-p");
			string projectPath;

			if (argProject != null) {
				projectPath = argProject;
			} else {
				Console.WriteLine("\n请选择要发布的项目：");
				foreach (var kv in projects)
					Console.WriteLine($"{kv.Key}. {kv.Value}");

				Console.Write("\n👉 输入数字选择项目: ");
				if (!int.TryParse(Console.ReadLine(), out int choice) || !projects.ContainsKey(choice))
					throw new Exception("❌ 无效的选择");

				projectPath = projects[choice];
			}

			if (!File.Exists(projectPath))
				throw new FileNotFoundException($"未找到项目文件: {projectPath}");

			// ============================
			// version 自动递增
			// ============================
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

			// ============================
			// dotnet pack
			// ============================
			RunCommand("dotnet", $"pack \"{projectPath}\" -c Release -o ./nupkg");

			// ============================
			// 修复 nupkg 匹配错误的问题 !!!
			// ============================
			string packageId = GetPackageId(projectPath);
			string nupkgPattern = $"{packageId}.{version}.nupkg";

			string? nupkgFile = Directory.GetFiles("./nupkg", nupkgPattern).FirstOrDefault()
			                    ?? throw new FileNotFoundException($"❌ 未找到对应的 NuGet 包文件: {nupkgPattern}\n请检查 ./nupkg 目录中的文件。");

			if (nupkgFile == null) {
				throw new FileNotFoundException(
					$"❌ 未找到对应的 NuGet 包文件: {nupkgPattern}\n" +
					$"请检查 ./nupkg 目录中的文件。"
				);
			}

			Console.WriteLine($"✅ 打包成功: {nupkgFile}");

			// ============================
			// dotnet nuget push
			// ============================
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

	private static string GetPackageId(string csprojPath) {
		var xml = XDocument.Load(csprojPath);

		// 优先 PackageId
		var pkg = xml.Descendants("PackageId").FirstOrDefault()?.Value;
		if (!string.IsNullOrEmpty(pkg))
			return pkg;

		// 次之 AssemblyName
		var asm = xml.Descendants("AssemblyName").FirstOrDefault()?.Value;
		if (!string.IsNullOrEmpty(asm))
			return asm;

		// 最后使用项目名
		return Path.GetFileNameWithoutExtension(csprojPath);
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