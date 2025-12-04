namespace MFUtility.Logging.Configs;

public class JsonOptions
{
    public bool Indented { get; set; } = false;
    public bool IgnoreNullValues { get; set; } = false;
    public bool UseJsonArray { get; set; } = false;

    public bool UseDateFolder { get; set; } = false;
    public string? RootPath { get; set; } = string.Empty;

    public bool SplitDaily { get; set; } = true;
    public long MaxFileSizeMB { get; set; } = 0;
    public bool Async { get; set; } = false;

    public bool InheritFromFile { get; set; } = true;
}