
namespace MFUtility.UI.Helpers;

public class CornerHelper {
	public static CornerRadius GetCornerRadius(DependencyObject obj)
		=> (CornerRadius)obj.GetValue(CornerRadiusProperty);

	public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
		=> obj.SetValue(CornerRadiusProperty, value);

	public static readonly DependencyProperty CornerRadiusProperty =
		DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius),
			typeof(CornerHelper),
			new PropertyMetadata(default(CornerRadius), OnCornerRadiusChanged));

	private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is Border border)
			border.CornerRadius = (CornerRadius)e.NewValue;
		else if (d is Control ctrl)
			ctrl.Loaded += (_, _) => {
				if (ctrl.Template.FindName("border", ctrl) is Border border) {
					border.CornerRadius = (CornerRadius)e.NewValue;
				}
			};
	}
}