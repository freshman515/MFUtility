using System;
using System.Windows;
using MFUtility.Ioc;
using MFUtility.Ioc.Enums;
using MFUtility.Ioc.Interfaces;
using MFUtility.Mvvm.Wpf.Helpers;
using MFUtility.Mvvm.Wpf.Managers;
using MFUtility.Mvvm.Wpf.Toolkit;
using MFUtility.Mvvm.Wpf.ToolKit.Interfaces;

namespace MFUtility.Mvvm.Wpf.Services;

public class NavigationService : INavigator {
	private readonly IResolver _resolver;

	public NavigationService(IResolver resolver) {
		_resolver = resolver;
	}

	// ================================
	// 基础导航（泛型）
	// ================================
	public void Navigate<TViewModel>(string region, object? parameter = null, Lifetime lifetime = Lifetime.Singleton) {
		Navigate(typeof(TViewModel), region, parameter, lifetime);
	}
	public void Navigate(string viewModelTypeString, string region, object? parameter = null, Lifetime lifetime = Lifetime.Singleton) {
		var type = ResolveViewModel(viewModelTypeString);
		Navigate(type, region, parameter, lifetime);
	}
	public static Type? ResolveViewModel(string name) {
		if (!name.EndsWith("ViewModel"))
			name += "ViewModel";

		// 查找当前 AppDomain 所有可用类型
		return AppDomain.CurrentDomain
		                .GetAssemblies()
		                .SelectMany(a => a.GetTypes())
		                .FirstOrDefault(t => t.Name == name);
	}
	public bool TryNavigate(string viewModelTypeString, string region, object? parameter = null) {
		try {
			Navigate(viewModelTypeString, region, parameter);
			return true;
		} catch {
			return false;
		}
	}

	// ================================
	// 基础导航（Type 版本）
	// ================================
	public void Navigate(Type viewModelType, string region, object? parameter = null, Lifetime lifetime = Lifetime.Singleton) {
		var regionControl = RegionManager.GetRegion(region);

		ViewModelBase? oldVm = null;

		// =====================================================
		// 0. 如果当前 VM 就是目标 VM，则不导航（避免重复刷新页面）
		// =====================================================
		if (regionControl.Content is FrameworkElement currentView &&
		    currentView.DataContext?.GetType() == viewModelType) {
			return;
		}
		// ===============================
		// 1. 旧 VM：即将离开（可阻止导航）
		// ===============================
		if (regionControl.Content is FrameworkElement oldView &&
		    oldView.DataContext is ViewModelBase prevVm) {
			if (!prevVm.OnNavigatingFrom())
				return; // ❗ 阻止导航

			oldVm = prevVm;
		}

		// ===============================
		// 2. 创建新 VM（自动注册 + 解析）
		// ===============================
		_resolver.TryRegisterIfNotExists(viewModelType, lifetime);
		var vm = (ViewModelBase)_resolver.Resolve(viewModelType);

		// ===============================
		// 3. 新 VM：即将进入
		// ===============================
		vm.OnNavigatingTo(parameter);

		// ===============================
		// 4. 创建 View
		// ===============================
		var view = ViewLocator.ResolveView(vm);

		// ===============================
		// 5. 切换 UI 内容（页面真正离开/进入）
		// ===============================
		regionControl.Content = view;

		// ===============================
		// 6. 旧 VM：已经离开
		// ===============================
		if (oldVm != null) {
			oldVm.IsActive = false;
			oldVm.OnNavigatedFrom();
			oldVm.OnDeactivated();
			RegionManager.PushToBackStack(region, oldVm);
		}

		// ===============================
		// 7. 设置新 VM 信息，并触发“已进入”
		// ===============================
		vm.NavigationParameter = parameter;
		vm.PreviousViewModel = oldVm;
		vm.RegionName = region;
		vm.IsActive = true;
		vm.AttachView(view);

		vm.OnNavigatedTo(parameter);
		vm.OnActivated();
	}
	// ================================
	// TryNavigate
	// ================================
	public bool TryNavigate<TViewModel>(string region, object? parameter = null) {
		try {
			Navigate<TViewModel>(region, parameter);
			return true;
		} catch {
			return false;
		}
	}
	// ================================
	// CanGoBack
	// ================================
	public bool CanGoBack(string region) {
		return RegionManager.BackStacks.ContainsKey(region)
		       && RegionManager.BackStacks[region].Count > 0;
	}

	// ================================
	// GoBack
	// ================================
	public void GoBack(string region) {
		if (!RegionManager.CanGoBack(region))
			return;

		var regionControl = RegionManager.GetRegion(region);

		// 当前页面离开
		if (regionControl.Content is FrameworkElement oldView &&
		    oldView.DataContext is ViewModelBase oldVm) {
			oldVm.OnNavigatingFrom();
			oldVm.OnNavigatedFrom();
			oldVm.IsActive = false;
			oldVm.OnDeactivated(); // ⭐ 新增
		}

		// 从 BackStack 弹出前一个 VM
		var vm = RegionManager.PopBackStack(region);
		if (vm == null)
			return;

		vm.OnNavigatingTo(vm.NavigationParameter);

		var view = ViewLocator.ResolveView(vm);
		regionControl.Content = view;

		vm.RegionName = region;
		vm.IsActive = true;
		vm.AttachView(view);

		vm.OnNavigatedTo(vm.NavigationParameter);
		vm.OnActivated();
	}
	// ================================
	// Clear（清空区域）
	// ================================
	public void Clear(string region) {
		var regionControl = RegionManager.GetRegion(region);

		if (regionControl.Content is FrameworkElement oldView &&
		    oldView.DataContext is ViewModelBase oldVm) {
			oldVm.OnNavigatedFrom();
			oldVm.IsActive = false;
		}

		regionControl.Content = null;

		if (RegionManager.BackStacks.ContainsKey(region))
			RegionManager.BackStacks[region].Clear();
	}
	// ================================
	// Close（关闭当前 VM）
	// ================================
	public void Close(string region) {
		var regionControl = RegionManager.GetRegion(region);

		if (regionControl.Content is FrameworkElement oldView &&
		    oldView.DataContext is ViewModelBase oldVm) {
			oldVm.OnNavigatedFrom();
			oldVm.IsActive = false;
		}

		regionControl.Content = null;
	}
	// ================================
	// GetCurrent（非泛型）
	// ================================
	public object? GetCurrent(string region) {
		var regionControl = RegionManager.GetRegion(region);
		return regionControl.Content is FrameworkElement view
			? view.DataContext
			: null;
	}
	// ================================
	// GetCurrent（泛型）
	// ================================
	public TViewModel? GetCurrent<TViewModel>(string region) where TViewModel : class {
		return GetCurrent(region) as TViewModel;
	}


}