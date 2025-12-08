using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ResxAutoTranslator;

public class Program {
	private static readonly string AssemblyDir = @"D:\pragma\MyProject\ReportView\ReportView";
	private static readonly string ZhResxPath = Path.Combine(AssemblyDir, @"Resources\Lang.zh-CN.resx");
	private static readonly string EnResxPath = Path.Combine(AssemblyDir, @"Resources\Lang.resx");

	public static async Task Main() {
		Console.WriteLine("🌍 多语言资源提取 + 自动翻译工具");
		Console.WriteLine("输入页面名（不带扩展名），自动在程序集内查找对应 .xaml 文件");
		Console.WriteLine("输入 exit 退出。\n");

		while (true) {
			Console.Write("请输入页面名: ");
			var viewName = Console.ReadLine()?.Trim();

			if (string.IsNullOrWhiteSpace(viewName) ||
			    viewName.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
				Console.WriteLine("👋 已退出。");
				break;
			}

			await ProcessFileAsync(viewName);
			Console.WriteLine();
		}
	}

	static async Task ProcessFileAsync(string viewName) {
		// 🔍 递归搜索对应 XAML 文件
		string? xamlPath = Directory
			.EnumerateFiles(AssemblyDir, viewName + ".xaml", SearchOption.AllDirectories)
			.FirstOrDefault();

		if (xamlPath == null) {
			Console.WriteLine($"❌ 未找到页面: {viewName}.xaml（在 {AssemblyDir} 内）");
			return;
		}

		Console.WriteLine($"🔍 找到文件: {xamlPath}");
		string xaml = File.ReadAllText(xamlPath);

		var regex = new Regex(
			@"<(?<tag>\w+)[^>]*(?<attr>Content|Text|Header|ToolTip|Title)=""(?<value>[^""]*[一-龥]+[^""]*)""",
			RegexOptions.Compiled);

		var zhResx = LoadOrCreateResx(ZhResxPath);
		var enResx = LoadOrCreateResx(EnResxPath);

		int index = 1;
		string newXaml = xaml;
		int addedCount = 0;

		foreach (Match m in regex.Matches(xaml)) {
			var tag = m.Groups["tag"].Value;
			var attr = m.Groups["attr"].Value;
			var value = m.Groups["value"].Value;

			string key = $"{viewName}.{tag}.{attr}.{index}";
			index++;

			// 已存在则跳过
			if (zhResx.Elements("data").Any(e => e.Attribute("name")?.Value == key))
				continue;

			string oldAttr = $"{attr}=\"{value}\"";
			string newAttr = $"{attr}=\"{{lex:Loc Key={key}}}\"";
			newXaml = newXaml.Replace(oldAttr, newAttr);

			// 中文资源
			zhResx.Add(new XElement("data",
				new XAttribute("name", key),
				new XAttribute(XNamespace.Xml + "space", "preserve"),
				new XElement("value", value)));

			// 英文翻译
			string translated = await TranslateToEnglish(value);
			if (string.IsNullOrWhiteSpace(translated))
				translated = value;

			enResx.Add(new XElement("data",
				new XAttribute("name", key),
				new XAttribute(XNamespace.Xml + "space", "preserve"),
				new XElement("value", translated)));

			Console.WriteLine($"🌐 {value} → {translated}");
			addedCount++;

			await Task.Delay(300);
		}

		SaveResx(ZhResxPath, zhResx);
		SaveResx(EnResxPath, enResx);
		File.WriteAllText(xamlPath, newXaml);

		Console.WriteLine($"\n✅ 文件处理完成: {viewName}");
		Console.WriteLine($"📁 中文资源: {ZhResxPath}");
		Console.WriteLine($"📁 英文资源: {EnResxPath}");
		Console.WriteLine($"📝 修改后的 XAML: {xamlPath}");
		Console.WriteLine($"✨ 新增资源数量: {addedCount}");
	}

	static XElement LoadOrCreateResx(string path) {
		if (File.Exists(path)) {
			try {
				return XElement.Load(path);
			} catch {
				Console.WriteLine($"⚠️ 无法读取 {path}，重新创建。");
			}
		}

		var root = new XElement("root");
		root.Add(new XComment("Auto-generated localization resource"));
		return root;
	}

	static void SaveResx(string path, XElement root) {
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		root.Save(path);
	}

	static async Task<string> TranslateToEnglish(string text) {
		try {
			using var http = new HttpClient();
			string url =
				$"https://translate.googleapis.com/translate_a/single?client=gtx&sl=zh-CN&tl=en&dt=t&q={Uri.EscapeDataString(text)}";
			string json = await http.GetStringAsync(url);

			var match = Regex.Match(json, "\"(.*?)\"");
			if (match.Success)
				return match.Groups[1].Value;
		} catch (Exception ex) {
			Console.WriteLine($"⚠️ 翻译失败: {ex.Message}");
		}

		return "";
	}
}