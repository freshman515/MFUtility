using System.Text;
using System.Reflection;

namespace MFUtility.Common.Helpers;

public static class IniHelper
{
    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    // --------------------------
    // Public API
    // --------------------------

    public static void Save<T>(string filePath, T data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        string tmp = filePath + ".tmp";

        try
        {
            string ini = SerializeToIni(data);

            lock (FileLock)
            {
                using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fs, Utf8NoBom))
                    writer.Write(ini);

                if (!File.Exists(filePath))
                    using (File.Create(filePath)) { }

                File.Replace(tmp, filePath, filePath + ".bak", true);
            }
        }
        finally
        {
            TryDelete(tmp);
        }
    }

    public static T Load<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return Activator.CreateInstance<T>();

        try
        {
            var info = new FileInfo(filePath);
            if (info.Length == 0)
            {
                var obj = Activator.CreateInstance<T>();
                Save(filePath, obj);
                return obj;
            }

            lock (FileLock)
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, Encoding.UTF8);
                string ini = reader.ReadToEnd();
                return DeserializeFromIni<T>(ini);
            }
        }
        catch
        {
            CreateBackup(filePath);
            return Activator.CreateInstance<T>();
        }
    }

    // --------------------------
    //   Serialization
    // --------------------------

    private static string SerializeToIni<T>(T obj)
    {
        var sb = new StringBuilder();

        // 主 Section 写简单属性
        WriteSimpleSection(sb, typeof(T).Name, obj);

        // 嵌套对象写子 Section
        WriteNestedSections(sb, typeof(T).Name, obj);

        return sb.ToString();
    }

    private static void WriteSimpleSection(StringBuilder sb, string sectionName, object? obj)
    {
        if (obj == null) return;

        sb.AppendLine($"[{sectionName}]");

        foreach (var prop in obj.GetType().GetProperties())
        {
            if (!prop.CanRead) continue;

            if (IsSimpleType(prop.PropertyType))
            {
                var val = prop.GetValue(obj);
                sb.AppendLine($"{prop.Name}={val}");
            }
        }

        sb.AppendLine();
    }

    private static void WriteNestedSections(StringBuilder sb, string parentSection, object? obj)
    {
        if (obj == null) return;

        foreach (var prop in obj.GetType().GetProperties())
        {
            if (!prop.CanRead) continue;

            var val = prop.GetValue(obj);

            if (val != null && !IsSimpleType(prop.PropertyType))
            {
                // 子 section 名: AppConfig.Student
                string childSection = parentSection + "." + prop.Name;

                WriteSimpleSection(sb, childSection, val);
                WriteNestedSections(sb, childSection, val);
            }
        }
    }

    private static bool IsSimpleType(Type t)
    {
        return t.IsPrimitive
            || t.IsEnum
            || t == typeof(string)
            || t == typeof(decimal);
    }

    // --------------------------
    //   Deserialization
    // --------------------------

    private static T DeserializeFromIni<T>(string ini)
    {
        var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = null;

        foreach (var line in ini.Split('\n'))
        {
            string raw = line.Trim();
            if (raw.Length == 0 || raw.StartsWith(";") || raw.StartsWith("#"))
                continue;

            if (raw.StartsWith("[") && raw.EndsWith("]"))
            {
                currentSection = raw.Substring(1, raw.Length - 2);

                if (!sections.ContainsKey(currentSection))
                    sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                continue;
            }

            if (currentSection == null || !raw.Contains("="))
                continue;

            int idx = raw.IndexOf('=');
            string key = raw.Substring(0, idx).Trim();
            string value = raw.Substring(idx + 1).Trim();

            sections[currentSection][key] = value;
        }

        return (T)CreateObject(typeof(T), typeof(T).Name, sections);
    }

    private static object CreateObject(Type type, string sectionName, Dictionary<string, Dictionary<string, string>> sections)
    {
        var obj = Activator.CreateInstance(type);

        if (sections.TryGetValue(sectionName, out var simpleProps))
        {
            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanWrite) continue;

                if (IsSimpleType(prop.PropertyType))
                {
                    if (simpleProps.TryGetValue(prop.Name, out var value))
                    {
                        try
                        {
                            var converted = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(obj, converted);
                        }
                        catch { }
                    }
                }
                else
                {
                    string childSection = sectionName + "." + prop.Name;
                    var child = CreateObject(prop.PropertyType, childSection, sections);
                    prop.SetValue(obj, child);
                }
            }
        }

        return obj;
    }

    // --------------------------
    // Helpers
    // --------------------------

    private static void CreateBackup(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string backup = $"{filePath}.corrupt-{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                File.Copy(filePath, backup, true);
            }
        }
        catch { }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch { }
    }
}
