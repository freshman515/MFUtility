using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MFUtility.Bases;
using MFUtility.Extensions;
using MFUtility.Bus;
using MFUtility.Enums;
using MFUtility.Helpers;
using MFUtility.Notifications.Services;
using MFUtility.Views;
using MFUtility.Win32;
using Newtonsoft.Json.Linq;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

namespace Test1;

public partial class MainViewMode : ObservableObject {


	public MainViewMode() {
		Bus.EnableRemote("127.0.0.1:5055");
		Bus.Subscribe<string>("hello1", s => ToastService.Show(s));
		Bus.Subscribe<string>("hello3", s => ToastService.Show(s));
		Bus.Subscribe("hello3", (args) => ToastService.Show($"{args[0]}"));
	}


	public ICommand ShowCommand { get; } = new RelayCommand<string>((string str) => {

		Bus.PublishRemote("hello2", "I am test1");
		Bus.Publish("hello3", "I am self");
	});
	public ICommand ConnectCommand => new AsyncRelayCommand(async () => {

	});

}