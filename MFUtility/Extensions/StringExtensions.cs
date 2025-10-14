using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MFUtility.Extensions;

public static class StringExtensions
{
    #region 🧩 判空与安全访问

    public static bool IsNullOrEmpty(this string? value) =>
        string.IsNullOrEmpty(value);

    public static bool IsNullOrWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static string Safe(this string? value, string defaultValue = "") =>
        string.IsNullOrEmpty(value) ? defaultValue : value;

    public static bool EqualsIgnoreCase(this string? a, string? b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsIgnoreCase(this string? source, string? value) =>
        source != null && value != null &&
        source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;

    #endregion

    #region 📝 格式与清理

    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    public static string Truncate(this string value, int maxLength, string suffix = "…")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
        return value.Substring(0, maxLength) + suffix;
    }

    public static string RemoveWhitespace(this string value) =>
        string.IsNullOrEmpty(value)
            ? value
            : new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());

    public static string RemoveInvalidFileChars(this string value, string replacement = "_")
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            value = value.Replace(c.ToString(), replacement);
        return value;
    }

    public static string NormalizePath(this string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        return path.Replace('\\', '/').TrimEnd('/');
    }

    public static string EnsureEndsWith(this string value, string suffix)
    {
        if (string.IsNullOrEmpty(value)) return suffix;
        return value.EndsWith(suffix) ? value : value + suffix;
    }

    public static string EnsureStartsWith(this string value, string prefix)
    {
        if (string.IsNullOrEmpty(value)) return prefix;
        return value.StartsWith(prefix) ? value : prefix + value;
    }

    #endregion

    #region 🔍 匹配与提取

    public static bool IsMatch(this string input, string pattern) =>
        Regex.IsMatch(input ?? "", pattern);

    public static string MatchFirst(this string input, string pattern)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var match = Regex.Match(input, pattern);
        return match.Success ? match.Value : string.Empty;
    }

    public static string[] SplitByWhitespace(this string input) =>
        Regex.Split(input ?? "", @"\s+").Where(s => s.Length > 0).ToArray();

    public static string[] SplitCsv(this string input) =>
        Regex.Split(input ?? "", @"\s*,\s*").Where(s => s.Length > 0).ToArray();

    #endregion

    #region 🔐 加密与散列

    public static string ToMd5(this string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        using (var md5 = MD5.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string ToSha1(this string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        using (var sha1 = SHA1.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = sha1.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string ToBase64(this string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? ""));

    public static string FromBase64(this string base64)
    {
        if (string.IsNullOrEmpty(base64)) return string.Empty;
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch
        {
            return base64; // 不是合法Base64则原样返回
        }
    }

    #endregion

    #region 🧮 转换与检测

    public static bool IsNumeric(this string value) =>
        double.TryParse(value, out _);

    public static int ToInt(this string value, int defaultValue = 0) =>
        int.TryParse(value, out var v) ? v : defaultValue;

    public static double ToDouble(this string value, double defaultValue = 0) =>
        double.TryParse(value, out var v) ? v : defaultValue;

    public static bool ToBool(this string value, bool defaultValue = false)
    {
        if (bool.TryParse(value, out var b)) return b;
        if (int.TryParse(value, out var i)) return i != 0;
        return defaultValue;
    }

    public static DateTime? ToDateTime(this string value, string? format = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        DateTime result;
        if (format != null)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out result))
                return result;
        }

        if (DateTime.TryParse(value, out result))
            return result;

        return null;
    }

    #endregion

    #region 🎲 其它杂项

    public static string Reverse(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var arr = value.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }

    public static string Repeat(this string value, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(value)) return string.Empty;
        var sb = new StringBuilder(value.Length * count);
        for (int i = 0; i < count; i++) sb.Append(value);
        return sb.ToString();
    }

    public static string JoinWith(this string[] items, string separator = ", ")
        => string.Join(separator, items ?? Array.Empty<string>());

    public static string ToSlug(this string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        value = value.ToLowerInvariant();
        value = Regex.Replace(value, @"[^a-z0-9]+", "-");
        return value.Trim('-');
    }

    #endregion
}
