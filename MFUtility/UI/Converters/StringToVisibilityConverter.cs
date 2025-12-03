using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    private static readonly StringToVisibilityConverter _instance = new();
    
    public static StringToVisibilityConverter Instance => _instance;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}