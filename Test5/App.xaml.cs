using System.Windows;
using MFUtility.Ioc.Core;
using MFUtility.Ioc.Enums;
using MFUtility.Mvvm.Wpf.Extensions;
using Test5.ViewModels;

namespace Test5 {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		public static Container Container { get; set; } 
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			var container = Container=new Container();
				container.AutoRegister(
			assembly: typeof(MainViewModel).Assembly,
			filter: t => t.Name.EndsWith("ViewModel")
			, Lifetime.Singleton);
			var main = container.ResolveView<MainView>();
			main.Show();
		}
	}
}