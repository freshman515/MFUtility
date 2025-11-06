using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace MFUtility.Extensions;

/// <summary>
/// 💎 StringExtensions — 超实用字符串扩展集合。
/// 
/// 支持：
/// 🧩 判空与安全访问（IsNullOrEmpty、Safe、EqualsIgnoreCase 等）
/// 📝 格式与清理（ToTitleCase、RemoveWhitespace、NormalizePath）
/// 🔍 匹配与提取（IsEmail、MatchFirst、SplitCsv、IsUrl、IsValidPath）
/// 🔐 加密与散列（MD5、SHA1、Base64、Hash、提取数字/字母等）
/// 🧮 转换与检测（ToInt、ToBool、ToDateTime、IsNumeric）
/// 🎲 其它杂项（Repeat、Reverse、JoinWith、ToSlug）
///
/// 🔧 附加功能：
/// - 文件路径操作（GetFileName、ChangeExtension、GetRelativePath）
/// - 生成唯一字符串（RandomString、RandomGuidString）
/// - 模糊匹配（StartsWithAny、EndsWithAny、EqualsAny）
/// - 控制台输出增强（Dump）
///
/// Designed for .NET Framework 4.7.2+ / .NET 6+
/// </summary>
public static class StringExtensions {
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

	public static string ToTitleCase(this string value) {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
	}

	public static string Truncate(this string value, int maxLength, string suffix = "…") {
		if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
		return value.Substring(0, maxLength) + suffix;
	}

