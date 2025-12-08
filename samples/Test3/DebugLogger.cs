using Caliburn.Micro;

namespace Test3;

public class DebugLogger : ILog
{
    private readonly string _typeName;

    public DebugLogger(Type type)
    {
        _typeName = type.Name;
    }

    public void Info(string format, params object[] args)
    {
       Console.WriteLine($"INFO [{_typeName}] {string.Format(format, args)}");
    }

    public void Warn(string format, params object[] args)
    {
        Console.WriteLine($"WARN [{_typeName}] {string.Format(format, args)}");
    }

    public void Error(Exception exception)
    {
        Console.WriteLine($"ERROR [{_typeName}] {exception}");
    }
}