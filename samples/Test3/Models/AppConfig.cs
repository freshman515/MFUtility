using System.Xml.Serialization;
using MFUtility.Configuration.Attributes;

namespace Test3.Models;
[ConfigMap("AppConfig.json")]
public class AppConfig
{
    public string Name { get; set; }
    public Student Student { get; set; }
    public string Version { get; set; }
}

public class Student {
    public string Name { get; set; }
    public int Age { get; set; }
}