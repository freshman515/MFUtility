using CommunityToolkit.Mvvm.ComponentModel;
using MFUtility.Mvvm.Wpf;

namespace Test2.ViewModels;

public partial class SettingsViewModel:ViewModelBase{
	public string Message { get; set; } = "Settings";
}