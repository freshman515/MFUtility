using System;
using MFUtility.Bus.Event;
using MFUtility.Mvvm.Avalonia;

namespace AvaloniaApplication1.ViewModels;

public class HomeViewModel : ViewModelBase {
	public override void OnNavigatedTo(object? parameter) {
		base.OnNavigatedTo(parameter);

	}
	public override void OnFirstActivated() {
		base.OnFirstActivated();
		
		EventBus.Subscribe("Home", args => Console.WriteLine(args[0]));
	}
}