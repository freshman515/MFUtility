using System.Windows;
using MFUtility.Configuration;
using MFUtility.Mvvm.Wpf.Toolkit;
using Test3.Models;

namespace Test3.ViewModels;

public class MainViewModel : ViewModelBase {
	public MainViewModel() {
		var app = ConfigManager.Load<AppConfig>();
		app.Version = "12.0.0";
		app.Name = "App.Test";
		app.Student = new Student() {
			Name = "小明",
			Age = 18,
		};
		ConfigManager.Save(app);
		ConfigManager.Watch<AppConfig>((oldValue, newValue, changes) => {
			foreach (var change in changes) {
				MessageBox.Show(
					$"{change.Name} 发生变化：{change.OldValue} → {change.NewValue}");
			}
		});
		app = ConfigManager.Load<AppConfig>();
	}

	public async Task InitializeAsync() {


	}
}