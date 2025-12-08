using System;
using System.IO;

namespace MFUtility.Extensions;

public static class FileExtensions
{
    public static bool ExistsSafe(this string? path)
        => !string.IsNullOrEmpty(path) && File.Exists(path);

    public static long GetFileSize(this string path)
    {
        if (!File.Exists(path)) return 0;
        return new FileInfo(path).Length;
    }

    public static string ToReadableSize(this long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public static string ReadAllSafe(this string path)
    {
        if (!File.Exists(path)) return string.Empty;
        return File.ReadAllText(path);
    }

    public static void WriteAllSafe(this string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, content);
    }
}
