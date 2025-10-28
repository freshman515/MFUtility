using System;
using System.Globalization;
using System.Windows.Data;

namespace MFUtility.UI.Converters;

public class NotNullToBoolConverter : IValueConverter
{
    public bool Inverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool result = value != null;
        return Inverse ? !result : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
