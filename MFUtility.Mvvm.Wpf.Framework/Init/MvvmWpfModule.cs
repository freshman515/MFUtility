using MFUtility.Ioc.Core;
using MFUtility.Ioc.Interfaces;
using MFUtility.Mvvm.Wpf.Interfaces;
using MFUtility.Mvvm.Wpf.Services;

namespace MFUtility.Mvvm.Wpf.Init;

public class MvvmWpfModule : IIocAutoModule {

	public void Load(Container container) {
		container.TryAddSingleton<INavigator, NavigationService>();
	}
}