using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace MFUtility.Helpers
{
    public static class AppHelper
    {
        // 获取应用程序的名称
        public static string AppName => Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";

        // 获取应用程序的版本
        public static Version AppVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0);

        // 获取应用程序的当前目录（可执行文件的目录）
        public static string AppDirectory => AppDomain.CurrentDomain.BaseDirectory;

        // 获取应用程序配置文件夹路径（如果不存在则创建）
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

        // 获取日志文件夹路径（如果不存在则创建）
        public static string LogDirectory
        {
            get
            {
                var path = Path.Combine(AppDirectory, "Logs");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        // 获取应用程序域的基础目录（通常与 AppDirectory 相同）
        public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

        // 获取应用程序域的应用程序数据目录
        public static string AppDataDirectory => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // 获取当前应用程序域中的所有程序集
        public static Assembly[] GetLoadedAssemblies() => AppDomain.CurrentDomain.GetAssemblies();

        // 获取应用程序域的配置文件路径（App.config 或 Web.config）
        public static string ConfigFilePath => Path.Combine(AppDirectory, "App.config");

        // 获取应用程序域中所有的临时文件夹路径
        public static string TempDirectory => Path.GetTempPath();

        // 重启当前应用程序
        public static void Restart()
        {
            var exe = Process.GetCurrentProcess().MainModule?.FileName;
            if (exe != null)
            {
                // 使用 Process.Start 启动新进程
                Process.Start(exe);
                // 关闭当前应用程序
                Application.Current.Shutdown();
            }
        }

        // 获取命令行参数
        public static string[] GetCommandLineArgs() => Environment.GetCommandLineArgs();

        // 打开指定路径的文件夹（在文件资源管理器中）
        public static void OpenInExplorer(string path)
        {
            if (Directory.Exists(path))
                Process.Start("explorer.exe", path);
            else
                throw new DirectoryNotFoundException($"路径 {path} 不存在.");
        }

        // 获取应用程序日志文件路径
        public static string GetLogFilePath(string logFileName)
        {
            // 将日志文件保存到日志目录
            return Path.Combine(LogDirectory, $"{logFileName}_{DateTime.Now:yyyyMMdd}.log");
        }

        // 写日志到指定文件
        public static void WriteLog(string logFileName, string message)
        {
            var logFilePath = GetLogFilePath(logFileName);
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                // 如果日志写入失败，输出到控制台
                Console.WriteLine($"日志写入失败: {ex.Message}");
            }
        }

        // 打开指定文件
        public static void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            else
                throw new FileNotFoundException($"文件 {filePath} 未找到.");
        }

        // 获取应用程序数据文件夹路径（如用户数据文件夹）
        public static string GetAppDataDirectory(string appFolder = "MyAppData")
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appFolder);
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
            return appDataPath;
        }

        // 创建或获取应用程序缓存目录
        public static string GetCacheDirectory(string cacheFolder = "Cache")
        {
            var cachePath = Path.Combine(AppDirectory, cacheFolder);
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);
            return cachePath;
        }

        // 获取当前应用程序的完整文件路径（包含文件名）
        public static string GetAppFilePath() => Assembly.GetEntryAssembly()?.Location ?? string.Empty;

        // 获取当前应用程序的入口程序集信息
        public static Assembly GetAppAssembly() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        // 获取当前应用程序的程序集 GUID（适用于 COM 互操作等场景）
        public static string GetAppAssemblyGuid()
        {
            var attribute = (GuidAttribute)Attribute.GetCustomAttribute(GetAppAssembly(), typeof(GuidAttribute));
            return attribute?.Value ?? "UnknownGUID";
        }

        // 获取应用程序域的所有线程ID
        public static string[] GetThreadIDs()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                                          .Select(asm => asm.GetModules())
                                          .SelectMany(mod => mod.SelectMany(m => m.GetTypes()))
                                          .Select(t => t.Name)
                                          .ToArray();
        }

        // 获取应用程序域的程序集路径
        public static string[] GetAssembliesFilePaths()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                                          .Select(asm => asm.Location)
                                          .ToArray();
        }

        // 获取应用程序域的临时目录路径
        public static string GetTemporaryDirectory()
        {
            return Path.GetTempPath();
        }
    }
}
