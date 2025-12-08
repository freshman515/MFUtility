using System.Windows;

namespace WpfApplication1 {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			new MainView().Show();
		}
	}
}