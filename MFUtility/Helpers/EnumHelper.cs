using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace MFUtility.Helpers;

/// <summary>
/// 枚举辅助工具类：
/// - 获取描述（Description / Display）
/// - 获取任意 Attribute（自动推断枚举类型）
/// - 枚举 ↔ 字符串、列表、字典转换
/// </summary>
public static class EnumHelper
{
    private static readonly ConcurrentDictionary<Enum, string> _descCache = new();
    private static readonly ConcurrentDictionary<Type, List<EnumItem>> _enumListCache = new();

    #region === 获取描述 ===

    /// <summary>
    /// 获取枚举的描述文本（优先 Description，其次 Display(Name)）
    /// </summary>
    public static string GetDescription(Enum value)
    {
        if (value == null) return string.Empty;
        if (_descCache.TryGetValue(value, out var cached)) return cached;

        var field = value.GetType().GetField(value.ToString());
        if (field == null) return value.ToString();

        string result = value.ToString();

        var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr != null)
            result = descAttr.Description;

        _descCache[value] = result;
        return result;
    }

    #endregion

    #region === 通用 Attribute 获取 ===

    /// <summary>
    /// 获取枚举值上的指定类型 Attribute（自动推断枚举类型）
    /// </summary>
    public static TAttr? GetAttribute<TAttr>(Enum value)
        where TAttr : Attribute
    {
        if (value == null) return null;
        var field = value.GetType().GetField(value.ToString());
        return field?.GetCustomAttribute<TAttr>();
    }

    /// <summary>
    /// 获取枚举值上 Attribute 的某个属性值（自动推断枚举类型）
    /// </summary>
    public static TValue? GetAttributeValue<TAttr, TValue>(
        Enum value,
        Func<TAttr, TValue> valueSelector)
        where TAttr : Attribute
    {
        var attr = GetAttribute<TAttr>(value);
        return attr == null ? default : valueSelector(attr);
    }

    /// <summary>
    /// 获取指定枚举类型所有字段的指定 Attribute
    /// </summary>
    public static Dictionary<TEnum, TAttr?> GetAttributes<TEnum, TAttr>()
        where TEnum : struct, Enum
        where TAttr : Attribute
    {
        var dict = new Dictionary<TEnum, TAttr?>();
        foreach (var val in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
        {
            var field = typeof(TEnum).GetField(val.ToString());
            dict[val] = field?.GetCustomAttribute<TAttr>();
        }
        return dict;
    }

    #endregion


    public static List<EnumItem> GetEnumItems<T>() where T : struct, Enum
    {
        var type = typeof(T);
        if (_enumListCache.TryGetValue(type, out var cached))
            return cached;

        var list = new List<EnumItem>();
        foreach (var v in Enum.GetValues(type).Cast<Enum>())
        {
            list.Add(new EnumItem
            {
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
public class EnumItem
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public override string ToString() => Description ?? Name;
}
