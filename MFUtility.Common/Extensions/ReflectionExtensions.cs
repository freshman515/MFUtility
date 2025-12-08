using System.Collections.Concurrent;
using System.Reflection;

namespace MFUtility.Common.Extensions;

/// <summary>
/// 🔍 ReflectionExtensions：全功能反射工具扩展类
/// 支持：
/// - 类型分析与缓存
/// - 属性/字段访问与赋值（含私有）
/// - 方法与泛型调用
/// - 特性读取
/// - 对象映射与批量赋值
/// - 泛型类型判断与参数提取
/// - 调试与类型结构输出
/// </summary>
public static class ReflectionExtensions {

	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
	private static readonly ConcurrentDictionary<Type, FieldInfo[]> _fieldCache = new();
	private static readonly ConcurrentDictionary<Type, MethodInfo[]> _methodCache = new();

	#region === 类型基础信息 ===

	/// <summary> 获取类型的所有公共属性（带缓存） </summary>
	public static PropertyInfo[] GetCachedProperties(this Type type) =>
		_propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

	/// <summary> 获取类型的所有公共字段（带缓存） </summary>
	public static FieldInfo[] GetCachedFields(this Type type) =>
		_fieldCache.GetOrAdd(type, t => t.GetFields(BindingFlags.Public | BindingFlags.Instance));

