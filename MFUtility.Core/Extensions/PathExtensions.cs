using System.IO;

namespace MFUtility.Extensions;

public static class PathExtensions
{
    public static string CombineSafe(this string basePath, params string[] segments)
    {
        foreach (var s in segments)
        {
            basePath = Path.Combine(basePath, s ?? string.Empty);
        }
        return Path.GetFullPath(basePath);
    }

    public static string EnsureDirectory(this string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return path;
    }

    public static string GetFileNameWithoutExtensionSafe(this string path)
        => Path.GetFileNameWithoutExtension(path ?? string.Empty);

    public static string ChangeExtensionTo(this string path, string newExt)
        => Path.ChangeExtension(path, newExt);
}