using MFUtility.Ioc.Core;
using MFUtility.Ioc.Interfaces;
using MFUtility.Mvvm.Wpf.Services;
using MFUtility.Mvvm.Wpf.ToolKit.Interfaces;

namespace MFUtility.Mvvm.Wpf.Toolkit.Init;

public class MvvmWpfModule : IIocAutoModule {

	public void Load(Container container) {
		container.TryAddSingleton<INavigator, NavigationService>();
	}
}