using System;
using System.Globalization;
using System.Windows.Data;

namespace MFUtility.UI.Converters;

public class MultiBoolAndConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.All(v => v is bool b && b);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
