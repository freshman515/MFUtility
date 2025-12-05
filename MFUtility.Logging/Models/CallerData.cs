namespace MFUtility.Logging.Configs;

public class CallerData {
	// ====== 分类（ILogger<T> 支持） ======
	public string? Category { get; set; }

	// ====== Trace / Request / Scope（预留未来扩展） ======
	public string? CorrelationId { get; set; }
	public string? Scope { get; set; } // 若支持 BeginScope() 可以启用
	public string? ClassName { get; set; }
	public string? AssemblyName { get; set; }
	public string? LineNumber { get; set; }
	public string? MethodName { get; set; }
	public string? ThreadId { get; set; }
	public string? ThreadType { get; set; }
	public string? ThreadName { get; set; }
	public string? TaskId { get; set; }


	// ====== Trace / Request / Scope（预留未来扩展） ======

	// ====== 异常信息（结构化） ======
	public string? ExceptionType { get; set; }
	public string? ExceptionMessage { get; set; }
	public string? ExceptionStack { get; set; }

	// ====== 原始消息（用于 JSON 输出） ======
	public string? RawMessage { get; set; }

	// ====== 更新时间或生成时间 ======
	public DateTime Timestamp { get; set; } = DateTime.Now;
}