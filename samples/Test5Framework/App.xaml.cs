using System.Windows;
using MFUtility.Ioc.Core;
using MFUtility.Ioc.Enums;
using MFUtility.Mvvm.Wpf.Framework.Extensions;
using Test5Framework.ViewModels;

namespace Test5Framework;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
	public static Container Container;
	public App() {
		
	}
	protected override void OnStartup(StartupEventArgs e) {
		base.OnStartup(e);
		var container = Container = new Container();
		container.AutoRegister(
			assembly: typeof(MainViewModel).Assembly,
			filter: t => t.Name.EndsWith("ViewModel")
			, Lifetime.Singleton);
		container.ResolveView<MainView>().Show();
	}
}