	public static string RemoveWhitespace(this string value) =>
		string.IsNullOrEmpty(value)
			? value
			: new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());

	public static string RemoveInvalidFileChars(this string value, string replacement = "_") {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		foreach (var c in Path.GetInvalidFileNameChars())
			value = value.Replace(c.ToString(), replacement);
		return value;
	}

	public static string NormalizePath(this string path) {
		if (string.IsNullOrEmpty(path)) return path;
		return path.Replace('\\', '/').TrimEnd('/');
	}

	public static string EnsureEndsWith(this string value, string suffix) {
		if (string.IsNullOrEmpty(value)) return suffix;
		return value.EndsWith(suffix) ? value : value + suffix;
	}

	public static string EnsureStartsWith(this string value, string prefix) {
		if (string.IsNullOrEmpty(value)) return prefix;
		return value.StartsWith(prefix) ? value : prefix + value;
	}

    #endregion

    #region 🔍 匹配与提取

	public static bool IsEmail(this string value) =>
		Regex.IsMatch(value ?? "", @"^[\w\.\-]+@([\w\-]+\.)+[a-zA-Z]{2,4}$");

	public static bool IsMatch(this string input, string pattern) =>
		Regex.IsMatch(input ?? "", pattern);

	public static bool IsUrl(this string value) =>
		Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
		(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

	public static bool IsPhoneNumber(this string value) =>
		Regex.IsMatch(value ?? "", @"^(\+?\d{1,4})?\s?\d{5,15}$");

	public static bool IsValidPath(this string value) =>
		!string.IsNullOrEmpty(value) && value.IndexOfAny(Path.GetInvalidPathChars()) == -1;

	public static string MatchFirst(this string input, string pattern) {
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

	public static string ToMd5(this string value) {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		using (var md5 = MD5.Create()) {
			var bytes = Encoding.UTF8.GetBytes(value);
			var hash = md5.ComputeHash(bytes);
			return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
		}
	}

	public static string ToSha1(this string value) {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		using (var sha1 = SHA1.Create()) {
			var bytes = Encoding.UTF8.GetBytes(value);
			var hash = sha1.ComputeHash(bytes);
			return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
		}
	}

	public static string ToBase64(this string value) =>
		Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? ""));

	public static string FromBase64(this string base64) {
		if (string.IsNullOrEmpty(base64)) return string.Empty;
		try {
			return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
		} catch {
			return base64; // 非 Base64 内容则原样返回
		}
	}

	public static int GetStableHashCode(this string value) {
		unchecked {
			int hash = 23;
			foreach (char c in value ?? "")
				hash = hash * 31 + c;
			return hash;
		}
	}

	public static string ExtractDigits(this string value) =>
		Regex.Replace(value ?? "", @"\D", "");

	public static string ExtractLetters(this string value) =>
		Regex.Replace(value ?? "", @"[^a-zA-Z]", "");

	public static string GetFileExtension(this string path) =>
		Path.GetExtension(path)?.TrimStart('.').ToLowerInvariant() ?? "";

	public static string GetFileNameOnly(this string path) =>
		Path.GetFileNameWithoutExtension(path) ?? "";

	public static string ToPascalCase(this string value) {
		if (string.IsNullOrEmpty(value)) return value;
		var words = Regex.Split(value, @"[\W_]+").Where(w => w.Length > 0);
		return string.Concat(words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
	}

	public static string ToSnakeCase(this string value) =>
		Regex.Replace(value ?? "", "(?<!^)([A-Z])", "_$1").ToLowerInvariant();

	public static string ToKebabCase(this string value) =>
		Regex.Replace(value ?? "", "(?<!^)([A-Z])", "-$1").ToLowerInvariant();

	public static string SubstringSafe(this string value, int start, int length) {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		if (start >= value.Length) return string.Empty;
		if (start + length > value.Length) length = value.Length - start;
		return value.Substring(start, length);
	}

	public static string ToFileSize(this long bytes) {
		if (bytes < 1024) return $"{bytes} B";
		double kb = bytes / 1024.0;
		if (kb < 1024) return $"{kb:F2} KB";
		double mb = kb / 1024.0;
		if (mb < 1024) return $"{mb:F2} MB";
		double gb = mb / 1024.0;
		return $"{gb:F2} GB";
	}

	public static string CapitalizeFirst(this string value) {
		if (string.IsNullOrEmpty(value)) return value;
		return char.ToUpper(value[0]) + (value.Length > 1 ? value.Substring(1) : "");
	}

	public static string LowercaseFirst(this string value) {
		if (string.IsNullOrEmpty(value)) return value;
		return char.ToLower(value[0]) + (value.Length > 1 ? value.Substring(1) : "");
	}

	public static string UrlEncode(this string value) =>
		System.Net.WebUtility.UrlEncode(value);

	public static string UrlDecode(this string value) =>
		System.Net.WebUtility.UrlDecode(value);

	public static string ToUnicodeEscape(this string value) =>
		string.Concat(value.Select(c => $"\\u{(int)c:x4}"));

	public static string FromUnicodeEscape(this string value) =>
		Regex.Replace(value, @"\\u([0-9a-fA-F]{4})",
			m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());

	public static string MaskPhone(this string phone) {
		if (string.IsNullOrEmpty(phone)) return phone;
		return Regex.Replace(phone, @"(\d{3})\d{4}(\d{4})", "$1****$2");
	}

	public static string MaskEmail(this string email) {
		if (string.IsNullOrEmpty(email) || !email.Contains("@")) return email;
		var parts = email.Split('@');
		var name = parts[0];
		var hidden = name.Length <= 2 ? name[0] + "*" : name.Substring(0, 2) + new string('*', name.Length - 2);
		return hidden + "@" + parts[1];
	}

	public static bool ContainsAny(this string value, params string[] keywords) =>
		keywords.Any(k => value?.Contains(k) ?? false);

	public static void Dump(this string value, string? title = null) {
		Console.ForegroundColor = ConsoleColor.Cyan;
		if (!string.IsNullOrEmpty(title))
			Console.WriteLine($"[{title}] {value}");
		else
			Console.WriteLine(value);
		Console.ResetColor();
	}

	public static string GetRelativePath(this string fullPath, string basePath) {
		var full = new Uri(fullPath);
		var baseUri = new Uri(basePath.EndsWith("\\") ? basePath : basePath + "\\");
		return Uri.UnescapeDataString(baseUri.MakeRelativeUri(full).ToString());
	}

	public static string RandomString(int length = 8) {
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		var data = new byte[length];
		using (var rng = RandomNumberGenerator.Create())
			rng.GetBytes(data);
		return new string(data.Select(b => chars[b % chars.Length]).ToArray());
	}

	public static string GetDirectory(this string path) =>
		string.IsNullOrWhiteSpace(path) ? string.Empty : Path.GetDirectoryName(path) ?? string.Empty;

	public static string GetFileName(this string path) =>
		string.IsNullOrWhiteSpace(path) ? string.Empty : Path.GetFileName(path) ?? string.Empty;

	public static string ChangeExtension(this string path, string newExt) {
		if (string.IsNullOrEmpty(path)) return path;
		if (!newExt.StartsWith(".")) newExt = "." + newExt;
		return Path.ChangeExtension(path, newExt);
	}

	public static bool StartsWithAny(this string value, params string[] prefixes) =>
		prefixes.Any(p => value?.StartsWith(p, StringComparison.OrdinalIgnoreCase) ?? false);

	public static bool EndsWithAny(this string value, params string[] suffixes) =>
		suffixes.Any(s => value?.EndsWith(s, StringComparison.OrdinalIgnoreCase) ?? false);

	public static string RandomGuidString(bool withoutDash = true) =>
		withoutDash ? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString();

	public static bool EqualsAny(this string value, params string[] options) =>
		options.Any(opt => string.Equals(value, opt, StringComparison.OrdinalIgnoreCase));

    #endregion

    #region 🧮 转换与检测

	public static bool IsNumeric(this string value) =>
		double.TryParse(value, out _);

	public static int ToInt(this string value, int defaultValue = 0) =>
		int.TryParse(value, out var v) ? v : defaultValue;

	public static double ToDouble(this string value, double defaultValue = 0) =>
		double.TryParse(value, out var v) ? v : defaultValue;

	public static bool ToBool(this string value, bool defaultValue = false) {
		if (bool.TryParse(value, out var b)) return b;
		if (int.TryParse(value, out var i)) return i != 0;
		return defaultValue;
	}

	public static DateTime? ToDateTime(this string value, string? format = null) {
		if (string.IsNullOrWhiteSpace(value)) return null;

		DateTime result;
		if (format != null) {
			if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture,
				    DateTimeStyles.None, out result))
				return result;
		}

		if (DateTime.TryParse(value, out result))
			return result;

		return null;
	}
	public static Brush ToBrush(this string hex) {
		if (!hex.StartsWith("#"))
			hex = "#" + hex;

		var color = (Color)ColorConverter.ConvertFromString(hex);

		return new SolidColorBrush(color);
	}
	public static Color ToColor(this string hex) {
		if (!hex.StartsWith("#"))
			hex = "#" + hex;

		var color = (Color)ColorConverter.ConvertFromString(hex);

		return color;
	}

	/// <summary>
	/// 将 Color 转换为十六进制字符串（#RRGGBB 或 #AARRGGBB）
	/// </summary>
	public static string ToHex(this Color color, bool includeAlpha = false) {
		return includeAlpha
			? $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"
			: $"#{color.R:X2}{color.G:X2}{color.B:X2}";
	}
	/// <summary>
	/// 根据 {key} 模板占位符替换内容。
	/// </summary>
	public static string FormatWith(this string template, object values) {
		if (template == null) return string.Empty;
		var dict = values.GetType()
			.GetProperties()
			.ToDictionary(p => p.Name, p => p.GetValue(values)?.ToString() ?? "");
		foreach (var kv in dict)
			template = template.Replace("{" + kv.Key + "}", kv.Value);
		return template;
	}

    #endregion

    #region 🎲 其它杂项

	public static string Reverse(this string value) {
		if (string.IsNullOrEmpty(value)) return value;
		var arr = value.ToCharArray();
		Array.Reverse(arr);
		return new string(arr);
	}

	public static string Repeat(this string value, int count) {
		if (count <= 0 || string.IsNullOrEmpty(value)) return string.Empty;
		var sb = new StringBuilder(value.Length * count);
		for (int i = 0; i < count; i++) sb.Append(value);
		return sb.ToString();
	}

	public static string JoinWith(this string[] items, string separator = ", ") =>
		string.Join(separator, items ?? Array.Empty<string>());

	public static string ToSlug(this string value) {
		if (string.IsNullOrEmpty(value)) return string.Empty;
		value = value.ToLowerInvariant();
		value = Regex.Replace(value, @"[^a-z0-9]+", "-");
		return value.Trim('-');
	}

    #endregion
}