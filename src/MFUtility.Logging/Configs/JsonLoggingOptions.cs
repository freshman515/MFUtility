using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Configs;

public class JsonLoggingOptions {
	public bool Indented { get; set; } = false;
	public bool IgnoreNullValues { get; set; } = false;
	public bool UseJsonArray { get; set; } = false;

	public bool UseDateFolder { get; set; } = false;
	public bool UseAppFolder { get; set; } = false;

	public bool SplitDaily { get; set; } = true;
	public long MaxFileSizeMB { get; set; } = 0;
	public bool Async { get; set; } = false;

		   // --- 目标路径1: 应用路径 ---
    public bool EnableAppBasePath { get; set; } = false;
    public string? AppBasePath { get; set; }

    // --- 目标路径2: 解决方案路径 ---
    public bool EnableSolutionPath { get; set; } = false;
    public string? SolutionSubFolder { get; set; }

    // --- 目标路径3: 绝对路径 ---
    public bool EnableAbsolutePath { get; set; } = false;
    public string? AbsolutePath { get; set; }
    public bool EnabelExceptionInfo { get; set; } = true;
    
}