using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MFUtility.WPF.UI.Converters;

public class BoolToBrushConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (parameter == null)
			return Brushes.Gray;

		string[] colorHex = parameter.ToString().Split(',');
		string trueColorHex = colorHex.Length > 0 ? colorHex[0] : "#00D3F3";
		string falseColorHex = colorHex.Length > 1 ? colorHex[1] : "#FF5252";

		Brush trueBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(trueColorHex);
		Brush falseBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(falseColorHex);

		bool status = false;
		if (value is bool b)
			status = b;
		else if (value is string s && bool.TryParse(s, out bool parsed))
			status = parsed;

		return status ? trueBrush : falseBrush;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}