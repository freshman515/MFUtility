using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class StringIsNullOrEmptyConverter : IValueConverter
{
    private static readonly StringIsNullOrEmptyConverter _instance = new();
    
    public static StringIsNullOrEmptyConverter Instance => _instance;

    public bool Inverse { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isEmpty = string.IsNullOrEmpty(value as string);
        return Inverse ? !isEmpty : isEmpty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
