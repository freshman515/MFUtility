using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

/// <summary>
/// 包装枚举的通用类型，包含描述、名称、值、所有 Attribute 信息。
/// </summary>
public sealed class EnumWrapper<T> where T : struct, Enum
{
    /// <summary>枚举实际值</summary>
    public T Value { get; }

    /// <summary>枚举整数值</summary>
    public int IntValue => Convert.ToInt32(Value);

    /// <summary>枚举名称（如 "Connected"）</summary>
    public string Name => Value.ToString();

    /// <summary>描述文本（支持 Description / Display(Name)）</summary>
    public string Description { get; }

    /// <summary>字段对应的所有 Attribute 实例</summary>
    public IReadOnlyList<Attribute> Attributes { get; }

    /// <summary>从缓存中构造或初始化新的包装对象</summary>
    private EnumWrapper(T value)
    {
        Value = value;

        // 取字段
        var field = typeof(T).GetField(value.ToString())!;
        var attrs = field.GetCustomAttributes(true).OfType<Attribute>().ToList();

        Attributes = attrs;

        // 优先取 Description，再取 Display
        Description = field.GetCustomAttribute<DescriptionAttribute>()?.Description ??
                      // field.GetCustomAttribute<DisplayAttribute>()?.Name ??
                      value.ToString();
    }

    // 缓存，避免重复反射
    private static readonly Dictionary<T, EnumWrapper<T>> _cache = new();

    /// <summary>
    /// 获取包装对象（带缓存）
    /// </summary>
    public static EnumWrapper<T> From(T value)
    {
        if (_cache.TryGetValue(value, out var existing))
            return existing;

        var wrapper = new EnumWrapper<T>(value);
        _cache[value] = wrapper;
        return wrapper;
    }

    /// <summary>
    /// 获取当前枚举的所有包装对象列表
    /// </summary>
    public static IReadOnlyList<EnumWrapper<T>> GetAll()
        => Enum.GetValues(typeof(T)).Cast<T>().Select(From).ToList();

    /// <summary>
    /// 获取指定类型 Attribute（若存在）
    /// </summary>
    public TAttr? GetAttribute<TAttr>() where TAttr : Attribute
        => Attributes.OfType<TAttr>().FirstOrDefault();

    /// <summary>
    /// 获取 Attribute 的属性值（通过选择器）
    /// </summary>
    public TValue? GetAttributeValue<TAttr, TValue>(Func<TAttr, TValue> selector)
        where TAttr : Attribute
        => GetAttribute<TAttr>() is TAttr attr ? selector(attr) : default;

    public override string ToString() => Description;

    #region === 隐式转换支持 ===

    public static implicit operator T(EnumWrapper<T> wrapper) => wrapper.Value;

    public static implicit operator EnumWrapper<T>(T value) => From(value);

    #endregion
}