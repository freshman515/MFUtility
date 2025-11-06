using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MFUtility.UI.Converters
{
    /// <summary>
    /// 将字符串（例如 "#FF0000" 或 "Red"）转换为 Brush。
    /// 支持 #RGB、#ARGB、颜色名（如 "Blue"、"Transparent"）等格式。
    /// </summary>
    [ValueConversion(typeof(string), typeof(Brush))]
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Transparent;

            var str = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(str))
                return Brushes.Transparent;

            try
            {
                // 尝试将字符串解析为 Color
                var color = (Color)ColorConverter.ConvertFromString(str);
                return new SolidColorBrush(color);
            }
            catch
            {
                // 解析失败返回透明
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var c = brush.Color;
                return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
            }
            return null!;
        }
    }
}
