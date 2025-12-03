using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MFUtility.Helpers;

public static class VisualHelper {
	// ---------------------------
	// ❶ 查找最近的父级
	// ---------------------------
	public static T FindParent<T>(this DependencyObject child, bool includeSelf = false)
		where T : DependencyObject {
		if (child == null) return null;

		if (includeSelf && child is T self)
			return self;

		DependencyObject parent = VisualTreeHelper.GetParent(child);

		while (parent != null) {
			if (parent is T match)
				return match;

			parent = VisualTreeHelper.GetParent(parent);
		}

		return null;
	}


	// ---------------------------
	// ❷ 查找单一子级
	// ---------------------------
	public static T FindChild<T>(this DependencyObject parent, string childName = null)
		where T : FrameworkElement {
		if (parent == null) return null;

		int count = VisualTreeHelper.GetChildrenCount(parent);

		for (int i = 0; i < count; i++) {
			var child = VisualTreeHelper.GetChild(parent, i);

			if (child is T t && (string.IsNullOrEmpty(childName) || t.Name == childName))
				return t;

			var found = FindChild<T>(child, childName);
			if (found != null)
				return found;
		}

		return null;
	}


	// ---------------------------
	// ❸ 查找所有指定类型的子控件（非常强大）
	// ---------------------------
	public static IEnumerable<T> FindChildren<T>(this DependencyObject parent)
		where T : DependencyObject {
		if (parent == null) yield break;

		int count = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < count; i++) {
			var child = VisualTreeHelper.GetChild(parent, i);

			if (child is T t)
				yield return t;

			foreach (var sub in FindChildren<T>(child))
				yield return sub;
		}
	}


	// ---------------------------
	// ❹ 查找 DataTemplate / ControlTemplate 内部元素
	// ---------------------------
	public static T FindFromTemplate<T>(this FrameworkElement parent, string childName = null)
		where T : FrameworkElement {
		if (parent == null) return null;

		parent.ApplyTemplate(); // ⭐确保模板已加载

		return FindChild<T>(parent, childName);
	}


	// ---------------------------
	// ❺ 混合逻辑树查找（可选）
	// ---------------------------
	public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject parent)
		where T : DependencyObject {
		if (parent == null) yield break;

		foreach (var child in LogicalTreeHelper.GetChildren(parent)) {
			if (child is DependencyObject dep) {
				if (dep is T t)
					yield return t;

				foreach (var sub in FindLogicalChildren<T>(dep))
					yield return sub;
			}
		}
	}
}