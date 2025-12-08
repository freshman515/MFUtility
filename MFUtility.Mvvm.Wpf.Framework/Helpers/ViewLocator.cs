using System;
using System.Linq;
using System.Windows;

namespace MFUtility.Mvvm.Wpf.Framework.Helpers;

public static class ViewLocator {
	public static FrameworkElement ResolveView(object viewModel) {

		var vmType = viewModel.GetType();
		var viewShortName = vmType.Name.Replace("ViewModel", "View");
		var viewType = AppDomain.CurrentDomain.GetAssemblies()
		                        .SelectMany(a => {
			                        try {
				                        return a.GetTypes();
			                        } catch (Exception e) {
				                        return Array.Empty<Type>();
			                        }
		                        })
		                        .FirstOrDefault(t => t.IsClass && !t.IsAbstract && typeof(FrameworkElement).IsAssignableFrom(t)
		                                             && t.Name == viewShortName);

		if (viewType == null)
			throw new InvalidOperationException(
				$"未找到与 ViewModel [{vmType.FullName}] 匹配的 View。\n" +
				$"请创建一个名为 {vmType.Name.Replace("ViewModel", "View")} 的视图类。");
		var view = (FrameworkElement)Activator.CreateInstance(viewType)!;
		view.DataContext = viewModel;
		return view;

	}
}