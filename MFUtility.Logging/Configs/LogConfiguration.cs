namespace MFUtility.Logging.Configs;

public class LogConfiguration {
	public FileLoggingOptions File { get; set; } = new();
    public FormatOptions Format { get; set; } = new();
    public LevelOptions Level { get; set; } = new();
    public JsonLoggingOptions Json { get; set; } = new();
}