	/// <summary> 获取类型的所有公共方法（带缓存） </summary>
	public static MethodInfo[] GetCachedMethods(this Type type) =>
		_methodCache.GetOrAdd(type, t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance));

	/// <summary> 判断类型是否实现了指定接口 </summary>
	public static bool ImplementsInterface<TInterface>(this Type type) =>
		typeof(TInterface).IsAssignableFrom(type);

	/// <summary> 判断类型是否继承自指定基类 </summary>
	public static bool InheritsFrom<TBase>(this Type type) =>
		type.IsSubclassOf(typeof(TBase));

	#endregion

	#region === 属性与字段操作 ===

	/// <summary> 获取对象指定属性的值（不存在则返回默认） </summary>
	public static object? GetPropertyValue(this object obj, string propertyName) {
		if (obj == null || string.IsNullOrWhiteSpace(propertyName))
			return null;

		var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
		return prop?.GetValue(obj);
	}

	/// <summary> 设置对象指定属性的值（支持自动类型转换） </summary>
	public static bool SetPropertyValue(this object obj, string propertyName, object? value) {
		if (obj == null || string.IsNullOrWhiteSpace(propertyName))
			return false;

		var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
		if (prop == null || !prop.CanWrite) return false;

		try {
			var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
			if (value != null && !targetType.IsAssignableFrom(value.GetType()))
				value = Convert.ChangeType(value, targetType);
			prop.SetValue(obj, value);
			return true;
		} catch { return false; }
	}

	/// <summary> 获取字段值（支持私有） </summary>
	public static object? GetFieldValue(this object obj, string fieldName) {
		if (obj == null || string.IsNullOrWhiteSpace(fieldName))
			return null;
		var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		return field?.GetValue(obj);
	}

	/// <summary> 设置字段值（支持私有） </summary>
	public static bool SetFieldValue(this object obj, string fieldName, object? value) {
		if (obj == null || string.IsNullOrWhiteSpace(fieldName))
			return false;
		var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (field == null) return false;
		field.SetValue(obj, value);
		return true;
	}

	#endregion

	#region === 私有成员访问 ===

	/// <summary> 获取私有字段或属性的值（递归查找继承链） </summary>
	public static object? GetPrivateMember(this object obj, string name) {
		if (obj == null || string.IsNullOrWhiteSpace(name)) return null;
		var type = obj.GetType();
		while (type != null) {
			var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null) return field.GetValue(obj);
			var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
			if (prop != null) return prop.GetValue(obj);
			type = type.BaseType;
		}
		return null;
	}

	/// <summary> 设置私有字段或属性的值（递归查找继承链） </summary>
	public static bool SetPrivateMember(this object obj, string name, object? value) {
		if (obj == null || string.IsNullOrWhiteSpace(name)) return false;
		var type = obj.GetType();
		while (type != null) {
			var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null) { field.SetValue(obj, value); return true; }
			var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
			if (prop != null && prop.CanWrite) { prop.SetValue(obj, value); return true; }
			type = type.BaseType;
		}
		return false;
	}

	#endregion

	#region === 方法调用 ===

	/// <summary> 调用实例方法（支持私有） </summary>
	public static object? InvokeMethod(this object obj, string methodName, params object?[]? parameters) {
		if (obj == null || string.IsNullOrWhiteSpace(methodName)) return null;

		var method = obj.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (method == null)
			throw new MissingMethodException(obj.GetType().FullName, methodName);

		return method.Invoke(obj, parameters);
	}

	/// <summary> 调用泛型方法（动态指定泛型类型） </summary>
	public static object? InvokeGenericMethod(this object obj, string methodName, Type genericType, params object?[]? parameters) {
		if (obj == null) return null;

		var method = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.FirstOrDefault(m => m.Name == methodName && m.IsGenericMethodDefinition);

		if (method == null)
			throw new MissingMethodException(obj.GetType().FullName, methodName);

		var genericMethod = method.MakeGenericMethod(genericType);
		return genericMethod.Invoke(obj, parameters);
	}

	#endregion

	#region === 属性拷贝与批量赋值 ===

	/// <summary> 将源对象属性复制到目标对象（同名属性匹配） </summary>
	public static void CopyPropertiesTo(this object source, object target, bool ignoreNull = false) {
		if (source == null || target == null) return;

		var srcProps = source.GetType().GetCachedProperties();
		var tgtProps = target.GetType().GetCachedProperties().ToDictionary(p => p.Name);

		foreach (var src in srcProps) {
			if (!src.CanRead) continue;
			if (!tgtProps.TryGetValue(src.Name, out var tgt)) continue;
			if (!tgt.CanWrite) continue;

			var value = src.GetValue(source);
			if (ignoreNull && value == null) continue;

			if (tgt.PropertyType.IsAssignableFrom(src.PropertyType))
				tgt.SetValue(target, value);
			else if (value != null)
				try { tgt.SetValue(target, Convert.ChangeType(value, tgt.PropertyType)); } catch { }
		}
	}

	/// <summary> 批量设置对象属性（支持 Dictionary 或匿名对象） </summary>
	public static void SetProperties(this object obj, object values) {
		if (obj == null || values == null) return;

		var dict = values switch {
			IDictionary<string, object?> d => d,
			_ => values.GetType().GetCachedProperties().ToDictionary(p => p.Name, p => p.GetValue(values))
		};

		foreach (var kv in dict)
			obj.SetPropertyValue(kv.Key, kv.Value);
	}

	#endregion

	#region === 特性与元数据 ===

	/// <summary> 获取类型上指定特性 </summary>
	public static TAttr? GetAttribute<TAttr>(this Type type, bool inherit = true) where TAttr : Attribute =>
		type.GetCustomAttributes(typeof(TAttr), inherit).FirstOrDefault() as TAttr;

	/// <summary> 获取属性上指定特性 </summary>
	public static TAttr? GetAttribute<TAttr>(this PropertyInfo property, bool inherit = true) where TAttr : Attribute =>
		property.GetCustomAttributes(typeof(TAttr), inherit).FirstOrDefault() as TAttr;

	/// <summary> 判断成员是否带有指定特性 </summary>
	public static bool HasAttribute<TAttr>(this MemberInfo member, bool inherit = true) where TAttr : Attribute =>
		member.GetCustomAttributes(typeof(TAttr), inherit).Any();

	#endregion

	#region === 泛型工具 ===

	/// <summary> 判断类型是否为指定开放泛型的实现 </summary>
	public static bool IsGenericOf(this Type type, Type genericTypeDef) {
		if (type == null || genericTypeDef == null) return false;
		if (!genericTypeDef.IsGenericTypeDefinition) return false;
		return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDef;
	}

	/// <summary> 获取类型的第 n 个泛型参数 </summary>
	public static Type? GetGenericArgument(this Type type, int index = 0) {
		if (!type.IsGenericType) return null;
		var args = type.GetGenericArguments();
		return (index >= 0 && index < args.Length) ? args[index] : null;
	}

	/// <summary> 获取继承自某泛型基类的实际参数类型 </summary>
	public static Type? GetGenericBaseArgument(this Type type, Type genericBaseType) {
		while (type != null && type != typeof(object)) {
			if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBaseType)
				return type.GetGenericArguments().FirstOrDefault();
			type = type.BaseType;
		}
		return null;
	}

	#endregion

	#region === 类型成员枚举 ===

	/// <summary> 获取类型及基类的所有字段（含私有） </summary>
	public static IEnumerable<FieldInfo> GetAllFields(this Type type) {
		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		while (type != null) {
			foreach (var field in type.GetFields(flags))
				yield return field;
			type = type.BaseType;
		}
	}

	/// <summary> 获取类型及基类的所有属性（含私有） </summary>
	public static IEnumerable<PropertyInfo> GetAllProperties(this Type type) {
		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		while (type != null) {
			foreach (var prop in type.GetProperties(flags))
				yield return prop;
			type = type.BaseType;
		}
	}

	#endregion

	#region === 对象实例化 ===

	/// <summary> 尝试创建类型实例（支持私有构造） </summary>
	public static object? CreateInstanceUnsafe(this Type type, params object?[]? args) {
		try {
			return Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null);
		} catch { return null; }
	}

	#endregion

	#region === 辅助工具 ===

	/// <summary> 获取对象的所有公共属性键值对 </summary>
	public static Dictionary<string, object?> ToPropertyDictionary(this object obj) {
		if (obj == null) return new();
		return obj.GetType().GetCachedProperties()
			.Where(p => p.CanRead)
			.ToDictionary(p => p.Name, p => p.GetValue(obj));
	}

	/// <summary> 判断对象是否包含指定属性 </summary>
	public static bool HasProperty(this object obj, string propertyName) {
		if (obj == null || string.IsNullOrWhiteSpace(propertyName)) return false;
		return obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null;
	}

	/// <summary> 打印类型结构（调试用） </summary>
	public static string DumpStructure(this Type type) {
		var props = type.GetCachedProperties().Select(p => $"  P: {p.PropertyType.Name} {p.Name}");
		var fields = type.GetCachedFields().Select(f => $"  F: {f.FieldType.Name} {f.Name}");
		return $"Type: {type.FullName}\n" + string.Join("\n", props.Concat(fields));
	}

	#endregion
}
