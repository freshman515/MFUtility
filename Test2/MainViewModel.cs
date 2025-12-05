using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Core.Bus;
using MFUtility.Extensions;
using MFUtility.Logging;
using MFUtility.Logging.Enums;
using MFUtility.Notifications.Enums;
using MFUtility.Notifications.Services;
using MFUtility.WPF.Bases;
using Microsoft.Extensions.Logging;
using Serilog;
using LogLevel = MFUtility.Logging.Enums.LogLevel;

namespace Test2;

public class User {
	public User(string name) { Name = name; }
	public string Name { get; set; }
}

public partial class MainViewModel : ObservableObject {
	public BindableCollection<User> Users { get; } = new();

	public MainViewModel() {
		Bus.EnableRemote("127.0.0.1:5055"); // 或者 MessageBus.EnableIpc("5055");
		Bus.Subscribe<string>("hello2", s => ToastService.ShowError(s, 2, NotifycationPosition.BottomCenter));
		Bus.Subscribe("hello", () => { });
		Bus.SubscribeEvent<ValueChange>(a => a.ToString()
			                                .Dump());
		Bus.Scope("hello")
			.SubscribeEvent<ValueChange>(a => ToastService.Show(a.value.ToString()));

		Task.Run(() => {
			Users.Add(new User("ha"));
		});


		Load();

	}
	public void Load() {
		Users.AddRange(new[] {
			new User("Tom"),
			new User("Jack"),
			new User("Alice")
		});
	}
	[ObservableProperty] private bool color = true;

	[RelayCommand]
	private void Test1() {
		Bus.Publish("hello2", "I am test2");
		Bus.Scope("hello")
			.PublishEvent(new ValueChange(12.34));

		LogManager.Info("Test2");
		LogManager.Info("Test3");
	}
}

record ValueChange(double value);