using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MFUtility.WPF.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToBrushConverter : IValueConverter
    {
        private static readonly BrushConverter _brushConverter = new();

        static BoolToBrushConverter()
        {
            Instance = new BoolToBrushConverter();
        }

        /// <summary>
        /// 全局静态实例，可在 XAML 中通过 x:Static 引用
        /// </summary>
        public static BoolToBrushConverter Instance { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = parameter?.ToString() ?? "#00D3F3,#FF5252";

            string[] colorHex = param.Split(',');

            string trueColorHex = colorHex.Length > 0 ? colorHex[0] : "#00D3F3";
            string falseColorHex = colorHex.Length > 1 ? colorHex[1] : "#FF5252";

            Brush trueBrush = (SolidColorBrush)_brushConverter.ConvertFrom(trueColorHex);
            Brush falseBrush = (SolidColorBrush)_brushConverter.ConvertFrom(falseColorHex);

            bool status = value switch
            {
                bool b => b,
                string s when bool.TryParse(s, out var p) => p,
                _ => false
            };

            return status ? trueBrush : falseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
