using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MFUtility.Common.Extensions {
	/// <summary>
	/// 提供对 <see cref="Enum"/> 类型的通用扩展方法。
	/// </summary>
	public static class EnumExtensions {

        #region 🔹 枚举映射获取

		/// <summary>
		/// 根据源枚举值（带有特性标注）获取对应目标枚举中的第 index 个值。
		/// 例如：UnitCategory.Length → 其特性 [UnitType(typeof(LengthUnit))] → LengthUnit 枚举。
		/// </summary>
		/// <typeparam name="TAttr">映射特性类型（如 UnitTypeAttribute）</typeparam>
		/// <param name="sourceEnum">源枚举值（例如 UnitCategory.Length）</param>
		/// <param name="index">目标枚举中的索引</param>
		/// <param name="typeSelector">从特性中提取目标枚举类型的委托（如 a => a.UnitEnumType）</param>
		/// <returns>对应目标枚举类型中的枚举值</returns>
		public static Enum GetMappedEnumByAttribute<TAttr>(
			this Enum sourceEnum,
			int index,
			Func<TAttr, Type?> typeSelector)
			where TAttr : Attribute {
			if (sourceEnum == null)
				throw new ArgumentNullException(nameof(sourceEnum));

			// 取源枚举字段
			var field = sourceEnum.GetType().GetField(sourceEnum.ToString());
			if (field == null)
				throw new InvalidOperationException($"找不到枚举字段: {sourceEnum}");

			// 获取特性
			var attr = field.GetCustomAttribute<TAttr>();
			if (attr == null)
				throw new InvalidOperationException($"{sourceEnum} 没有特性 {typeof(TAttr).Name}");

			// 从特性中取目标枚举类型
			var targetType = typeSelector(attr);
			if (targetType == null)
				throw new InvalidOperationException($"{typeof(TAttr).Name} 返回的目标类型为 null");

			// 获取目标枚举值
			var values = Enum.GetValues(targetType);
			if (index < 0 || index >= values.Length)
				throw new IndexOutOfRangeException($"索引 {index} 超出范围 ({targetType.Name})");

			return (Enum)values.GetValue(index)!;
		}


		/// <summary>
		/// 根据特性映射获取目标枚举值的泛型版本。
		/// </summary>
		public static TTarget GetMappedEnum<TAttr, TTarget>(
			this Enum sourceEnum,
			int index,
			Func<TAttr, Type?> typeSelector)
			where TAttr : Attribute
			where TTarget : Enum {
			var result = GetMappedEnumByAttribute(sourceEnum, index, typeSelector);
			return (TTarget)result;
		}

		#endregion

        #region 🔹 获取描述或属性

		/// <summary>
		/// 根据描述或显示名称解析为枚举值（忽略大小写）
		/// </summary>
		public static T ParseByDescription<T>(this string description, bool ignoreCase = true) where T : Enum {
			foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)) {
				if (field.GetCustomAttribute<DescriptionAttribute>() is { } desc &&
				    string.Equals(desc.Description, description, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
					return (T)field.GetValue(null)!;

				if (field.GetCustomAttribute<DisplayAttribute>() is { } disp &&
				    string.Equals(disp.Name, description, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
					return (T)field.GetValue(null)!;

				if (string.Equals(field.Name, description, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
					return (T)field.GetValue(null)!;
			}

			throw new ArgumentException($"无法解析 \"{description}\" 到 {typeof(T).Name}");
		}
		/// <summary>
		/// 获取枚举项的友好名称（优先顺序：Display → Description → ToString）
		/// </summary>
		public static string GetFriendlyName(this Enum value) {
			var field = value.GetType().GetField(value.ToString());
			if (field == null) return value.ToString();

			// 优先 DisplayAttribute
			if (field.GetCustomAttribute<DisplayAttribute>() is DisplayAttribute display && !string.IsNullOrWhiteSpace(display.Name))
				return display.Name;

			// 其次 DescriptionAttribute
			if (field.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute desc && !string.IsNullOrWhiteSpace(desc.Description))
				return desc.Description;

			return value.ToString();
		}
		/// <summary>
		/// 获取枚举项的 <see cref="DescriptionAttribute"/> 描述，如果不存在则返回枚举名。
		/// </summary>
		public static string GetDescription(this Enum value) {
			var field = value.GetType().GetField(value.ToString());
			var attr = field?.GetCustomAttribute<DescriptionAttribute>();
			return attr?.Description ?? value.ToString();
		}

		/// <summary>
		/// 获取枚举项的 <see cref="DisplayAttribute"/> 名称，如果不存在则返回枚举名。
		/// </summary>
		public static string GetDisplayName(this Enum value) {
			var field = value.GetType().GetField(value.ToString());
			var attr = field?.GetCustomAttribute<DisplayAttribute>();
			return attr?.Name ?? value.ToString();
		}

		/// <summary>
		/// 获取指定类型的自定义特性。
		/// </summary>
		public static TAttr? GetAttribute<TAttr>(this Enum value) where TAttr : Attribute {
			var field = value.GetType().GetField(value.ToString());
			return field?.GetCustomAttribute<TAttr>();
		}

        #endregion

        #region 🔹 枚举集合操作

		/// <summary>
		/// 获取枚举类型的所有枚举值。
		/// </summary>
		public static IEnumerable<T> GetValues<T>() where T : Enum =>
			Enum.GetValues(typeof(T)).Cast<T>();

		/// <summary>
		/// 获取枚举类型的所有名称。
		/// </summary>
		public static IEnumerable<string> GetNames<T>() where T : Enum =>
			Enum.GetNames(typeof(T));

		/// <summary>
		/// 获取所有枚举项及描述的字典映射。
		/// </summary>
		public static Dictionary<T, string> ToDictionary<T>() where T : Enum =>
			GetValues<T>().ToDictionary(v => v, v => v.GetDescription());

		/// <summary>
		/// 获取所有枚举项与描述的列表。
		/// </summary>
		public static List<(T Value, string Name, string Description)> ToList<T>() where T : Enum =>
			GetValues<T>().Select(v => (v, v.ToString(), v.GetDescription())).ToList();

        #endregion

        #region 🔹 枚举解析 / 转换

		/// <summary>
		/// 将字符串解析为枚举，若失败则返回默认值。
		/// </summary>
		public static T ParseEnum<T>(this string value, bool ignoreCase = true, T defaultValue = default) where T : struct, Enum =>
			Enum.TryParse(value, ignoreCase, out T result) ? result : defaultValue;

		/// <summary>
		/// 尝试解析字符串为枚举值。
		/// </summary>
		public static bool TryParseEnum<T>(this string value, out T result, bool ignoreCase = true) where T : struct, Enum =>
			Enum.TryParse(value, ignoreCase, out result);

		/// <summary>
		/// 将枚举值转换为基础整数类型。
		/// </summary>
		public static long ToLong(this Enum value) => Convert.ToInt64(value);

		/// <summary>
		/// 将整数值转换为枚举类型（不安全转换）。
		/// </summary>
		public static T ToEnum<T>(this long value) where T : struct, Enum =>
			(T)Enum.ToObject(typeof(T), value);

        #endregion

        #region 🔹 Flag 操作

		/// <summary>
		/// 高性能检测枚举标志位（比 <see cref="Enum.HasFlag"/> 快）。
		/// </summary>
		public static bool HasFlagFast<T>(this T value, T flag) where T : Enum =>
			(Convert.ToInt64(value) & Convert.ToInt64(flag)) == Convert.ToInt64(flag);

		/// <summary>
		/// 添加标志位。
		/// </summary>
		public static T AddFlag<T>(this T value, T flag) where T : struct, Enum =>
			((T)Enum.ToObject(typeof(T), Convert.ToInt64(value) | Convert.ToInt64(flag)));

		/// <summary>
		/// 移除标志位。
		/// </summary>
		public static T RemoveFlag<T>(this T value, T flag) where T : struct, Enum =>
			((T)Enum.ToObject(typeof(T), Convert.ToInt64(value) & ~Convert.ToInt64(flag)));

		/// <summary>
		/// 切换标志位状态（有则去掉，无则添加）。
		/// </summary>
		public static T ToggleFlag<T>(this T value, T flag) where T : struct, Enum {
			long v = Convert.ToInt64(value);
			long f = Convert.ToInt64(flag);
			return (T)Enum.ToObject(typeof(T), v ^ f);
		}

        #endregion

        #region 🔹 辅助工具

		/// <summary>
		/// 获取当前枚举的整数值。
		/// </summary>
		public static int GetValue(this Enum value) => Convert.ToInt32(value);

		/// <summary>
		/// 获取当前枚举项名称。
		/// </summary>
		public static string GetName(this Enum value) => Enum.GetName(value.GetType(), value) ?? value.ToString();

		/// <summary>
		/// 检查枚举值是否定义在当前枚举类型中。
		/// </summary>
		public static bool IsDefined<T>(this T value) where T : Enum =>
			Enum.IsDefined(typeof(T), value);

        #endregion
	}
}