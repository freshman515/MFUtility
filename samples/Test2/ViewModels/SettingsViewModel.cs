using CommunityToolkit.Mvvm.ComponentModel;
using MFUtility.Mvvm.Wpf;
using MFUtility.Mvvm.Wpf.Toolkit;

namespace Test2.ViewModels;

public partial class SettingsViewModel:ViewModelBase{
	public string Message { get; set; } = "Settings";
}