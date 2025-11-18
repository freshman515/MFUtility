using System.Runtime.InteropServices;

namespace MFUtility.WPF.Win32;

public class NativeStructs {
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT {
		public int Left, Top, Right, Bottom;
		public int Width => Right - Left;
		public int Height => Bottom - Top;
		public override string ToString() => $"RECT({Left},{Top},{Width},{Height})";
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct MONITORINFO {
		public int cbSize;
		public RECT rcMonitor;
		public RECT rcWork;
		public int dwFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT {
		public int X, Y;
		public POINT(int x, int y) {
			X = x;
			Y = y;
		}
	}
}