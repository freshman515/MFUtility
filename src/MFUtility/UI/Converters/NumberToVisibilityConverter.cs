using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class NumberToVisibilityConverter : IValueConverter
{
    private static readonly NumberToVisibilityConverter _instance = new();
    
    public static NumberToVisibilityConverter Instance => _instance;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is double d && d > 0) return Visibility.Visible;
		if (value is int i && i > 0) return Visibility.Visible;
		return Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> Binding.DoNothing;
}