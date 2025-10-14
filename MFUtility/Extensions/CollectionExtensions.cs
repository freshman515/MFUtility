using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MFUtility.Extensions;

public static class CollectionExtensions
{
    #region 🔹 基础迭代与判断

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
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

    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in source)
        {
            if (seen.Add(selector(item)))
                yield return item;
        }
    }

    public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int size)
    {
        if (source == null) yield break;
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));

        var chunk = new List<T>(size);
        foreach (var item in source)
        {
            chunk.Add(item);
            if (chunk.Count == size)
            {
                yield return chunk.ToList();
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
            yield return chunk;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where(x => x != null)!;
    }

    #endregion

    #region 🔹 集合修改（Add / Remove / Replace）

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T>? items)
    {
        if (collection == null || items == null) return;
        foreach (var item in items)
            collection.Add(item);
    }

    public static int RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        if (collection == null || predicate == null) return 0;

        var toRemove = collection.Where(predicate).ToList();
        foreach (var item in toRemove)
            collection.Remove(item);

        return toRemove.Count;
    }

    public static void Replace<T>(this IList<T> list, IEnumerable<T> newItems)
    {
        if (list == null || newItems == null) return;

        list.Clear();
        foreach (var item in newItems)
            list.Add(item);
    }

    public static void Replace<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
    {
        if (collection == null || newItems == null) return;

        collection.Clear();
        foreach (var item in newItems)
            collection.Add(item);
    }

    #endregion

    #region 🔹 字典相关

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict,
        TKey key, Func<TKey, TValue> factory)
    {
        if (dict == null) throw new ArgumentNullException(nameof(dict));
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        TValue value;
        if (!dict.TryGetValue(key, out value))
        {
            value = factory(key);
            dict[key] = value;
        }

        return value;
    }

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict,
        TKey key, TValue value)
    {
        if (dict == null) throw new ArgumentNullException(nameof(dict));
        dict[key] = value;
    }

    public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> target,
        IDictionary<TKey, TValue> source, bool overwrite = true)
    {
        if (target == null || source == null) return;
        foreach (var kvp in source)
        {
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
    public static void Shuffle<T>(this IList<T> list, int? seed = null)
    {
        if (list == null || list.Count < 2) return;
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count, int? seed = null)
    {
        if (source == null) return Enumerable.Empty<T>();
        var list = source.ToList();
        list.Shuffle(seed);
        return list.Take(count);
    }

    public static T? GetRandom<T>(this IEnumerable<T> source, int? seed = null)
    {
        if (source == null || !source.Any()) return default;
        var list = source.ToList();
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        return list[rng.Next(list.Count)];
    }

    #endregion
}
