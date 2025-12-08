using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MFUtility.Win32 {
	public static class Win32Display {


		private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

		public static IntPtr GetMonitorFromWindow(IntPtr hwnd) {
			const int MONITOR_DEFAULTTONEAREST = 0x00000002;
			return MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
		}

		public static NativeStructs.MONITORINFO GetMonitorInfo(IntPtr monitor) {
			var info = new NativeStructs.MONITORINFO { cbSize = Marshal.SizeOf(typeof(NativeStructs.MONITORINFO)) };
			GetMonitorInfo(monitor, ref info);
			return info;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref NativeStructs.MONITORINFO lpmi);

		[DllImport("user32.dll")]
		private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
			MonitorEnumProc lpfnEnum, IntPtr dwData);

		private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref NativeStructs.RECT lprcMonitor, IntPtr dwData);

		/// <summary>
		/// 获取指定窗口所在显示器的工作区域。
		/// </summary>
		public static (double Left, double Top, double Width, double Height) GetWorkAreaFromWindow(IntPtr hwnd) {
			var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
			var info = new NativeStructs.MONITORINFO { cbSize = Marshal.SizeOf(typeof(NativeStructs.MONITORINFO)) };
			if (!GetMonitorInfo(monitor, ref info))
				throw new InvalidOperationException("无法获取显示器信息。");

			var r = info.rcWork;
			return (r.Left, r.Top, r.Width, r.Height);
		}

		/// <summary>
		/// 获取系统中所有显示器的矩形信息。
		/// </summary>
		public static IEnumerable<(IntPtr Handle, NativeStructs.RECT Bounds)> GetAllMonitors() {
			var list = new List<(IntPtr Handle, NativeStructs.RECT Bounds)>();

			// ✅ 回调签名必须匹配 delegate 定义
			bool Callback(IntPtr hMonitor, IntPtr hdcMonitor, ref NativeStructs.RECT lprcMonitor, IntPtr dwData) {
				list.Add((hMonitor, lprcMonitor));
				return true; // 返回 true 继续枚举
			}

			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, IntPtr.Zero);
			return list;
		}
	}
}