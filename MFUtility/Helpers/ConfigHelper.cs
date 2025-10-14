using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace MFUtility.Helpers;

/// <summary>
/// 通用配置文件读写工具，支持 JSON / XML 格式。
/// 自动创建文件夹，支持异步操作。
/// </summary>
public static class ConfigHelper
{
    #region ==== JSON ====

    /// <summary>
    /// 异步保存对象为 JSON 文件（UTF-8）
    /// </summary>
    public static async Task SaveJsonAsync<T>(string filePath, T data, bool indented = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var json = JsonConvert.SerializeObject(data,
            indented ? Formatting.Indented : Formatting.None);

        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(false));
        await writer.WriteAsync(json);
    }

    /// <summary>
    /// 同步保存对象为 JSON 文件
    /// </summary>
    public static void SaveJson<T>(string filePath, T data, bool indented = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var json = JsonConvert.SerializeObject(data,
            indented ? Formatting.Indented : Formatting.None);

        File.WriteAllText(filePath, json, new UTF8Encoding(false));
    }

    /// <summary>
    /// 异步读取 JSON 文件并反序列化
    /// </summary>
    public static async Task<T?> LoadJsonAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        using var reader = new StreamReader(filePath, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// 同步读取 JSON 文件并反序列化
    /// </summary>
    public static T? LoadJson<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        var json = File.ReadAllText(filePath, Encoding.UTF8);
        return JsonConvert.DeserializeObject<T>(json);
    }

    #endregion

    #region ==== XML ====

    /// <summary>
    /// 保存为 XML 文件
    /// </summary>
    public static void SaveXml<T>(string filePath, T data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var serializer = new XmlSerializer(typeof(T));
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        serializer.Serialize(fs, data);
    }

    /// <summary>
    /// 异步保存为 XML 文件
    /// </summary>
    public static async Task SaveXmlAsync<T>(string filePath, T data)
    {
        await Task.Run(() => SaveXml(filePath, data));
    }

    /// <summary>
    /// 从 XML 文件读取
    /// </summary>
    public static T? LoadXml<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        var serializer = new XmlSerializer(typeof(T));
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (T?)serializer.Deserialize(fs);
    }

    /// <summary>
    /// 异步读取 XML 文件
    /// </summary>
    public static async Task<T?> LoadXmlAsync<T>(string filePath)
    {
        return await Task.Run(() => LoadXml<T>(filePath));
    }

    #endregion
}
