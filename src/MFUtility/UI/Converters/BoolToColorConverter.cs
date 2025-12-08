using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace MFUtility.WPF.UI.Converters;

public class BoolToColorConverter : IValueConverter
{
    private static readonly BoolToColorConverter _instance = new();
    
    public static BoolToColorConverter Instance => _instance;

	public Brush TrueBrush { get; set; } = Brushes.Green;
	public Brush FalseBrush { get; set; } = Brushes.Gray;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> (value is bool b && b) ? TrueBrush : FalseBrush;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> Binding.DoNothing;
}