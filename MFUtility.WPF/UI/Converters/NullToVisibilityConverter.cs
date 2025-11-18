using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public bool Inverse { get; set; }
    public bool CollapseWhenHidden { get; set; } = true;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value == null;
        if (Inverse) isNull = !isNull;
        return isNull ? (CollapseWhenHidden ? Visibility.Collapsed : Visibility.Hidden) : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
