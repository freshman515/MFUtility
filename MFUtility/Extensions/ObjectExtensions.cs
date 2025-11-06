using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace MFUtility.Extensions;

public static class ObjectExtensions {

	/// <summary>
	/// 通用对象克隆（可选浅拷贝或深拷贝）
	/// </summary>
	/// <typeparam name="T">对象类型</typeparam>
	/// <param name="source">源对象</param>
	/// <param name="deep">是否执行深拷贝</param>
	/// <returns>克隆的新对象</returns>
	public static T? Clone<T>(this T source, bool deep = false) {
		if (source == null)
			return default;

		if (!deep)
			return ShallowClone(source);

		return DeepClone(source);
	}


	/// <summary>
	/// 浅拷贝（仅复制值类型与引用地址）
	/// </summary>
	private static T ShallowClone<T>(T source) {
		var type = source!.GetType();

		// 值类型或字符串直接返回
		if (type.IsValueType || type == typeof(string))
			return source;

		// ICloneable 支持
		if (source is ICloneable cloneable)
			return (T)cloneable.Clone();

		// 通过反射创建副本
		var clone = Activator.CreateInstance(type)!;
		foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
			if (!prop.CanRead || !prop.CanWrite) continue;
			var value = prop.GetValue(source);
			prop.SetValue(clone, value);
		}

		foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
			var value = field.GetValue(source);
			field.SetValue(clone, value);
		}

		return (T)clone;
	}

	/// <summary>
	/// 深拷贝（递归复制所有引用对象）
	/// </summary>
	private static T DeepClone<T>(T source) {
		if (source == null)
			return default!;

		var visited = new Dictionary<object, object>(new ReferenceEqualityComparer());
		return (T)DeepCopy(source!, visited);
	}

	private static object DeepCopy(object obj, IDictionary<object, object> visited) {
		var type = obj.GetType();

		// 值类型或字符串直接返回
		if (type.IsValueType || type == typeof(string))
			return obj;

		// 避免循环引用
		if (visited.TryGetValue(obj, out var existing))
			return existing;

		if (type.IsArray) {
			var array = (Array)obj;
			var clone = (Array)Activator.CreateInstance(type, array.Length)!;
			visited[obj] = clone;

			for (int i = 0; i < array.Length; i++) {
				var item = array.GetValue(i);
				clone.SetValue(item != null ? DeepCopy(item, visited) : null, i);
			}

			return clone;
		}

		// 集合类型
		if (typeof(IEnumerable).IsAssignableFrom(type)) {
			var clone = Activator.CreateInstance(type)!;
			visited[obj] = clone;

			var addMethod = type.GetMethod("Add");
			if (addMethod != null) {
				foreach (var item in (IEnumerable)obj) {
					var copy = item != null ? DeepCopy(item, visited) : null;
					addMethod.Invoke(clone, new[] { copy });
				}
			}

			return clone;
		}

		// 普通类
		var instance = Activator.CreateInstance(type)!;
		visited[obj] = instance;

		foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
			if (!prop.CanRead || !prop.CanWrite) continue;
			var value = prop.GetValue(obj);
			if (value == null) continue;
			prop.SetValue(instance, DeepCopy(value, visited));
		}

		foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
			var value = field.GetValue(obj);
			if (value == null) continue;
			field.SetValue(instance, DeepCopy(value, visited));
		}

		return instance;
	}

	/// <summary>
	/// 二进制序列化克隆（要求可序列化）
	/// </summary>
	public static T? CloneBinary<T>(this T source) {
		if (source == null) return default;
		using var ms = new MemoryStream();
#pragma warning disable SYSLIB0011
		var formatter = new BinaryFormatter();
		formatter.Serialize(ms, source);
		ms.Position = 0;
		return (T)formatter.Deserialize(ms);
#pragma warning restore SYSLIB0011
	}

	/// <summary>
	/// JSON 克隆（最简单的跨类型深拷贝）
	/// </summary>
	public static T? CloneJson<T>(this T source) {
		if (source == null) return default;
		var json = JsonConvert.SerializeObject(source);
		return JsonConvert.DeserializeObject<T>(json);
	}

	// 内部引用相等比较器（防止循环引用）
	private sealed class ReferenceEqualityComparer : IEqualityComparer<object> {
		public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
		public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}

	/// <summary>
	/// 将源对象的公共属性复制到目标对象（忽略类型不兼容或只读属性）
	/// </summary>
	public static void CopyTo<TSource, TTarget>(this TSource source, TTarget target) {
		if (source == null || target == null) return;

		var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var tgtProps = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(p => p.CanWrite)
			.ToDictionary(p => p.Name);

		foreach (var srcProp in srcProps) {
			if (!srcProp.CanRead) continue;
			if (tgtProps.TryGetValue(srcProp.Name, out var tgtProp)) {
				try {
					var value = srcProp.GetValue(source);
					if (value != null && tgtProp.PropertyType.IsAssignableFrom(srcProp.PropertyType)) {
						tgtProp.SetValue(target, value);
					}
				} catch { /* ignore type mismatch */
				}
			}
		}
	}

	/// <summary>
	/// 从源对象复制公共属性到当前对象（忽略类型不兼容或只读属性）
	/// </summary>
	/// <typeparam name="TTarget">目标对象类型</typeparam>
	/// <param name="target">要被赋值的对象</param>
	/// <param name="source">提供数据的源对象</param>
	public static TTarget CopyFrom<TTarget>(this TTarget target, object source) {
		if (target == null || source == null)
			return target;

		var srcType = source.GetType();
		var tgtType = typeof(TTarget);

		var srcProps = srcType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var tgtProps = tgtType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(p => p.CanWrite)
			.ToDictionary(p => p.Name);

		foreach (var srcProp in srcProps) {
			if (!srcProp.CanRead) continue;
			if (tgtProps.TryGetValue(srcProp.Name, out var tgtProp)) {
				try {
					var value = srcProp.GetValue(source);
					if (value != null && tgtProp.PropertyType.IsAssignableFrom(srcProp.PropertyType)) {
						tgtProp.SetValue(target, value);
					}
				} catch {
					// 忽略类型不匹配或写入错误
				}
			}
		}

		return target;
	}

	/// <summary>
	/// 尝试将对象安全转换为指定类型
	/// </summary>
	public static T? As<T>(this object? obj, T? defaultValue = default) {
		try {
			if (obj == null) return defaultValue;
			if (obj is T variable) return variable;
			return (T)Convert.ChangeType(obj, typeof(T));
		} catch {
			return defaultValue;
		}
	}
}