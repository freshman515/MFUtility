namespace MFUtility.Logging.Configs;

public class LogConfiguration {
	public OutputOptions Output { get; set; } = new();
    public FormatOptions Format { get; set; } = new();
    public LevelOptions Level { get; set; } = new();
    public JsonOptions Json { get; set; } = new();
}