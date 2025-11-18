using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class MultiBoolOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.Any(v => v is bool b && b);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
