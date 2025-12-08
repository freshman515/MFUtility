using System.Windows;

namespace MFUtility.WPF.UI;

public class BindingProxy:Freezable {
	public object Data {
		get => (object)GetValue(DataProperty);
		set => SetValue(DataProperty, value);
	}

	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy));

	protected override Freezable CreateInstanceCore()=>new BindingProxy();
}