using System.Collections.ObjectModel;

namespace MFUtility.Common.Extensions;

public static class EnumerableExtensions {
    #region 🔹 基础迭代与判断

	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
		if (source == null || action == null) return;
		foreach (var item in source)
			action(item);
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
		=> source == null || !source.Any();

	public static bool AnySafe<T>(this IEnumerable<T>? source, Func<T, bool>? predicate = null)
		=> source != null && (predicate == null ? source.Any() : source.Any(predicate));

    #endregion

    #region 🔹 集合操作（Distinct / Chunk / Filter）

	public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector) {
		var seen = new HashSet<TKey>();
		foreach (var item in source) {
			if (seen.Add(selector(item)))
				yield return item;
		}
	}

	public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int size) {
		if (source == null) yield break;
		if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

		var chunk = new List<T>(size);
		foreach (var item in source) {
			chunk.Add(item);
			if (chunk.Count == size) {
				yield return chunk.ToList();
				chunk.Clear();
			}
		}

		if (chunk.Count > 0)
			yield return chunk;
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
		where T : class {
		return source.Where(x => x != null)!;
	}

    #endregion

    #region 🔹 集合修改（Add / Remove / Replace）

	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T>? items) {
		if (collection == null || items == null) return;
		foreach (var item in items)
			collection.Add(item);
	}

	public static int RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate) {
		if (collection == null || predicate == null) return 0;

		var toRemove = collection.Where(predicate).ToList();
		foreach (var item in toRemove)
			collection.Remove(item);

		return toRemove.Count;
	}
	/// <summary>
	/// 删除所有满足条件的元素（比 RemoveWhere 更通用，可用于 List / ICollection）
	/// </summary>
	public static int RemoveAllWhere<T>(this ICollection<T> collection, Func<T, bool> predicate) {
		if (collection == null || predicate == null) return 0;

		var toRemove = collection.Where(predicate).ToList();
		foreach (var item in toRemove)
			collection.Remove(item);

		return toRemove.Count;
	}
	/// <summary>
	/// 删除不满足条件的元素，仅保留符合条件的部分。
	/// </summary>
	public static int RemoveExceptWhere<T>(this ICollection<T> collection, Func<T, bool> predicate) {
		if (collection == null || predicate == null) return 0;

		var toRemove = collection.Where(x => !predicate(x)).ToList();
		foreach (var item in toRemove)
			collection.Remove(item);

		return toRemove.Count;
	}
	/// <summary>
	/// 用新元素替换所有满足条件的旧元素。
	/// </summary>
	public static int ReplaceWhere<T>(this IList<T> list, Func<T, bool> predicate, T newItem) {
		if (list == null || predicate == null) return 0;

		int count = 0;
		for (int i = 0; i < list.Count; i++) {
			if (predicate(list[i])) {
				list[i] = newItem;
				count++;
			}
		}

		return count;
	}

	/// <summary>
	/// 当 condition 为 true 时才执行 Where 过滤，否则返回原集合。
	/// </summary>
	public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate) {
		if (source == null) return Enumerable.Empty<T>();
		return condition ? source.Where(predicate) : source;
	}

	/// <summary>
	/// 判断集合中是否存在满足条件的元素（空安全）。
	/// </summary>
	public static bool AnyWhere<T>(this IEnumerable<T>? source, Func<T, bool> predicate) {
		return source != null && predicate != null && source.Any(predicate);
	}
	/// <summary>
	/// 判断集合是否为空或不包含任何元素。
	/// </summary>
	public static bool None<T>(this IEnumerable<T>? source)
		=> source == null || !source.Any();
	/// <summary>
	/// 查找满足条件的第一个元素，找不到则返回默认值。
	/// </summary>
	///
	/// /// <summary>
	/// 判断集合中是否不存在满足条件的元素。
	/// </summary>
	public static bool NoneWhere<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
		=> source == null || !source.Any(predicate);

	/// <summary>
	/// 统计满足条件的元素个数（空安全）。
	/// </summary>
	public static int CountWhere<T>(this IEnumerable<T>? source, Func<T, bool> predicate) {
		return source == null ? 0 : source.Count(predicate);
	}

	/// <summary>
	/// 判断集合中所有元素是否都满足指定条件（空集合返回 false）。
	/// </summary>
	public static bool AllWhere<T>(this IEnumerable<T>? source, Func<T, bool> predicate) {
		if (source == null || !source.Any()) return false;
		return source.All(predicate);
	}
	public static T? FindWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate, T? defaultValue = default) {
		if (source == null || predicate == null) return defaultValue;
		return source.FirstOrDefault(predicate) ?? defaultValue;
	}
	public static void Replace<T>(this IList<T> list, IEnumerable<T> newItems) {
		if (list == null || newItems == null) return;

		list.Clear();
		foreach (var item in newItems)
			list.Add(item);
	}

	public static void Replace<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems) {
		if (collection == null || newItems == null) return;

		collection.Clear();
		foreach (var item in newItems)
			collection.Add(item);
	}

    #endregion

    #region 🔹 字典相关

	public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict,
		TKey key, Func<TKey, TValue> factory) {
		if (dict == null) throw new ArgumentNullException(nameof(dict));
		if (factory == null) throw new ArgumentNullException(nameof(factory));

		TValue value;
		if (!dict.TryGetValue(key, out value)) {
			value = factory(key);
			dict[key] = value;
		}

		return value;
	}

	public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict,
		TKey key, TValue value) {
		if (dict == null) throw new ArgumentNullException(nameof(dict));
		dict[key] = value;
	}

	public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> target,
		IDictionary<TKey, TValue> source, bool overwrite = true) {
		if (target == null || source == null) return;
		foreach (var kvp in source) {
			if (overwrite || !target.ContainsKey(kvp.Key))
				target[kvp.Key] = kvp.Value;
		}
	}

    #endregion

    #region 🔹 转换辅助

	public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
		=> new ObservableCollection<T>(source ?? Enumerable.Empty<T>());

	public static List<T> ToListSafe<T>(this IEnumerable<T>? source)
		=> source == null ? new List<T>() : source.ToList();

	public static HashSet<T> ToHashSetSafe<T>(this IEnumerable<T>? source)
		=> source == null ? new HashSet<T>() : new HashSet<T>(source);

    #endregion

    #region 🎲 其它杂项

	/// <summary>随机打乱集合（Fisher–Yates Shuffle）</summary>
	public static void Shuffle<T>(this IList<T> list, int? seed = null) {
		if (list == null || list.Count < 2) return;
		var rng = seed.HasValue ? new Random(seed.Value) : new Random();

		for (int i = list.Count - 1; i > 0; i--) {
			int j = rng.Next(i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count, int? seed = null) {
		if (source == null) return Enumerable.Empty<T>();
		var list = source.ToList();
		list.Shuffle(seed);
		return list.Take(count);
	}

	public static T? GetRandom<T>(this IEnumerable<T> source, int? seed = null) {
		if (source == null || !source.Any()) return default;
		var list = source.ToList();
		var rng = seed.HasValue ? new Random(seed.Value) : new Random();
		return list[rng.Next(list.Count)];
	}
	public static bool HadItems<T>(this IEnumerable<T> values) => values != null && values.Any<T>();

    #endregion
}