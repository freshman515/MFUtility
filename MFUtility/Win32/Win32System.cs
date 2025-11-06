using System;
using System.Runtime.InteropServices;

namespace MFUtility.Win32
{
    public static class Win32System
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

        public static double GetScaleFactor()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
            ReleaseDC(IntPtr.Zero, hdc);
            return dpiX / 96.0;
        }

        public static string GetOSVersion() => Environment.OSVersion.VersionString;
    }
}
