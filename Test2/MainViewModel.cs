using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MFUtility.Core.Bus;
using MFUtility.Extensions;
using MFUtility.Notifications.Enums;
using MFUtility.Notifications.Services;

namespace Test2;

public partial class MainViewModel : ObservableObject {
	public MainViewModel() {
		Bus.EnableRemote("127.0.0.1:5055"); // 或者 MessageBus.EnableIpc("5055");

		Bus.Subscribe<string>("hello2", s => ToastService.ShowError(s, 2, NotifycationPosition.BottomCenter));
		Bus.Subscribe("hello", () => { });
		Bus.SubscribeEvent<ValueChange>(a=>a.ToString().Dump());
		Bus.Scope("hello").SubscribeEvent<ValueChange>(a => ToastService.Show(a.value.ToString()));
	}

	[RelayCommand]
	private void Test1() {
		Bus.Publish("hello2", "I am test2");
		Bus.Scope("hello").PublishEvent(new ValueChange(12.34));
	}
}

record ValueChange(double value);