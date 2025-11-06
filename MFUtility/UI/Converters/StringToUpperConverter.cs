using System.Globalization;
using System.Windows.Data;

namespace MFUtility.UI.Converters;

public class StringToUpperConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString()?.ToUpper() ?? "";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString()?.ToLower();
}