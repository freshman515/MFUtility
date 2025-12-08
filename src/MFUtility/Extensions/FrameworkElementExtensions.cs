using System.Diagnostics;
using System.Windows;

namespace MFUtility.Extensions;

public static  class FrameworkElementExtensions {
	public static Window? GetParentWindow(this FrameworkElement element)
    {
        try
        {
            return Window.GetWindow(element);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FrameworkElementExtensions.GetParentWindow] {ex.Message}");
            return null;
        }
    }
      public static void RefreshLayout(this FrameworkElement element)
    {
        try
        {
            element?.InvalidateMeasure();
            element?.InvalidateVisual();
            element?.UpdateLayout();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FrameworkElementExtensions.RefreshLayout] {ex.Message}");
        }
    }
       public static object? TryFindResourceSafe(this FrameworkElement element, object key)
    {
        try
        {
            return element?.TryFindResource(key);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FrameworkElementExtensions.TryFindResourceSafe] {ex.Message}");
            return null;
        }
    }
}