using System.Globalization;
using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket.Extensions;

/// <summary>
/// 🌟 扩展：List&lt;MessageParam&gt; 参数访问与转换工具
/// </summary>
public static class MessageParamListExtensions
{
	#region === 基础访问 ===

	/// <summary>
	/// 获取第 n 个参数的值（安全获取，不抛异常）
	/// </summary>
	public static T? Get<T>(this List<MessageParam> list, int index, T? defaultValue = default)
	{
		if (list == null || index < 0 || index >= list.Count)
			return defaultValue;
		return list[index].GetValue<T>();
	}

	/// <summary>第一个参数</summary>
	public static T? First<T>(this List<MessageParam> list, T? defaultValue = default)
		=> list.Get(0, defaultValue);

	/// <summary>第二个参数</summary>
	public static T? Second<T>(this List<MessageParam> list, T? defaultValue = default)
		=> list.Get(1, defaultValue);

	/// <summary>第三个参数</summary>
	public static T? Third<T>(this List<MessageParam> list, T? defaultValue = default)
		=> list.Get(2, defaultValue);

	#endregion

	#region === 常用类型 ===

	public static string? GetString(this List<MessageParam> list, int index, string? defaultValue = null)
		=> list.Get(index, defaultValue);

	public static int GetInt(this List<MessageParam> list, int index, int defaultValue = 0)
		=> list.Get(index, defaultValue);

	public static bool GetBool(this List<MessageParam> list, int index, bool defaultValue = false)
		=> list.Get(index, defaultValue);

	public static double GetDouble(this List<MessageParam> list, int index, double defaultValue = 0d)
		=> list.Get(index, defaultValue);

	public static float GetFloat(this List<MessageParam> list, int index, float defaultValue = 0f)
		=> list.Get(index, defaultValue);

	public static long GetLong(this List<MessageParam> list, int index, long defaultValue = 0L)
		=> list.Get(index, defaultValue);

	public static DateTime GetDateTime(this List<MessageParam> list, int index, DateTime? defaultValue = null)
	{
		var val = list.Get<object>(index);
		if (val == null) return defaultValue ?? DateTime.MinValue;

		if (val is DateTime dt) return dt;
		if (DateTime.TryParse(val.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
			return parsed;

		return defaultValue ?? DateTime.MinValue;
	}

	#endregion

	#region === TryGet 安全版本 ===

	public static bool TryGet<T>(this List<MessageParam> list, int index, out T? value)
	{
		value = default;
		if (list == null || index < 0 || index >= list.Count) return false;
		value = list[index].GetValue<T>();
		return true;
	}

	#endregion

	#region === 判断辅助 ===

	/// <summary>列表是否为空或为 null</summary>
	public static bool IsEmpty(this List<MessageParam>? list) => list == null || list.Count == 0;

	/// <summary>是否包含索引</summary>
	public static bool Has(this List<MessageParam>? list, int index) => list != null && index >= 0 && index < list.Count;

	#endregion

	#region === 批量转换 ===

	/// <summary>
	/// 将所有参数转换为字符串数组
	/// </summary>
	public static string[] ToStringArray(this List<MessageParam> list)
		=> list?.Select(p => p.GetValue<string>() ?? string.Empty).ToArray() ?? Array.Empty<string>();

	/// <summary>
	/// 将参数转换为字典（键 = 索引，值 = 字符串）
	/// </summary>
	public static Dictionary<int, string> ToDictionary(this List<MessageParam> list)
	{
		var dict = new Dictionary<int, string>();
		if (list == null) return dict;
		for (int i = 0; i < list.Count; i++)
			dict[i] = list[i].GetValue<string>() ?? "";
		return dict;
	}

	#endregion

	#region === 枚举支持 ===

	/// <summary>
	/// 获取枚举值（自动解析字符串或数字）
	/// </summary>
	public static TEnum GetEnum<TEnum>(this List<MessageParam> list, int index, TEnum defaultValue = default)
		where TEnum : struct, Enum
	{
		var val = list.Get<object>(index);
		if (val == null) return defaultValue;

		if (val is TEnum e) return e;
		if (Enum.TryParse(val.ToString(), true, out TEnum result))
			return result;

		return defaultValue;
	}

	#endregion
}
