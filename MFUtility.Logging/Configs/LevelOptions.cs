using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Configs;

public class LevelOptions
{
    /// <summary>最低日志输出级别</summary>
    public LogLevel Minimum { get; set; } = LogLevel.Debug;
}