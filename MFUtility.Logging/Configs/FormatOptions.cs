using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Configs;

public class FormatOptions {
	/// <summary>是否包含程序集名</summary>
	public bool IncludeAssembly { get; set; } = false;

	/// <summary>是否包含类名</summary>
	public bool IncludeClassName { get; set; } = false;

	/// <summary>是否包含行号</summary>
	public bool IncludeLineNumber { get; set; } = false;
	public bool IncludeMethodName { get; set; } = false;
	public bool IncludeThreadId { get; set; } = false;
	public bool IncludeThreadType { get; set; } = false;
	public bool IncludeThreadName { get; set; } = false;
	public bool IncludeTaskId { get; set; } = false;

	/// <summary>时间格式（默认：yyyy-MM-dd HH:mm:ss.fff）</summary>
	public string TimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

	/// <summary>是否显示标签（如 CLASS:, LINE:）</summary>
	public bool ShowFieldTag { get; set; } = true;

	/// <summary>是否使用 [] 包裹字段</summary>
	public bool UseFieldBrackets { get; set; } = true;

	/// <summary>字段顺序</summary>
	public List<LogField> FieldOrder { get; set; } =
		new() {
			LogField.Time,
			LogField.Level,
			LogField.Assembly,
			LogField.ClassName,
			LogField.MethodName,
			LogField.ThreadId,
			LogField.ThreadType,
			LogField.ThreadName,
			LogField.TaskId,
			LogField.LineNumber,
			LogField.Message
		};

	/// <summary>左括号字符（默认 "["）</summary>
	public string LeftBracket { get; set; } = "[";

	/// <summary>右括号字符（默认 "]"）</summary>
	public string RightBracket { get; set; } = "]";

	/// <summary>消息内容是否使用括号包裹</summary>
	public bool MessageUseBrackets { get; set; } = false;

	/// <summary>异常是否换行显示</summary>
	public bool ExceptionNewLine { get; set; } = true;
}