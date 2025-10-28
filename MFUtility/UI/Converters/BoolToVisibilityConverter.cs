using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MFUtility.UI.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Inverse { get; set; }
    public bool CollapseWhenFalse { get; set; } = true;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool val = value is bool b && b;
        if (Inverse) val = !val;

        if (val)
            return Visibility.Visible;
        return CollapseWhenFalse ? Visibility.Collapsed : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            bool result = v == Visibility.Visible;
            return Inverse ? !result : result;
        }
        return false;
    }
}
