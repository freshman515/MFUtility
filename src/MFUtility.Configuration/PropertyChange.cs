namespace MFUtility.Configuration;

public class PropertyChange {
	public string Name { get; set; } = "";
	public object? OldValue { get; set; }
	public object? NewValue { get; set; }
}