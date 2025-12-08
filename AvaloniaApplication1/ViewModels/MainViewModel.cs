using CommunityToolkit.Mvvm.Input;
using MFUtility.Mvvm.Avalonia;
using MFUtility.Mvvm.Avalonia.Services;

namespace AvaloniaApplication1.ViewModels;

public partial class MainViewModel : ViewModelBase {
	public string Greeting { get; } = "Welcome to Avalonia!";
	[RelayCommand]
	private void GoHome() {
		Navigator.Navigate<HomeViewModel>("Main");
	}
	[RelayCommand]
	private void GoAbout() {
		Navigator.Navigate<AboutViewModel>("Main");
	}
	[RelayCommand]
	private void GoBack() {
		Navigator.GoBack("Main");
	}
}