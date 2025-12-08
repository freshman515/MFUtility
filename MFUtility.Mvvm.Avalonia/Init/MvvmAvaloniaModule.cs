using MFUtility.Ioc.Core;
using MFUtility.Ioc.Interfaces;
using MFUtility.Mvvm.Avalonia.Interfaces;
using MFUtility.Mvvm.Avalonia.Services;

namespace MFUtility.Mvvm.Avalonia.Init;

public class MvvmAvaloniaModule : IIocAutoModule {

	public void Load(Container container) {
		container.TryAddSingleton<INavigator, NavigationService>();
	}
}