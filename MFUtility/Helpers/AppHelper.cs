using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MFUtility.Helpers;

public static class AppHelper
{
    public static string AppName => Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";

    public static Version AppVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0);

    public static string AppDirectory => AppDomain.CurrentDomain.BaseDirectory;

    public static string ConfigDirectory
    {
        get
        {
            var path = Path.Combine(AppDirectory, "Config");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public static void Restart()
    {
        var exe = Process.GetCurrentProcess().MainModule?.FileName;
        if (exe != null)
        {
            Process.Start(exe);
            Application.Current.Shutdown();
        }
    }

    public static string[] GetCommandLineArgs() => Environment.GetCommandLineArgs();

    public static void OpenInExplorer(string path)
    {
        if (Directory.Exists(path))
            Process.Start("explorer.exe", path);
    }
}
