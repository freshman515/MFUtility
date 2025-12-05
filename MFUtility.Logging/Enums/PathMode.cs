namespace MFUtility.Logging.Enums;

public enum PathMode
{
    ApplicationBase, // AppDomain.BaseDirectory
    SolutionRoot,    // 解决方案根目录
    Absolute         // 用户指定绝对路径
}