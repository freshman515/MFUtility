using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class MultiBoolOrConverter : IMultiValueConverter
{
    private static readonly MultiBoolOrConverter _instance = new();
    
    public static MultiBoolOrConverter Instance => _instance;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.Any(v => v is bool b && b);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
