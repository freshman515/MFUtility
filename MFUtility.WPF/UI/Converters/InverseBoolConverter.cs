using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class InverseBoolConverter : IValueConverter
{
    private static readonly InverseBoolConverter _instance = new();
    
    public static InverseBoolConverter Instance => _instance;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}
