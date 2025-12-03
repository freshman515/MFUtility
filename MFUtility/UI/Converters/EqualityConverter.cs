using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class EqualityConverter : IValueConverter
{
    private static readonly EqualityConverter _instance = new();
    
    public static EqualityConverter Instance => _instance;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => Equals(value, parameter);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? parameter : Binding.DoNothing;
}
