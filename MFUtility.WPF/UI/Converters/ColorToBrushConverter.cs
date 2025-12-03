using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MFUtility.WPF.UI.Converters;

public class ColorToBrushConverter : IValueConverter
{
    private static readonly ColorToBrushConverter _instance = new();
    
    public static ColorToBrushConverter Instance => _instance;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Color color ? new SolidColorBrush(color) : Brushes.Transparent;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is SolidColorBrush brush ? brush.Color : Colors.Transparent;
}