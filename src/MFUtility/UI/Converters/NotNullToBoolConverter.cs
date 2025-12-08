using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class NotNullToBoolConverter : IValueConverter
{
    private static readonly NotNullToBoolConverter _instance = new();
    
    public static NotNullToBoolConverter Instance => _instance;

    public bool Inverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool result = value != null;
        return Inverse ? !result : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
