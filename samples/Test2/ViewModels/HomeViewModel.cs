using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Ioc.Attributes;
using MFUtility.Mvvm.Wpf;
using MFUtility.Mvvm.Wpf.Toolkit;

namespace Test2.ViewModels;

public partial class HomeViewModel : ViewModelBase{
	public string Message { get; set; } = "Home";
	[ObservableProperty] private string hello;
	[Inject]
	public IConfigService ConfigService { get; set; }
	// public override void OnNavigatedFrom() {
	// 	//MessageBox.Show("Leave HomeViewModel");
	// 	base.OnNavigatedFrom();
	// }
	// public override void OnNavigatedTo(object? parameter) {
	// 	 ShowMessage($"To HomeViewModel|{parameter.ToInt()}|{RegionName}|{DisplayName}");
	// 	var vm = PreviousViewModel;
	// 	var obj = GetCurrentViewModel("Main");
	// 	var param = NavigationParameter;
	// 	base.OnNavigatedTo(parameter);
	// 	
	// }
	[RelayCommand]
	private void GoHome() {
		
	}
	public override void OnActivated() {
		base.OnActivated();
		
	}
}