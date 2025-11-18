using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class PercentageConverter : IValueConverter
{
    public double Factor { get; set; } = 1.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            double factor = parameter is string s && double.TryParse(s, out var p) ? p : Factor;
            return $"{d * factor:P0}";
        }
        return "0%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
