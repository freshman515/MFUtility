namespace MFUtility.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigMapAttribute : Attribute
{
    public string FileName { get; }

    public ConfigMapAttribute(string fileName)
    {
        FileName = fileName;
    }
}
