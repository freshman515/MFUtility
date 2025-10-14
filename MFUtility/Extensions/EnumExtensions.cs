using System.ComponentModel;
using System.Reflection;

namespace MFUtility.Extensions;
public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? value.ToString();
    }

    public static TAttr? GetAttribute<TAttr>(this Enum value) where TAttr : Attribute
    {
        var field = value.GetType().GetField(value.ToString());
        return field?.GetCustomAttribute<TAttr>();
    }

    public static IEnumerable<T> GetValues<T>() where T : Enum =>
        Enum.GetValues(typeof(T)).Cast<T>();

    public static T ParseEnum<T>(this string value, bool ignoreCase = true) where T : struct, Enum =>
        Enum.TryParse(value, ignoreCase, out T result) ? result : default;

    public static bool HasFlagFast<T>(this T value, T flag) where T : Enum =>
        Convert.ToInt64(value) == (Convert.ToInt64(value) | Convert.ToInt64(flag));
}