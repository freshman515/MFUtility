using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using MFUtility.Extensions;
using MFUtility.Mvvm.Wpf;

namespace Test2.ViewModels;

public partial class HomeViewModel : ViewModelBase{
	public string Message { get; set; } = "Home";
	[ObservableProperty] private string hello;
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
}