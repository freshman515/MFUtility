using System.Windows.Media;

namespace MFUtility.Helpers;

public static class VisualHelper
{
    public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null && !(parent is T))
            parent = VisualTreeHelper.GetParent(parent);
        return parent as T;
    }

    public static T FindChild<T>(DependencyObject parent, string childName = null) where T : FrameworkElement
    {
        if (parent == null) return null;
        int count = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T t && (string.IsNullOrEmpty(childName) || t.Name == childName))
                return t;

            var found = FindChild<T>(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }
}
