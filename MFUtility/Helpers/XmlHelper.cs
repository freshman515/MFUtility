using System.IO;
using System.Xml.Serialization;

namespace MFUtility.Helpers;

/// <summary>
/// XML 文件读写辅助类。
/// 支持共享访问、异步读写、自动创建目录及失败重试。
/// </summary>
public static class XmlHelper
{
    public static void Save<T>(string filePath, T data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var serializer = new XmlSerializer(typeof(T));

        FileRetryHelper.Retry(() =>
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            serializer.Serialize(fs, data);
        });
    }

    public static async Task SaveAsync<T>(string filePath, T data)
    {
        await Task.Run(() => Save(filePath, data));
    }

    public static T? Load<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return default;

        var serializer = new XmlSerializer(typeof(T));
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return (T?)serializer.Deserialize(fs);
    }

    public static async Task<T?> LoadAsync<T>(string filePath)
    {
        return await Task.Run(() => Load<T>(filePath));
    }
}
