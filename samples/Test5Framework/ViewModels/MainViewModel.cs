using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Mvvm.Wpf;
using MFUtility.Mvvm.Wpf.Framework;

namespace Test5Framework.ViewModels;

public partial class MainViewModel : ViewModelBase {
	public MainViewModel() {
	}

	[RelayCommand]
	void GoHome() {
		Navigator.Navigate<HomeViewModel>("Main");
	}
	[RelayCommand]
	void GoSettings() {
		Navigator.Navigate<SettingsViewModel>("Main");
	}

}