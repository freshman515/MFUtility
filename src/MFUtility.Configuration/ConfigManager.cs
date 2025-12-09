using System.Reflection;
using MFUtility.Common.Helpers;
using MFUtility.Configuration.Attributes;

namespace MFUtility.Configuration;

public static class ConfigManager {
	private static readonly Dictionary<string, object?> _cache = new();
	private static readonly Dictionary<string, FileSystemWatcher> _watchers = new();

	#region ========== Public API ==========

	/// <summary>
	/// 加载配置
	/// </summary>
	public static T Load<T>(string path = "") where T : class, new() {
		string file = string.IsNullOrWhiteSpace(path)
			? NormalizePath(ResolveFileName<T>())
			: NormalizePath(path);

		if (_cache.TryGetValue(file, out var cached))
			return (T)cached!;

		var obj = LoadFile<T>(file);
		_cache[file] = obj!;
		return obj!;
	}

	/// <summary>
	/// 保存配置
	/// </summary>
	public static void Save<T>(T value, string path = "") {
		string file = string.IsNullOrWhiteSpace(path)
			? NormalizePath(ResolveFileName<T>())
			: NormalizePath(path);

		SaveFile(file, value);
		_cache[file] = value!;
	}

	/// <summary>
	/// 监听文件变化（自动触发回调 + 自动 Reload）
	/// </summary>
	/// <summary>
	/// 监听配置文件变化（支持自定义路径）
	/// </summary>
	private static readonly Dictionary<string, DateTime> _lastTriggerTime = new();

	public static void Watch<T>(Action<T, T, List<PropertyChange>> onChanged, string path = "") {
		string file = string.IsNullOrWhiteSpace(path)
			? NormalizePath(ResolveFileName<T>())
			: NormalizePath(path);

		string dir = Path.GetDirectoryName(file)!;
		string name = Path.GetFileName(file);

		if (_watchers.ContainsKey(file))
			return;

		var watcher = new FileSystemWatcher(dir, name) {
			NotifyFilter = NotifyFilters.LastWrite,
			EnableRaisingEvents = true,
		};

		watcher.Changed += async (_, _) => {
			try {
				await Task.Delay(100);

				var now = DateTime.Now;
				if (_lastTriggerTime.TryGetValue(file, out var last)
				    && (now - last).TotalMilliseconds < 500) {
					return;
				}
				_lastTriggerTime[file] = now;

				// 旧对象
				T oldObj;
				if (_cache.TryGetValue(file, out var oldVal))
					oldObj = (T)oldVal!;
				else
					oldObj = Activator.CreateInstance<T>();

				// 新对象
				var newObj = LoadFile<T>(file);
				_cache[file] = newObj!;

				// 变化属性
				var changes = GetChanges(oldObj, newObj);

				// 回调
				onChanged(oldObj, newObj, changes);
			} catch {
			}
		};

		_watchers[file] = watcher;
	}
	private static List<PropertyChange> GetChanges<T>(T oldObj, T newObj) {
		var changes = new List<PropertyChange>();

		if (oldObj == null || newObj == null)
			return changes;

		foreach (var prop in typeof(T).GetProperties()) {
			if (!prop.CanRead) continue;

			var oldValue = prop.GetValue(oldObj);
			var newValue = prop.GetValue(newObj);

			// 简单类型 → 直接比较
			if (IsSimpleType(prop.PropertyType)) {
				if (!Equals(oldValue, newValue)) {
					changes.Add(new PropertyChange {
						Name = prop.Name,
						OldValue = oldValue,
						NewValue = newValue
					});
				}
			} else {
				// 🟢 嵌套对象，比较其内部属性是否有变化
				var nestedChanges = GetNestedChanges(prop.Name, oldValue, newValue);
				changes.AddRange(nestedChanges);
			}
		}

		return changes;
	}

	private static bool IsSimpleType(Type t) {
		return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal);
	}

	private static List<PropertyChange> GetNestedChanges(string parentName, object oldObj, object newObj) {
		var list = new List<PropertyChange>();
		if (oldObj == null || newObj == null) return list;

		foreach (var prop in oldObj.GetType().GetProperties()) {
			var oldValue = prop.GetValue(oldObj);
			var newValue = prop.GetValue(newObj);

			if (IsSimpleType(prop.PropertyType)) {
				if (!Equals(oldValue, newValue)) {
					list.Add(new PropertyChange {
						Name = parentName + "." + prop.Name,
						OldValue = oldValue,
						NewValue = newValue
					});
				}
			}
		}

		return list;
	}
	public static void Unwatch<T>(string path = "") {
		string file = string.IsNullOrWhiteSpace(path)
			? NormalizePath(ResolveFileName<T>())
			: NormalizePath(path);

		if (_watchers.TryGetValue(file, out var w)) {
			w.EnableRaisingEvents = false;
			w.Dispose();
			_watchers.Remove(file);
			_lastTriggerTime.Remove(file);
		}
	}

	#endregion

	#region ========== Internal Tools ==========

	private static string ResolveFileName<T>() {
		var attr = typeof(T).GetCustomAttribute<ConfigMapAttribute>();
		if (attr != null)
			return attr.FileName;

		// 自动推断：AppConfig → appconfig.json
		return typeof(T).Name.ToLower() + ".json";
	}

	private static string NormalizePath(string file) {
		if (Path.IsPathRooted(file))
			return file;

		var dir = Path.GetDirectoryName(file);

		// 文件名不包含目录 → 存到程序根目录
		if (string.IsNullOrEmpty(dir))
			return Path.Combine(AppContext.BaseDirectory, file);

		// 其他相对路径 → 转绝对路径
		return Path.GetFullPath(file);
	}

	private static T LoadFile<T>(string file) {
		if (!File.Exists(file))
			return Activator.CreateInstance<T>();

		string ext = Path.GetExtension(file).ToLowerInvariant();

		return ext switch {
			".json" => JsonHelper.Load<T>(file)!,
			".xml" => XmlHelper.Load<T>(file)!,
			".ini" => IniHelper.Load<T>(file)!,
			_ => throw new NotSupportedException(ext)
		};
	}

	private static void SaveFile<T>(string file, T value) {
		var dir = Path.GetDirectoryName(file);
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);

		string ext = Path.GetExtension(file).ToLowerInvariant();

		switch (ext) {
			case ".json":
				JsonHelper.Save(file, value);
				break;

			case ".xml":
				XmlHelper.Save(file, value);
				break;
			case ".ini":
				IniHelper.Save(file, value);
				break;

			default:
				throw new NotSupportedException(ext);
		}
	}

	#endregion
}