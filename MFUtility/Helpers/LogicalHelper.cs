using System.Collections.Generic;
using System.Windows;

namespace MFUtility.Helpers;

public static class LogicalHelper {
	// ---------------------------
	// ❶ 查找最近的逻辑父级
	// ---------------------------
	public static T FindParent<T>(this DependencyObject child, bool includeSelf = false)
		where T : DependencyObject {
		if (child == null) return null;

		if (includeSelf && child is T self)
			return self;

		DependencyObject parent = LogicalTreeHelper.GetParent(child);

		while (parent != null) {
			if (parent is T match)
				return match;

			parent = LogicalTreeHelper.GetParent(parent);
		}

		return null;
	}


	// ---------------------------
	// ❷ 查找单个逻辑子级
	// ---------------------------
	public static T FindChild<T>(this DependencyObject parent, string childName = null)
		where T : FrameworkElement {
		if (parent == null) return null;

		foreach (var obj in LogicalTreeHelper.GetChildren(parent)) {
			if (obj is DependencyObject child) {
				// 类型匹配，且名称匹配（如果有要求）
				if (child is T t && (string.IsNullOrEmpty(childName) || t.Name == childName))
					return t;

				var found = FindChild<T>(child, childName);
				if (found != null)
					return found;
			}
		}

		return null;
	}


	// ---------------------------
	// ❸ 查找所有逻辑子级
	// ---------------------------
	public static IEnumerable<T> FindChildren<T>(this DependencyObject parent)
		where T : DependencyObject {
		if (parent == null) yield break;

		foreach (var obj in LogicalTreeHelper.GetChildren(parent)) {
			if (obj is DependencyObject child) {
				if (child is T t)
					yield return t;

				foreach (var sub in FindChildren<T>(child))
					yield return sub;
			}
		}
	}
}