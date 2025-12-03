using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class StringToUpperConverter : IValueConverter
{
    private static readonly StringToUpperConverter _instance = new();
    
    public static StringToUpperConverter Instance => _instance;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString()?.ToUpper() ?? "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString()?.ToLower();
}