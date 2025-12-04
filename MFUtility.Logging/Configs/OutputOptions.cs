namespace MFUtility.Logging.Configs;

public class OutputOptions
{
    /// <summary>是否输出到 Console</summary>
    public bool ToConsole { get; set; } = false;

    /// <summary>Console 是否使用彩色输出</summary>
    public bool ConsoleColor { get; set; } = false;

    /// <summary>是否异步写文件</summary>
    public bool Async { get; set; } = false;

    /// <summary>是否按日期创建文件夹 logs/2025/12/</summary>
    public bool UseDateFolder { get; set; } = false;

    /// <summary>文件大小上限（MB），0 表示不限制</summary>
    public long MaxFileSizeMB { get; set; } = 0;

    /// <summary>是否按天分割文件（true=2025-12-05.log / false=log.log）</summary>
    public bool SplitDaily { get; set; } = true;

    /// <summary>日志根目录，默认自动生成 logs/</summary>
    public string? RootPath { get; set; } = string.Empty;
}