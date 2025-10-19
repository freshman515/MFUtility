using System.IO;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace MFUtility.Helpers;

/// <summary>
/// 通用配置文件读写工具，支持 JSON / XML 格式。
/// 自动创建文件夹，支持异步操作，并允许文件共享访问。
/// </summary>
public static class ConfigHelper
{
    #region ==== JSON ====

    /// <summary>
    /// 异步保存对象为 JSON 文件（带共享写入和重试机制）
    /// </summary>
    public static async Task SaveJsonAsync<T>(string filePath, T data, bool indented = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var json = JsonConvert.SerializeObject(data,
            indented ? Formatting.Indented : Formatting.None);

        await RetryAsync(() =>
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fs, new UTF8Encoding(false));
            writer.Write(json);
        });
    }

    /// <summary>
    /// 同步保存对象为 JSON 文件（带共享写入和重试机制）
    /// </summary>
    public static void SaveJson<T>(string filePath, T data, bool indented = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var json = JsonConvert.SerializeObject(data,
            indented ? Formatting.Indented : Formatting.None);

        Retry(() =>
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fs, new UTF8Encoding(false));
            writer.Write(json);
        });
    }

    /// <summary>
    /// 异步读取 JSON 文件（带共享访问）
    /// </summary>
    public static async Task<T?> LoadJsonAsync<T>(string filePath)
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
    public static T? LoadJson<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fs, Encoding.UTF8);
        var json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(json);
    }

    #endregion

    #region ==== XML ====

    public static void SaveXml<T>(string filePath, T data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var serializer = new XmlSerializer(typeof(T));

        Retry(() =>
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            serializer.Serialize(fs, data);
        });
    }

    public static async Task SaveXmlAsync<T>(string filePath, T data)
    {
        await Task.Run(() => SaveXml(filePath, data));
    }

    public static T? LoadXml<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        var serializer = new XmlSerializer(typeof(T));
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return (T?)serializer.Deserialize(fs);
    }

    public static async Task<T?> LoadXmlAsync<T>(string filePath)
    {
        return await Task.Run(() => LoadXml<T>(filePath));
    }

    #endregion

    #region ==== Internal Retry Helper ====

    private static void Retry(Action action, int retryCount = 3, int delayMs = 200)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                action();
                return;
            }
            catch (IOException)
            {
                if (i == retryCount - 1) throw;
                Thread.Sleep(delayMs);
            }
        }
    }

    private static async Task RetryAsync(Action action, int retryCount = 3, int delayMs = 200)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                action();
                return;
            }
            catch (IOException)
            {
                if (i == retryCount - 1) throw;
                await Task.Delay(delayMs);
            }
        }
    }

    #endregion
}
