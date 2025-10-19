using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MFUtility;
using MFUtility.UI;

namespace Test1;

public enum UserRole {
	[Display(Name = "admin")] [Description("管理员")][Lang("管理员", "Admin")]    [Program(Zh = "管理员", En = "Admin")]

	Admin = 1,

	[Display(Name = "CommonUser")] [Description("普通用户")] [Lang("普通用户", "User")] [Program(Zh = "普通用户", En = "User")]
	User = 2,

	[Display(Name = "Visitor")] [Description("访客")]     [Lang("访客", "Guest")]  [Program(Zh = "访客", En = "Guest")]

	Guest = 3
}

   [AttributeUsage(AttributeTargets.Field)]
public class ProgramAttribute : Attribute
{
    public string Zh { get; set; }
    public string En { get; set; }
}
 public enum UnitCategory
    {
        [Description("长度单位")]
        Length,
        [Description("压力单位")]
        Pressure
    }

    public enum LengthUnit
    {
        [Description("米")] Meter,
        [Description("厘米")] Centimeter,
        [Description("毫米")] Millimeter
    }

    public enum PressureUnit
    {
        [Description("帕")] Pa,
        [Description("千帕")] KPa,
        [Description("兆帕")] MPa
    }