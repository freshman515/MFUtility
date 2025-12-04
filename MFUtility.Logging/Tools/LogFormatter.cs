using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;
using MFUtility.Logging.Providers;
using Newtonsoft.Json.Linq;

namespace MFUtility.Logging.Tools;

public static class LogFormatter {
	public static CallerData BuildCallerData(CallerInfo caller) {
		var (assembly, className) = GetCallerType(caller);

		return new CallerData {
			AssemblyName = assembly,
			ClassName = className,
			LineNumber = caller.Line.ToString(),
			MethodName = caller.Member ?? "Unknown"
		};
	}
	public static string Format(LogLevel level, string message, Exception? ex, CallerData info) {
		string ts = DateTime.Now.ToString(LogManager.Config.Format.TimeFormat);

		bool tag = LogManager.Config.Format.ShowFieldTag;
		bool brackets = LogManager.Config.Format.UseFieldBrackets;

		var parts = new List<string>();

		foreach (var field in LogManager.Config.Format.FieldOrder) {
			switch (field) {
				case LogField.Time:
					parts.Add($"[{ts}]");
					break;

				case LogField.Level:
					parts.Add($"[{level}]");
					break;

				case LogField.Assembly:
					if (LogManager.Config.Format.IncludeAssembly)
						parts.Add(FormatField("Assembly", info.AssemblyName, tag, brackets));
					break;

				case LogField.ClassName:
					if (LogManager.Config.Format.IncludeClassName)
						parts.Add(FormatField("Class", info.ClassName, tag, brackets));
					break;

				case LogField.LineNumber:
					if (LogManager.Config.Format.IncludeLineNumber)
						parts.Add(FormatField("Line", info.LineNumber, tag, brackets));
					break;
				case LogField.MethodName:
					if (LogManager.Config.Format.IncludeMethodName)
						parts.Add(FormatField("Method", info.MethodName, tag, brackets));
					break;

				case LogField.Message:
					if (LogManager.Config.Format.MessageUseBrackets)
						parts.Add(FormatField("Message", message, false, true));
					else
						parts.Add(message);
					break;
			}
		}

		// 拼装
		string line = string.Join(" ", parts) + Environment.NewLine;

		if (ex != null) {
			if (LogManager.Config.Format.ExceptionNewLine) {
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
	private static string FormatField(string tagName, string value, bool tag, bool brackets) {
		string content = tag ? $"{tagName}:{value}" : value;

		// 用户关闭括号 → 完全不加括号
		if (!brackets)
			return content;

		string left = LogManager.Config.Format.LeftBracket;
		string right = LogManager.Config.Format.RightBracket;

		// 用户启用括号，但没有提供括号 → 不使用默认括号，直接返回内容
		if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(right))
			return content;

		return $"{left}{content}{right}";
	}

	public static JObject BuildJsonObject(LogLevel level, string message, Exception? ex, CallerData info) {

		JObject obj = new JObject();
		var fmt = LogManager.Config.Format;

		foreach (var field in fmt.FieldOrder) {
			switch (field) {
				case LogField.Time:
					obj["timestamp"] = DateTime.Now.ToString(fmt.TimeFormat);
					break;

				case LogField.Level:
					obj["level"] = level.ToString();
					break;

				case LogField.Assembly:
					if (fmt.IncludeAssembly)
						obj["assembly"] = info.AssemblyName;
					break;

				case LogField.ClassName:
					if (fmt.IncludeClassName)
						obj["className"] = info.ClassName;
					break;

				case LogField.LineNumber:
					if (fmt.IncludeLineNumber)
						obj["line"] = info.LineNumber;
					break;
				case LogField.MethodName:
					if (fmt.IncludeMethodName)
						obj["Method"] = info.MethodName;
					break;


				case LogField.Message:
					obj["message"] = message;
					break;
			}
		}

		if (ex != null)
			obj["exception"] = ex.ToString();

		return obj;
	}
	private static (string assembly, string className) GetCallerType(CallerInfo caller) {
		if (caller.File == null)
			return ("Unknown", "Unknown");

		string className = Path.GetFileNameWithoutExtension(caller.File);

		// 从所有已加载的程序集查找对应类名
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
			try {
				var type = asm.GetType(className) ??
				           asm.GetTypes().FirstOrDefault(t => t.Name == className);

				if (type != null) {
					return (asm.GetName().Name!, className);
				}
			} catch {
				// 某些动态程序集无法反射，安全忽略
			}
		}

		// 找不到则返回文件名作为类名
		return ("Unknown", className);
	}

}