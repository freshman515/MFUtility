using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MFUtility.Mvvm.Wpf.Managers;

public static class RegionManager {
	private static readonly Dictionary<string, ContentControl> _regions = new();
	internal static readonly Dictionary<string, Stack<ViewModelBase>> BackStacks = new();
	public static void ClearBackStack(string region) {
		if (BackStacks.ContainsKey(region))
			BackStacks[region].Clear();
	}
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

	public static readonly DependencyProperty RegionNameProperty =
		DependencyProperty.RegisterAttached(
			"RegionName",
			typeof(string),
			typeof(RegionManager),
			new PropertyMetadata(null, OnRegionNameChanged));

	public static void SetRegionName(DependencyObject d, string value)
		=> d.SetValue(RegionNameProperty, value);

	public static string GetRegionName(DependencyObject d)
		=> (string)d.GetValue(RegionNameProperty);

	private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ContentControl region && e.NewValue is string name) {
			_regions[name] = region;
		}
	}

	public static ContentControl GetRegion(string name) {
		if (_regions.TryGetValue(name, out var region))
			return region;

		throw new Exception($"Region '{name}' not registered.");
	}
}