using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;

// 如果不需要 ViewModelBase，可移除

namespace MFUtility.Mvvm.Avalonia.Managers;

public static class RegionManager {
	/// <summary>
	/// 保存已注册的 Region
	/// </summary>
	private static readonly Dictionary<string, ContentControl> _regions = new();

	/// <summary>
	/// 返回栈（支持 GoBack）
	/// </summary>
	internal static readonly Dictionary<string, Stack<ViewModelBase>> BackStacks = new();

	// 静态构造：订阅 AttachedProperty 的变化事件
	static RegionManager() {
		RegionManagerAttached.RegionNameProperty.Changed.Subscribe(OnRegionNameChanged);
	}

	#region Region 注册

	internal static void RegisterRegion(string name, ContentControl region) {
		_regions[name] = region;
	}

	/// <summary>
	/// 区域名改变时触发
	/// </summary>
	private static void OnRegionNameChanged(AvaloniaPropertyChangedEventArgs args) {
		if (args.Sender is ContentControl region &&
		    args.NewValue is string name &&
		    !string.IsNullOrWhiteSpace(name)) {
			_regions[name] = region;
		}
	}

	/// <summary>
	/// 获取 Region 控件
	/// </summary>
	public static ContentControl GetRegion(string name) {
		if (_regions.TryGetValue(name, out var region))
			return region;

		throw new Exception($"Region '{name}' not registered.");
	}

	#endregion

	#region BackStack（导航返回支持）

	public static void PushToBackStack(string region, ViewModelBase vm) {
		if (!BackStacks.ContainsKey(region))
			BackStacks[region] = new Stack<ViewModelBase>();

		BackStacks[region].Push(vm);
	}

	public static ViewModelBase? PopBackStack(string region) {
		if (BackStacks.TryGetValue(region, out var stack) && stack.Count > 0)
			return stack.Pop();

		return null;
	}

	public static bool CanGoBack(string region) {
		return BackStacks.TryGetValue(region, out var stack) && stack.Count > 0;
	}

	public static void ClearBackStack(string region) {
		if (BackStacks.ContainsKey(region))
			BackStacks[region].Clear();
	}

	#endregion
}

public class RegionManagerAttached {
	static RegionManagerAttached() {
		// ✔ 在类加载时订阅
		RegionNameProperty.Changed.Subscribe(OnRegionNameChanged);
	}

	public static readonly AttachedProperty<string?> RegionNameProperty =
		AvaloniaProperty.RegisterAttached<RegionManagerAttached, Control, string?>(
			"RegionName",
			defaultValue: null);

	public static void SetRegionName(AvaloniaObject obj, string? value)
		=> obj.SetValue(RegionNameProperty, value);

	public static string? GetRegionName(AvaloniaObject obj)
		=> obj.GetValue(RegionNameProperty);

	private static void OnRegionNameChanged(AvaloniaPropertyChangedEventArgs args) {
		if (args.Sender is ContentControl region &&
		    args.NewValue is string name &&
		    !string.IsNullOrWhiteSpace(name)) {
			RegionManager.RegisterRegion(name, region);
		}
	}
}