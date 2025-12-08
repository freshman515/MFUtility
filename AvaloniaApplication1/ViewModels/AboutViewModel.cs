using MFUtility.Bus.Event;
using MFUtility.Mvvm.Avalonia;

namespace AvaloniaApplication1.ViewModels;

public class AboutViewModel : ViewModelBase {
	public override void OnNavigatedTo(object? parameter) {
		base.OnNavigatedTo(parameter);
		EventBus.Publish("Home", parameter);
	}

}