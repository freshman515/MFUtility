using System.Windows;
using MFUtility.Ioc.Core;
using MFUtility.Ioc.Enums;
using MFUtility.Mvvm.Wpf.Extensions;
using Test2.ViewModels;
using Test2.Views;

namespace Test2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
	public static Container Container { get; set; }
	protected override async void OnStartup(StartupEventArgs e) {
		var container = new Container();
		Container = container;
		container.AddSingleton(new Config { Name = "Hello" });
		container.AutoRegister(
			assembly: typeof(MainViewModel).Assembly,
			filter: t => t.Name.EndsWith("ViewModel")
			, Lifetime.Singleton);
		container.AutoRegister(
			assembly: typeof(MainViewModel).Assembly,
			filter: t => t.Name.EndsWith("Service") || t.Name.EndsWith("Repository"),
			Lifetime.Singleton, registerInterfaces: true);
		
		var main = container.ResolveView<MainView>();
		main.Show();
	}

	protected override async void OnExit(ExitEventArgs e) {
	}
}