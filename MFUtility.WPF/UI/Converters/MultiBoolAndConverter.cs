using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class MultiBoolAndConverter : IMultiValueConverter
{
    private static readonly MultiBoolAndConverter _instance = new();
    
    public static MultiBoolAndConverter Instance => _instance;
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values.All(v => v is bool b && b);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
