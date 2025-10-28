using System;
using System.Globalization;
using System.Windows.Data;

namespace MFUtility.UI.Converters;

public class StringIsNullOrEmptyConverter : IValueConverter
{
    public bool Inverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isEmpty = string.IsNullOrEmpty(value as string);
        return Inverse ? !isEmpty : isEmpty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
