using System.Globalization;
using System.Windows.Data;

namespace MFUtility.UI.Converters;

public class BoolToStringConverter : IValueConverter {

	/// <summary>
	/// 将 bool 转换成字符串
	/// 参数格式: "TrueText,FalseText"
	/// 例如： "开,关" 或 "在线,离线"
	/// </summary>
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		// 默认文字
		string trueText = "True";
		string falseText = "False";

		// 如果传入了参数，比如 "开,关"
		if (parameter is string param) {
			var parts = param.Split(',');
			if (parts.Length > 0) trueText = parts[0];
			if (parts.Length > 1) falseText = parts[1];
		}

		// 解析布尔值
		bool status = false;
		if (value is bool b)
			status = b;
		else if (value is string s && bool.TryParse(s, out var parsed))
			status = parsed;

		return status ? trueText : falseText;
	}

	/// <summary>
	/// 如果你希望反向绑定（字符串 → bool），这里也可以实现。
	/// </summary>
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value == null)
			return false;

		string input = value.ToString();
		if (parameter is string param) {
			var parts = param.Split(',');
			if (parts.Length > 0 && input == parts[0])
				return true;
			if (parts.Length > 1 && input == parts[1])
				return false;
		}

		// 默认情况：字符串是 "True" / "False"
		return string.Equals(input, "True", StringComparison.OrdinalIgnoreCase);
	}
}