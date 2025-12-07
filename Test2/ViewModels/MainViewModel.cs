using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.EventBus;
using MFUtility.Extensions;
using MFUtility.Ioc;
using MFUtility.Logging;
using MFUtility.Mvvm.Wpf;
using MFUtility.Notifications.Enums;
using MFUtility.Notifications.Services;
using MFUtility.WPF.Bases;
using Test2.Views;

namespace Test2.ViewModels;

public class User {
	public User(string name) { Name = name; }
	public string Name { get; set; }
}

public partial class MainViewModel : ViewModelBase {
	[ObservableProperty] private string _hello;
	public BindableCollection<User> Users { get; } = new();
	public MainViewModel() {
		EventBus.EnableRemote("127.0.0.1:5055"); // 或者 MessageBus.EnableIpc("5055");
		EventBus.Subscribe<string>("hello2", s => ToastService.ShowError(s, 2, NotifycationPosition.BottomCenter));
		EventBus.Subscribe("hello", () => { });
		EventBus.SubscribeEvent<ValueChange>(a => a.ToString().Dump());
		EventBus.Scope("hello")
		   .SubscribeEvent<ValueChange>(a => ToastService.Show(a.value.ToString()));

		Task.Run(() => {
			Users.Add(new User("ha"));
		});

		var service = IoC.Default?.Resolve<IUserService>();
		var config =  IoC.Default?.Resolve<Config>();
		LogManager.AddProvider(new DebugLogProvider());
		
		Load();

	}
	public void Load() {
		Users.AddRange(new[] {
			new User("Tom"),
			new User("Jack"),
			new User("Alice")
		});
	}

	[RelayCommand]
	private void Test1() {
		// Bus.Publish("hello2", "I am test2");
		// Bus.Scope("hello")
		// 	.PublishEvent(new ValueChange(12.34));
		LogManager.Info("Test3");
		try {
			throw new ArgumentException("hello using System;");
		} catch (Exception e) {
			LogManager.Error("这是一个错误", e);
		}
	}
	[RelayCommand]
	private void NavigateHome() {
		Navigator.Navigate<HomeViewModel>("Main",123);
	}
	[RelayCommand]
	private void NavigateSettings() {
		
		Navigator.Navigate<AboutViewModel>("Main");
	}
	[RelayCommand]
	private void GoBack() {
		Navigator.GoBack("Main");
	}
}

record ValueChange(double value);