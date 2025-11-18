using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MFUtility.WPF.UI.Converters;

/// <summary>
/// 将字符串（文件路径、URL、Base64）转换为 ImageSource。
/// 支持本地路径、网络地址、Base64 图片。
/// </summary>
[ValueConversion(typeof(string), typeof(ImageSource))]
public class StringToImageConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value == null) return null;
		string str = value.ToString() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(str)) return null;

		try {
			//  1. Base64 编码图像
			if (str.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) || str.Length > 1000) {
				var base64Data = str.Contains(",") ? str.Split(',')[1] : str;
				var bytes = System.Convert.FromBase64String(base64Data);
				using var ms = new MemoryStream(bytes);
				var bmp = new BitmapImage();
				bmp.BeginInit();
				bmp.CacheOption = BitmapCacheOption.OnLoad;
				bmp.StreamSource = ms;
				bmp.EndInit();
				bmp.Freeze();
				return bmp;
			}

			//  2. 网络图片 (http/https)
			if (Uri.IsWellFormedUriString(str, UriKind.Absolute) &&
			    (str.StartsWith("http", StringComparison.OrdinalIgnoreCase))) {
				var bmp = new BitmapImage();
				bmp.BeginInit();
				bmp.UriSource = new Uri(str, UriKind.Absolute);
				bmp.CacheOption = BitmapCacheOption.OnDemand;
				bmp.EndInit();
				bmp.Freeze();
				return bmp;
			}

			//  3. 本地路径
			if (File.Exists(str)) {
				var bmp = new BitmapImage();
				bmp.BeginInit();
				bmp.UriSource = new Uri(Path.GetFullPath(str), UriKind.Absolute);
				bmp.CacheOption = BitmapCacheOption.OnLoad;
				bmp.EndInit();
				bmp.Freeze();
				return bmp;
			}

			return null;
		} catch {
			return null;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> Binding.DoNothing;
}