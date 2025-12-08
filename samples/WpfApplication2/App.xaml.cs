using System.Windows;
using MFUtility.Ioc;
using MFUtility.Ioc.Core;
using MFUtility.Ioc.Enums;
using MFUtility.Mvvm.Wpf.Framework.Extensions;

namespace WpfApplication2 {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			var container = IoC.Default ;
			container.AutoRegisterServices(lifetime:Lifetime.Transient);
			container.ResolveView<MainView>().Show();
		}
	}
}