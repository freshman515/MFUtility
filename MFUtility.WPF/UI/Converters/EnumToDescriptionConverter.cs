using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    private static readonly EnumToDescriptionConverter _instance = new();
    
    public static EnumToDescriptionConverter Instance => _instance;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        FieldInfo? field = value.GetType().GetField(value.ToString()!);
        if (field?.GetCustomAttribute<DescriptionAttribute>() is { } desc)
            return desc.Description;

        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
