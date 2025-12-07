using System.Reflection;

namespace MFUtility.Helpers;

public static class ReflectionHelper {
	 /// <summary>
    /// 创建指定类型实例（带默认构造或参数）
    /// </summary>
    public static object? CreateInstance(Type type, params object[]? args)
    {
        try
        {
            return Activator.CreateInstance(type, args ?? Array.Empty<object>());
        }
        catch
        {
            return null;
        }
    }
       /// <summary>
    /// 泛型版本：创建 T 实例
    /// </summary>
    public static T? CreateInstance<T>(params object[]? args)
    {
        var obj = CreateInstance(typeof(T), args);
        return obj is T t ? t : default;
    }
          /// <summary>
    /// 获取属性值
    /// </summary>
    public static object? GetPropertyValue(object target, string propertyName)
    {
        if (target == null || string.IsNullOrEmpty(propertyName))
            return null;

        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(target, null);
    }
 /// <summary>
    /// 设置属性值
    /// </summary>
    public static bool SetPropertyValue(object target, string propertyName, object? value)
    {
        if (target == null || string.IsNullOrEmpty(propertyName))
            return false;

        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null || !prop.CanWrite) return false;

        try
        {
            prop.SetValue(target, value, null);
            return true;
        }
        catch
        {
            return false;
        }
    }
 /// <summary>
    /// 获取字段值（包括私有字段）
    /// </summary>
    public static object? GetFieldValue(object target, string fieldName)
    {
        if (target == null) return null;

        var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(target);
    }
  /// <summary>
    /// 设置字段值
    /// </summary>
    public static bool SetFieldValue(object target, string fieldName, object? value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null) return false;
        try
        {
            field.SetValue(target, value);
            return true;
        }
        catch
        {
            return false;
        }
    }
  /// <summary>
    /// 获取类型的所有属性名
    /// </summary>
    public static string[] GetPropertyNames(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToArray();
}