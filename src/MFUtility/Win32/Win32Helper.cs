using System;
using System.Windows;

namespace MFUtility.Win32
{
    public static class Win32Helper
    {
        public static IntPtr GetHandle(this Window window)
            => new System.Windows.Interop.WindowInteropHelper(window).Handle;

        public static (double X, double Y, double W, double H) GetWorkArea(this Window window)
            => Win32Display.GetWorkAreaFromWindow(window.GetHandle());

        public static bool IsKeyDown(System.Windows.Input.Key key)
            => Win32Input.IsKeyDown(key);

        public static double GetDPIScale()
            => Win32System.GetScaleFactor();
    }
}
