using System.Runtime.InteropServices;
using System.Windows.Input;

namespace MFUtility.WPF.Win32
{
    public static class Win32Input
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out NativeStructs.POINT lpPoint);

        public static bool IsKeyDown(System.Windows.Input.Key key)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            return (GetAsyncKeyState(vk) & 0x8000) != 0;
        }

        public static NativeStructs.POINT GetCursorPosition()
        {
            GetCursorPos(out var pt);
            return pt;
        }
    }
}
