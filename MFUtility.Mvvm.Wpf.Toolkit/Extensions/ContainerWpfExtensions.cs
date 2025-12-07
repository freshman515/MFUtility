using System;
using System.Collections.Generic;
using System.Windows;
using MFUtility.Ioc;
using MFUtility.Ioc.Core;
using MFUtility.Ioc.Enums;
using MFUtility.Mvvm.Wpf.Helpers;
using MFUtility.Mvvm.Wpf.Interfaces;
using MFUtility.Mvvm.Wpf.Services;

namespace MFUtility.Mvvm.Wpf.Extensions;

public static class ContainerWpfExtensions {
	private static readonly Dictionary<Type, Type> _viewMap = new();
	public static void RegisterView<TView, TViewModel>(this Container container, Lifetime lifetime = Lifetime.Transient)
		where TView : FrameworkElement {
		container.Register<TView, TView>(lifetime); // View 自身
		container.Register<TViewModel, TViewModel>(lifetime); // VM 自身

		_viewMap[typeof(TView)] = typeof(TViewModel);
	}
	public static void RegisterView<TView>(this Container container)
		where TView : FrameworkElement {
		// 自动找到 ViewModel 的类型
		var vmType = ViewModelLocator.FindViewModelForView(typeof(TView));

		if (vmType == null)
			throw new Exception($"Cannot find ViewModel for view {typeof(TView).Name}");

		// 自动注册 View → View
		container.Register<TView>();

		// 自动注册 ViewModel → ViewModel
		container.Register(vmType);

		// 存储映射（可选）
		_viewMap[typeof(TView)] = vmType;
	}
	public static TView ResolveView<TView>(this Container container, Lifetime lifetime = Lifetime.Singleton)
		where TView : FrameworkElement {
		var viewType = typeof(TView);
		var view = container.Resolve<TView>();

		// 1. 优先使用手动注册的映射
		if (_viewMap.TryGetValue(viewType, out var vmType)) {
			view.DataContext = container.Resolve(vmType, lifetime);
			return view;
		}

		// 2. 否则 fallback 自动 ViewModel 定位
		var autoVM = ViewModelLocator.FindViewModelForView(viewType);
		if (autoVM != null) {
			// 自动注册（如果需要)
			container.TryRegisterIfNotExists(autoVM, lifetime);
			view.DataContext = container.Resolve(autoVM, lifetime);
			return view;
		}
		return view;
	}
	public static FrameworkElement ResolveView(this Container container, Type viewType) {
		var method = typeof(ContainerWpfExtensions)
		             .GetMethod("ResolveView")
		             .MakeGenericMethod(viewType);
		return (FrameworkElement)method.Invoke(null, new object[] { container })!;
	}

}