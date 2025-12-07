using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Logging;
using MFUtility.Logging.Enums;
using MFUtility.Mvvm.Wpf;

namespace Test4.ViewModels;

public partial class MainViewModel :ViewModelBase {
	[ObservableProperty] private string hello;
	public MainViewModel() {
		
	}
	[RelayCommand]
	private void Log() {
		LogManager.Info("Test4");
	}
	[RelayCommand]
	private void SendEvent() {
	}
}