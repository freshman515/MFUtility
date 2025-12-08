using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OxyPlot;
using FontWeights = System.Windows.FontWeights;

namespace MFUtility.WPF.Helpers;

/// <summary>
/// 🎨 全能 WPF 资源助手
/// 提供：
/// </summary>
public static class ResourceHelper {
	#region 🧩 通用资源查找

	/// <summary>
	/// 判断资源是否存在（包括合并字典）
	/// </summary>
	public static bool ResourceExists(string resourceKey) =>
		Application.Current.Resources.Contains(resourceKey)
		|| FindResourceRecursive(resourceKey) != null;

	/// <summary>
	/// 从资源中递归查找指定 Key
	/// </summary>
	public static object? FindResourceRecursive(string key) =>
		FindResourceRecursive(Application.Current.Resources, key);

	private static object? FindResourceRecursive(ResourceDictionary dict, string key) {
		if (dict.Contains(key))
			return dict[key];

		foreach (var md in dict.MergedDictionaries) {
			var result = FindResourceRecursive(md, key);
			if (result != null)
				return result;
		}

		return null;
	}

	#endregion

	#region 🎨 颜色与画刷

	/// <summary>
	/// 根据十六进制颜色生成 Color
	/// </summary>
	public static Color FromHex(string hex, Color? fallback = null) {
		try {
			return (Color)ColorConverter.ConvertFromString(hex)!;
		} catch {
			return fallback ?? Colors.Transparent;
		}
	}

	/// <summary>
	/// 由 Color 创建 SolidColorBrush
	/// </summary>
	public static SolidColorBrush CreateBrush(Color color, bool freeze = true) {
		var brush = new SolidColorBrush(color);
		if (freeze) brush.Freeze();
		return brush;
	}

	/// <summary>
	/// 由十六进制字符串创建 SolidColorBrush
	/// </summary>
	public static SolidColorBrush CreateBrush(string hex, bool freeze = true) =>
		CreateBrush(FromHex(hex), freeze);

	/// <summary>
	/// 生成线性渐变画刷
	/// </summary>
	public static LinearGradientBrush CreateGradient(params (Color color, double offset)[] stops) {
		var gradient = new LinearGradientBrush();
		foreach (var (color, offset) in stops)
			gradient.GradientStops.Add(new GradientStop(color, offset));
		gradient.Freeze();
		return gradient;
	}

	/// <summary>
	/// 更新或添加全局 SolidColorBrush 资源
	/// </summary>
	public static void SetGlobalBrush(string resourceKey, string hexColor) {
		if (string.IsNullOrWhiteSpace(resourceKey) || string.IsNullOrWhiteSpace(hexColor))
			return;

		var brush = CreateBrush(FromHex(hexColor));
		if (Application.Current.Resources.Contains(resourceKey))
			Application.Current.Resources[resourceKey] = brush;
		else
			Application.Current.Resources.Add(resourceKey, brush);
	}

	/// <summary>
	/// 获取 SolidColorBrush 资源，找不到则返回 fallback
	/// </summary>
	public static SolidColorBrush GetBrush(string resourceKey, SolidColorBrush? fallback = null) {
		if (Application.Current.TryFindResource(resourceKey) is SolidColorBrush brush)
			return brush;
		return fallback ?? Brushes.Transparent;
	}

	/// <summary>
	/// 获取颜色资源（支持 Solid / Linear）
	/// </summary>
	public static Color GetColor(string resourceKey, Color fallback) {
		var res = FindResourceRecursive(Application.Current.Resources, resourceKey);
		return res switch {
			SolidColorBrush solid => solid.Color,
			LinearGradientBrush grad => grad.GradientStops.FirstOrDefault()?.Color ?? fallback,
			_ => fallback
		};
	}

	/// <summary>
	/// 混合两种颜色
	/// </summary>
	public static Color Blend(Color a, Color b, double ratio) {
		byte Lerp(byte x, byte y) => (byte)(x * (1 - ratio) + y * ratio);
		return Color.FromArgb(Lerp(a.A, b.A), Lerp(a.R, b.R), Lerp(a.G, b.G), Lerp(a.B, b.B));
	}

	/// <summary>
	/// 反转颜色
	/// </summary>
	public static Color Invert(Color color) =>
		Color.FromArgb(color.A, (byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));

	/// <summary>
	/// 设置透明度
	/// </summary>
	public static Color WithOpacity(Color color, double opacity) =>
		Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B);

	public static SolidColorBrush WithOpacity(SolidColorBrush brush, double opacity) =>
		new SolidColorBrush(WithOpacity(brush.Color, opacity));

	/// <summary>
	/// 获取所有全局颜色资源（键名 + 颜色）
	/// </summary>
	public static IEnumerable<(string key, Color color)> GetAllGlobalColors() {
		foreach (var key in Application.Current.Resources.Keys) {
			if (Application.Current.Resources[key] is SolidColorBrush brush)
				yield return (key.ToString()!, brush.Color);
		}
	}

	#endregion

	#region 🧠 OxyPlot 颜色桥接

	public static OxyColor ToOxyColor(Color color) =>
		OxyColor.FromArgb(color.A, color.R, color.G, color.B);

	public static OxyColor ToOxyColor(string hex) =>
		ToOxyColor(FromHex(hex));

	public static OxyColor GetOxyColor(string resourceKey) =>
		ToOxyColor(GetColor(resourceKey, Colors.Transparent));

	public static LinearGradientBrush ToGradient(OxyColor start, OxyColor end) =>
		new LinearGradientBrush(
			Color.FromArgb(start.A, start.R, start.G, start.B),
			Color.FromArgb(end.A, end.R, end.G, end.B),
			new Point(0, 0), new Point(1, 1));

	#endregion

	#region 🖼 图像与图标

	public static DrawingImage? GetVectorImage(string resourceKey) =>
		Application.Current.FindResource(resourceKey) as DrawingImage;

	public static ImageSource? GetImage(string resourceKey) =>
		FindResourceRecursive(Application.Current.Resources, resourceKey) as ImageSource;

	public static ImageSource LoadImage(string path) =>
		new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));

	#endregion

	#region 🅰️ 字体资源

	public static string GetFontName(string key, string fallback = "Segoe UI") =>
		FindResourceRecursive(Application.Current.Resources, key) is FontFamily font
			? font.Source
			: fallback;

	public static Typeface GetFont(string key, string fallback = "Segoe UI") {
		var font = FindResourceRecursive(Application.Current.Resources, key) as FontFamily
		           ?? new FontFamily(fallback);
		return new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
	}

	#endregion
}