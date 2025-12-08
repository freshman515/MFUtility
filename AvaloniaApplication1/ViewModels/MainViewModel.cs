using System;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Logging;
using MFUtility.Mvvm.Avalonia;
using MFUtility.Mvvm.Avalonia.Services;

namespace AvaloniaApplication1.ViewModels;

public partial class MainViewModel : ViewModelBase {
	public string Greeting { get; } = "Welcome to Avalonia!";
	public MainViewModel() {
		LogManager.Configure()
		          .WriteTo(w =>
			                   w.Console()
			                    .File(f => f.UseAppFolder()
			                                .UseDateFolder()
			                                .UseBasePath()))
		          .Apply();
	}
	[RelayCommand]
	private void GoHome() {
		Navigator.Navigate<HomeViewModel>("Main");
		LogManager.Info("GoHome");
	}
	[RelayCommand]
	private void GoAbout() {
		Navigator.Navigate<AboutViewModel>("Main","hello world");
		LogManager.Info("GoAbout");
	}
	[RelayCommand]
	private void GoBack() {
		Navigator.GoBack("Main");
		LogManager.Info("GoBack");
		
	}
}