using System.Threading.Channels;
using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Configs;

public class FileLoggingOptions {
	/// <summary>是否输出到 Console</summary>
	public bool ToConsole { get; set; } = false;

	/// <summary>Console 是否使用彩色输出</summary>
	public bool ConsoleColor { get; set; } = false;

	/// <summary>是否异步写文件</summary>
	public bool Async { get; set; } = false;

	/// <summary>是否按日期创建文件夹 logs/2025/12/</summary>
	public bool UseDateFolder { get; set; } = false;
	public bool UseAppFolder { get; set; } = false;

	/// <summary>文件大小上限（MB），0 表示不限制</summary>
	public long MaxFileSizeMB { get; set; } = 0;

	/// <summary>是否按天分割文件（true=2025-12-05.log / false=log.log）</summary>
	public bool SplitDaily { get; set; } = true;

	   // --- 目标路径1: 应用路径 ---
    public bool EnableAppBasePath { get; set; } = false;
    public string? AppBasePath { get; set; }

    // --- 目标路径2: 解决方案路径 ---
    public bool EnableSolutionPath { get; set; } = false;
    public string? SolutionSubFolder { get; set; }

    // --- 目标路径3: 绝对路径 ---
    public bool EnableAbsolutePath { get; set; } = false;
    public string? AbsolutePath { get; set; }

}