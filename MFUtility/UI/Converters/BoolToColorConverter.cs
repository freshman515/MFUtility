using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace MFUtility.UI.Converters;

public class BoolToColorConverter : IValueConverter {
	public Brush TrueBrush { get; set; } = Brushes.Green;
	public Brush FalseBrush { get; set; } = Brushes.Gray;

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> (value is bool b && b) ? TrueBrush : FalseBrush;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> Binding.DoNothing;
}