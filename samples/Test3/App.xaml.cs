using System.Configuration;
using System.Data;
using System.Windows;
using MFUtility.Ioc.Core;
using MFUtility.Mvvm.Wpf.Extensions;

namespace Test3;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
	public static Container Container { get; set; }
	public App() {
		var container=Container = new Container();
		container.AutoRegisterViewModels();
		container.ResolveView<MainView>().Show();
	}
}