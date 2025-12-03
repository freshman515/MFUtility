namespace MFUtility.Logging {
	public class LogOptions {
		public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;
		public bool Async { get; set; } = false;
		public bool UseDateFolder { get; set; } = false;
		public long MaxFileSizeMB { get; set; } = 0; // 0 表示不限制
		public bool SplitDaily { get; set; } = true;
		public bool OutputToConsole { get; set; } = false;

		/// <summary>
		/// 是否包含程序集名
		/// </summary>
		public bool IncludeAssembly { get; set; } = false;

		/// <summary>
		/// 是否包含类名
		/// </summary>
		public bool IncludeClassName { get; set; } = false;

		/// <summary>
		/// 是否包含行号
		/// </summary>
		public bool IncludeLineNumber { get; set; } = false;

		/// <summary>
		/// 日志时间格式（默认：yyyy-MM-dd HH:mm:ss.fff）
		/// </summary>
		public string TimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

		/// <summary>
		/// 是否显示标签（如 CLASS:, LINE:）
		/// </summary>
		public bool ShowFieldTag { get; set; } = true;
		/// <summary>是否用 [] 包裹字段（如 [CLASS:MainViewModel]）</summary>
		public bool UseFieldBrackets { get; set; } = true;
		public List<LogField> FieldOrder { get; set; } =
			new() {
				LogField.Time,
				LogField.Level,
				LogField.Assembly,
				LogField.ClassName,
				LogField.LineNumber,
				LogField.Message // 新增
			};
		public string LeftBracket { get; set; } = "[";
		public string RightBracket { get; set; } = "]";
		public bool MessageUseBrackets { get; set; } = false;
		public bool ExceptionNewLine { get; set; } = true;  

	}
}