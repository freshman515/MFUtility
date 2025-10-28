using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MFUtility.Helpers;

/// <summary>
/// JSON 文件读写辅助类。
/// 支持共享访问、异步读写、自动创建目录及失败重试。
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// 异步保存对象为 JSON 文件（带重试与共享写入）
    /// </summary>
    public static async Task SaveAsync<T>(string filePath, T data, bool indented = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var json = JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None);

        await FileRetryHelper.RetryAsync(() =>
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fs, new UTF8Encoding(false));
            writer.Write(json);
        });
    }

    /// <summary>
    /// 同步保存对象为 JSON 文件
    /// </summary>
    public static void Save<T>(string filePath, T data, bool indented = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var json = JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None);

        FileRetryHelper.Retry(() =>
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fs, new UTF8Encoding(false));
            writer.Write(json);
        });
    }

    /// <summary>
    /// 异步读取 JSON 文件（带共享访问）
    /// </summary>
    public static async Task<T?> LoadAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fs, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// 同步读取 JSON 文件（带共享访问）
    /// </summary>
    public static T? Load<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fs, Encoding.UTF8);
        var json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(json);
    }
}
