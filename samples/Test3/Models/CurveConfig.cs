using MFUtility.Configuration.Attributes;

namespace Test3.Models;
[ConfigMap("curves.json")]
public class CurveConfig
{
    public string Name { get; set; }
    public string NameEn { get; set; }
    public string DisplayName { get; set; }
    public bool IsShow { get; set; }
    public string Color { get; set; }
    public string FieldName { get; set; }
    public bool SmoothFilter { get; set; }
    public int XAxisMode { get; set; }
    public int UnitCategory { get; set; }
    public int ShowUnit { get; set; }
    public string DisplayShowUnit { get; set; }
    public int SystemUnit { get; set; }
    public string DisplaySystemUnit { get; set; }
    public double Maximize { get; set; }
    public double Minimize { get; set; }
    public bool YAxisAuto { get; set; }
    public int Tickness { get; set; }
    public bool UseTimeAxis { get; set; }
    public bool UseDepthAxis { get; set; }
}