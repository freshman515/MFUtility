using System.Windows;
using System.Windows.Documents;
using MFUtility.Ioc.Core;
using MFUtility.Ioc.Enums;
using MFUtility.Mvvm.Wpf.Framework.Extensions;
using Test5.ViewModels;

namespace Test5 {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		public static Container Container { get; set; }
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			var container = Container = new Container();
			container.AutoRegisterViewModels(typeof(App).Assembly);
			var main = container.ResolveView<MainView>();
			main.Show();
		}
	}
}