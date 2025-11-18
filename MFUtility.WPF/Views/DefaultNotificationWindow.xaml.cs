using System.Windows;
using System.Windows.Media;
using MFUtility.Notifications.Enums;
using MFUtility.Notifications.Interfaces;
using MFUtility.WPF.Helpers;

namespace MFUtility.WPF.Views;

public partial class DefaultNotificationWindow : Window, INotificationDialog {
	public DefaultNotificationWindow() {
		InitializeComponent();
		Loaded += OnLoaded;
	}

	private void Button_Click(object sender, RoutedEventArgs e) => Close();

	private void OnLoaded(object sender, RoutedEventArgs e) {
		var iconDict = new ResourceDictionary {
			Source = new Uri("pack://application:,,,/MFUtility;component/Assets/Svgs/Icons.xaml", UriKind.Absolute)
		};

		var successIcon = iconDict["SuccessIcon"] as ImageSource;
		var infoIcon = iconDict["InfoIcon"] as ImageSource;
		var warningIcon = iconDict["HintIcon"] as ImageSource;
		var errorIcon = iconDict["ErrorIcon"] as ImageSource;

		switch (Type) {
			case NotificationType.Success:
				Icon = successIcon;
				BorderBrush = new SolidColorBrush(Color.FromRgb(60, 200, 110));
				break;
			case NotificationType.Warning:
				Icon = warningIcon;
				BorderBrush = new SolidColorBrush(Color.FromRgb(255, 170, 30));
				break;
			case NotificationType.Error:
				Icon = errorIcon;
				BorderBrush = new SolidColorBrush(Color.FromRgb(255, 70, 70));
				break;
			default:
				Icon = infoIcon;
				BorderBrush = new SolidColorBrush(Color.FromRgb(80, 160, 255));
				break;


		}
		if (!EnableBorderColor) {
			BorderBrush = ResourceHelper.CreateBrush("#43474A");
		}
	}

	// ============ 依赖属性 ============
	public static readonly DependencyProperty IconProperty =
		DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(DefaultNotificationWindow));
	public ImageSource Icon {
		get => (ImageSource)GetValue(IconProperty);
		set => SetValue(IconProperty, value);
	}

	public static readonly DependencyProperty BorderBrushProperty =
		DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(DefaultNotificationWindow));
	public Brush BorderBrush {
		get => (Brush)GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	// ============ 通用属性 ============
	public string MessageTitle { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public NotificationType Type { get; set; }

	/// <summary>是否启用边框颜色（仅当前窗口）</summary>
	public bool EnableBorderColor { get; set; } = true;
}