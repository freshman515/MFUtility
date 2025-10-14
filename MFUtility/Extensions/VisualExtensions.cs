using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MFUtility.Extensions;

/// <summary>
/// 🔹 WPF 视觉树工具类
/// 提供 VisualTree 和 LogicalTree 的便捷扩展：
/// - 查找父级、子级、兄弟节点（支持泛型与动态 Type）
/// - 模板内元素查找
/// - 坐标转换与命中测试
/// - 焦点控制与调试辅助
/// </summary>
public static class VisualExtensions
{
    #region 🧭 泛型父子节点查找

    /// <summary>递归查找父级元素（泛型）</summary>
    public static T? FindParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }

    /// <summary>递归查找子元素（泛型）</summary>
    public static T? FindChild<T>(this DependencyObject parent, string? name = null) where T : FrameworkElement
    {
        if (parent == null) return null;
        int count = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T tChild && (string.IsNullOrEmpty(name) || tChild.Name == name))
                return tChild;

            var found = FindChild<T>(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <summary>递归查找所有子元素（泛型）</summary>
    public static IEnumerable<T> FindChildren<T>(this DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T tChild)
                yield return tChild;

            foreach (var sub in FindChildren<T>(child))
                yield return sub;
        }
    }

    #endregion

    #region 🧭 非泛型父子节点查找（Type 动态版）

    /// <summary>递归查找指定类型的父级元素（非泛型）</summary>
    public static DependencyObject? FindParent(this DependencyObject child, Type targetType)
    {
        if (child == null || targetType == null) return null;

        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null && !targetType.IsInstanceOfType(parent))
            parent = VisualTreeHelper.GetParent(parent);
        return parent;
    }

    /// <summary>递归查找指定类型的子元素（非泛型）</summary>
    public static DependencyObject? FindChild(this DependencyObject parent, Type targetType, string? name = null)
    {
        if (parent == null || targetType == null) return null;
        int count = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (targetType.IsInstanceOfType(child))
            {
                if (child is FrameworkElement fe)
                {
                    if (string.IsNullOrEmpty(name) || fe.Name == name)
                        return child;
                }
                else
                    return child;
            }

            var found = FindChild(child, targetType, name);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>递归查找所有指定类型的子元素（非泛型）</summary>
    public static IEnumerable<DependencyObject> FindChildren(this DependencyObject parent, Type targetType)
    {
        if (parent == null || targetType == null) yield break;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (targetType.IsInstanceOfType(child))
                yield return child;

            foreach (var sub in FindChildren(child, targetType))
                yield return sub;
        }
    }

    #endregion




    #region 🎯 坐标与命中测试

    /// <summary>将相对坐标转换为屏幕坐标</summary>
    public static Point ToScreenPoint(this UIElement element, Point relativePoint)
    {
        var source = PresentationSource.FromVisual(element);
        if (source?.CompositionTarget == null) return new Point();
        var transform = element.TransformToAncestor((Visual)source.RootVisual);
        var point = transform.Transform(relativePoint);
        return ((Visual)source.RootVisual).PointToScreen(point);
    }

    /// <summary>判断屏幕坐标是否在元素范围内</summary>
    public static bool IsPointInside(this UIElement element, Point screenPoint)
    {
        var relative = element.PointFromScreen(screenPoint);
        return relative.X >= 0 && relative.X <= element.RenderSize.Width &&
               relative.Y >= 0 && relative.Y <= element.RenderSize.Height;
    }

    /// <summary>命中测试：获取鼠标下的元素</summary>
    public static DependencyObject? GetElementAtMouse()
    {
        var pos = Mouse.GetPosition(Application.Current.MainWindow);
        return VisualTreeHelper.HitTest(Application.Current.MainWindow, pos)?.VisualHit;
    }

    #endregion

    #region 👁️ 可见性与焦点

    /// <summary>判断元素是否在可见树中（包括父级）</summary>
    public static bool IsVisibleInTree(this FrameworkElement element)
    {
        if (element == null || !element.IsVisible)
            return false;

        var parent = element.FindParent<FrameworkElement>();
        while (parent != null)
        {
            if (!parent.IsVisible)
                return false;
            parent = parent.FindParent<FrameworkElement>();
        }
        return true;
    }

    /// <summary>安全聚焦元素</summary>
    public static bool TryFocus(this UIElement element)
    {
        if (element == null) return false;
        if (!element.Focusable) element.Focusable = true;
        return element.Focus();
    }

    #endregion

    #region 🧱 工具辅助

    /// <summary>获取所有祖先元素</summary>
    public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject element)
    {
        var current = VisualTreeHelper.GetParent(element);
        while (current != null)
        {
            yield return current;
            current = VisualTreeHelper.GetParent(current);
        }
    }

    /// <summary>获取元素的层级路径（Window → Grid → Border → Button）</summary>
    public static string GetVisualPath(this DependencyObject element)
    {
        var ancestors = new List<string>();
        var current = element;
        while (current != null)
        {
            ancestors.Add(current.GetType().Name);
            current = VisualTreeHelper.GetParent(current);
        }
        ancestors.Reverse();
        return string.Join(" → ", ancestors);
    }

    #endregion

    #region 🧩 LogicalTree 逻辑树支持

    /// <summary>逻辑父级（LogicalTree）</summary>
    public static T? FindLogicalParent<T>(this DependencyObject child) where T : DependencyObject
    {
        var parent = LogicalTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
            parent = LogicalTreeHelper.GetParent(parent);
        return parent as T;
    }

    /// <summary>逻辑父级（非泛型）</summary>
    public static DependencyObject? FindLogicalParent(this DependencyObject child, Type targetType)
    {
        var parent = LogicalTreeHelper.GetParent(child);
        while (parent != null && !targetType.IsInstanceOfType(parent))
            parent = LogicalTreeHelper.GetParent(parent);
        return parent;
    }

    /// <summary>获取逻辑子元素</summary>
    public static IEnumerable<DependencyObject> FindLogicalChildren(this DependencyObject parent, Type targetType)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>())
        {
            if (targetType.IsInstanceOfType(child))
                yield return child;

            foreach (var sub in FindLogicalChildren(child, targetType))
                yield return sub;
        }
    }

    #endregion
}
