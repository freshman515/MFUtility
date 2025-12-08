using System.Windows;

namespace MFUtility.WPF.Bases {
	public class BindingProxy : Freezable {
		protected override Freezable CreateInstanceCore() {
			return new BindingProxy();
		}

		public object Value {
			get => GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
				nameof(Value),
				typeof(object),
				typeof(BindingProxy),
				new UIPropertyMetadata(null));
	}
}