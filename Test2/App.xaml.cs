using System.Configuration;
using System.Data;
using System.Windows;
using MFUtility.Logging;
using MFUtility.Logging.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
	public static IHost? AppHost;
	public App() {
		AppHost = Host.CreateDefaultBuilder()
			.ConfigureServices((context, services) => {
				services.AddSingleton<MainWindow>();
				services.AddSingleton<MainViewModel>();

			})
			.Build();
	}
	protected override async void OnStartup(StartupEventArgs e) {
		await AppHost!.StartAsync();
		LogManager.Configure()
			.WriteTo(w => {
				w.Console();
				w.File(f => {
					f.MaxFileSizeMB(10)
						.UseAppFolder()
						.UseSolutionPath()
						.UseBasePath()
						.Async()
						.UseDateFolder();
				});
				w.JsonFile(j => j.InheritFromFile()
					           .Indented(true)
					           .UseJsonArrayFile()
				);
			})
			.Format(f => {
				f.Include(i => {
					i.ClassName()
						.Assembly()
						.LineNumber();
				});
				f.Style(s => {
					s.TimeFormat("yyyy-MM-dd HH:mm:ss")
						.UseFieldTag(false);
				});
				f.OrderDefault();
			})
			.Level(LogLevel.Debug)
			.Apply();
		var main = AppHost.Services.GetRequiredService<MainWindow>();
		main.DataContext = AppHost.Services.GetRequiredService<MainViewModel>();
		main.Show();
	}

	protected override async void OnExit(ExitEventArgs e) {
		await AppHost!.StopAsync();
		AppHost.Dispose();
		base.OnExit(e);
	}
}