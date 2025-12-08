using MFUtility.Ioc.Core;
using MFUtility.Ioc.Interfaces;
using MFUtility.Mvvm.Wpf.Framework.Interfaces;
using MFUtility.Mvvm.Wpf.Framework.Services;

namespace MFUtility.Mvvm.Wpf.Framework.Init;

public class MvvmWpfModule : IIocAutoModule {

	public void Load(Container container) {
		container.TryAddSingleton<INavigator, NavigationService>();
	}
}