using System.Runtime.InteropServices;

namespace MFUtility.WPF.Win32
{
    public static class Win32Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeStructs.RECT rect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static NativeStructs.RECT GetBounds(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out var r);
            return r;
        }

        public static void Move(IntPtr hwnd, int x, int y, int width, int height)
        {
            const uint SWP_NOZORDER = 0x0004;
            SetWindowPos(hwnd, IntPtr.Zero, x, y, width, height, SWP_NOZORDER);
        }

        public static bool IsActiveWindow(IntPtr hwnd)
            => hwnd == GetForegroundWindow();

        public static void Show(IntPtr hwnd, bool visible)
            => ShowWindow(hwnd, visible ? 5 : 0); // 5=SW_SHOW, 0=SW_HIDE
    }
}
