using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Bus;
using MFUtility.Notifications.Enums;
using MFUtility.Notifications.Services;

namespace Test2;

public partial class MainViewModel : ObservableObject {
	public MainViewModel() {
		MessageBus.EnableRemote("127.0.0.1:5055"); // 或者 MessageBus.EnableIpc("5055");

		MessageBus.Subscribe<string>("hello2", s => ToastService.ShowError(s,2,NotifycationPosition.BottomCenter));
	}

	[RelayCommand]
	private void Test1() {
		MessageBus.Publish("hello1", "I am test2");
	}
}