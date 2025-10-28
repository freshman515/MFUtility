using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MFUtility.Helpers;

/// <summary>
/// 枚举辅助工具类：
/// - 获取描述（Description / Display）
/// - 获取任意 Attribute（自动推断枚举类型）
/// - 枚举 ↔ 字符串、列表、字典转换
/// </summary>
public static class EnumHelper {
	private static readonly ConcurrentDictionary<Enum, string> _descCache = new();
	private static readonly ConcurrentDictionary<Type, List<EnumItem>> _enumListCache = new();

	#region === 获取描述 ===

	private static readonly ConcurrentDictionary<(Type, Enum), Type?> _mappedTypeCache = new();

	/// <summary>
	/// 泛型通用方法：从源枚举读取特性 TAttr，找到目标枚举类型，
	/// 再根据目标值获取特性 TDisplay 中的显示名称。
	/// </summary>
	/// <typeparam name="TSource">源枚举类型（如 UnitCategory）</typeparam>
	/// <typeparam name="TAttr">映射特性类型（如 UnitTypeAttribute）</typeparam>
	/// <typeparam name="TDisplay">显示特性类型（如 DisplayAttribute 或 DescriptionAttribute）</typeparam>
	/// <param name="sourceEnum">源枚举值</param>
	/// <param name="targetValue">目标枚举的整型值</param>
	/// <param name="typeSelector">从 TAttr 中提取目标类型的委托（如 a => a.UnitEnumType）</param>
	/// <param name="displaySelector">从 TDisplay 特性中提取显示字符串的委托（如 d => d.Name 或 d.Description）</param>
	/// <returns>目标枚举的显示名称</returns>
	public static string GetMappedEnumDisplay<TSource, TAttr, TDisplay>(
		TSource sourceEnum,
		int targetValue,
		Func<TAttr, Type?> typeSelector,
		Func<TDisplay, string?> displaySelector)
		where TSource : struct, Enum
		where TAttr : Attribute
		where TDisplay : Attribute {
		try {
			if (!Enum.IsDefined(typeof(TSource), sourceEnum))
				return string.Empty;

			// ✅ 缓存目标类型
			var cacheKey = (typeof(TSource), (Enum)(object)sourceEnum);
			var targetType = _mappedTypeCache.GetOrAdd(cacheKey, key => {
				var field = key.Item1.GetField(key.Item2.ToString());
				var attr = field?.GetCustomAttribute<TAttr>();
				return attr != null ? typeSelector(attr) : null;
			});

			if (targetType == null)
				return string.Empty;

			if (!Enum.IsDefined(targetType, targetValue))
				return string.Empty;

			// ✅ 获取目标枚举值
			var enumValue = (Enum)Enum.ToObject(targetType, targetValue);
			var field = targetType.GetField(enumValue.ToString());
			if (field == null)
				return enumValue.ToString();

			// ✅ 查找显示特性
			var displayAttr = field.GetCustomAttribute<TDisplay>();
			var displayName = displayAttr != null ? displaySelector(displayAttr) : null;

			return !string.IsNullOrEmpty(displayName)
				? displayName
				: enumValue.ToString();
		} catch {
			return string.Empty;
		}
	}

	/// <summary>
	/// 获取枚举的描述文本（优先 Display(Name)，其次 Description）
	/// </summary>
	public static string GetDescription(Enum value) {
		if (value == null) return string.Empty;
		if (_descCache.TryGetValue(value, out var cached)) return cached;

		var field = value.GetType().GetField(value.ToString());
		if (field == null) return value.ToString();

		string result = value.ToString();

		// 优先 Display(Name)
		var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
		if (!string.IsNullOrEmpty(displayAttr?.Name))
			result = displayAttr.Name;
		else {
			// 再尝试 Description
			var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
			if (!string.IsNullOrEmpty(descAttr?.Description))
				result = descAttr.Description;
		}

		_descCache[value] = result;
		return result;
	}

	/// <summary>
	/// 获取枚举的 Display(Name) 值（没有则返回枚举名）
	/// </summary>
	public static string GetDisplayName(Enum value) {
		if (value == null) return string.Empty;

		var field = value.GetType().GetField(value.ToString());
		var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
		return !string.IsNullOrEmpty(displayAttr?.Name)
			? displayAttr.Name
			: value.ToString();
	}

	#endregion

	#region === 通用 Attribute 获取 ===

	public static TAttr? GetAttribute<TAttr>(Enum value)
		where TAttr : Attribute {
		if (value == null) return null;
		var field = value.GetType().GetField(value.ToString());
		return field?.GetCustomAttribute<TAttr>();
	}

	public static TValue? GetAttributeValue<TAttr, TValue>(
		Enum value,
		Func<TAttr, TValue> valueSelector)
		where TAttr : Attribute {
		var attr = GetAttribute<TAttr>(value);
		return attr == null ? default : valueSelector(attr);
	}

	public static Dictionary<TEnum, TAttr?> GetAttributes<TEnum, TAttr>()
		where TEnum : struct, Enum
		where TAttr : Attribute {
		var dict = new Dictionary<TEnum, TAttr?>();
		foreach (var val in Enum.GetValues(typeof(TEnum)).Cast<TEnum>()) {
			var field = typeof(TEnum).GetField(val.ToString());
			dict[val] = field?.GetCustomAttribute<TAttr>();
		}

		return dict;
	}

	#endregion

	public static List<EnumItem> GetEnumItems<T>() where T : struct, Enum {
		var type = typeof(T);
		if (_enumListCache.TryGetValue(type, out var cached))
			return cached;

		var list = new List<EnumItem>();
		foreach (var v in Enum.GetValues(type).Cast<Enum>()) {
			list.Add(new EnumItem {
				Name = v.ToString(),
				Value = Convert.ToInt32(v),
				Description = GetDescription(v)
			});
		}

		_enumListCache[type] = list;
		return list;
	}

	public static Dictionary<int, string> ToDictionary<T>() where T : struct, Enum
		=> GetEnumItems<T>().ToDictionary(x => x.Value, x => x.Description);

	public static Dictionary<string, string> ToNameDictionary<T>() where T : struct, Enum
		=> GetEnumItems<T>().ToDictionary(x => x.Name, x => x.Description);
}

/// <summary>
/// 枚举项模型（用于绑定）
/// </summary>
public class EnumItem {
	public string Name { get; set; } = string.Empty;
	public int Value { get; set; }
	public string Description { get; set; } = string.Empty;
	public override string ToString() => Description ?? Name;
}