using System.Runtime.InteropServices;
using System.Text;

namespace MFUtility.WPF.Win32;

/// <summary>
/// 扩展 Win32 窗口控制功能（前台检测、全屏判断、置顶、激活等）
/// </summary>
public static class Win32WindowEx {
        #region === Win32 Imports ===

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	[DllImport("user32.dll")]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool BringWindowToTop(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
		int X, int Y, int cx, int cy, uint uFlags);

	private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
	private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

	private const uint SWP_NOMOVE = 0x0002;
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_SHOWWINDOW = 0x0040;

        #endregion

        #region === 窗口检测 ===

	/// <summary>
	/// 获取当前前台窗口句柄。
	/// </summary>
	public static IntPtr GetForeground() => GetForegroundWindow();

	/// <summary>
	/// 获取窗口标题。
	/// </summary>
	public static string GetTitle(IntPtr hwnd) {
		int length = GetWindowTextLength(hwnd);
		if (length == 0) return string.Empty;

		var sb = new StringBuilder(length + 1);
		GetWindowText(hwnd, sb, sb.Capacity);
		return sb.ToString();
	}

	/// <summary>
	/// 判断窗口是否当前活动窗口。
	/// </summary>
	public static bool IsForeground(IntPtr hwnd) => hwnd == GetForegroundWindow();

	/// <summary>
	/// 判断窗口是否可见。
	/// </summary>
	public static bool IsVisible(IntPtr hwnd) => IsWindowVisible(hwnd);

	/// <summary>
	/// 判断窗口是否最小化。
	/// </summary>
	public static bool IsMinimized(IntPtr hwnd) => IsIconic(hwnd);

        #endregion

        #region === 窗口操作 ===

	/// <summary>
	/// 激活窗口并置顶到前台。
	/// </summary>
	public static void Activate(IntPtr hwnd) {
		if (hwnd == IntPtr.Zero) return;
		BringWindowToTop(hwnd);
		SetForegroundWindow(hwnd);
	}

	/// <summary>
	/// 设置窗口置顶（true）或取消置顶（false）。
	/// </summary>
	public static void SetTopMost(IntPtr hwnd, bool topMost) {
		if (hwnd == IntPtr.Zero) return;

		SetWindowPos(
			hwnd,
			topMost ? HWND_TOPMOST : HWND_NOTOPMOST,
			0, 0, 0, 0,
			SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
	}

	/// <summary>
	/// 显示或隐藏窗口。
	/// </summary>
	public static void Show(IntPtr hwnd, bool visible) {
		const int SW_SHOW = 5;
		const int SW_HIDE = 0;
		ShowWindow(hwnd, visible ? SW_SHOW : SW_HIDE);
	}

        #endregion

        #region === 全屏判断 ===

	/// <summary>
	/// 判断指定窗口是否全屏。
	/// </summary>
	public static bool IsFullScreen(IntPtr hwnd) {
		if (hwnd == IntPtr.Zero) return false;

		Win32Window.GetBounds(hwnd);
		var rect = Win32Window.GetBounds(hwnd);

		foreach (var (h, bounds) in Win32Display.GetAllMonitors()) {
			if (Math.Abs(bounds.Left - rect.Left) < 2 &&
			    Math.Abs(bounds.Top - rect.Top) < 2 &&
			    Math.Abs(bounds.Width - rect.Width) < 2 &&
			    Math.Abs(bounds.Height - rect.Height) < 2)
				return true;
		}

		return false;
	}

        #endregion

        #region === 辅助 ===

	/// <summary>
	/// 打印当前前台窗口信息（调试用）。
	/// </summary>
	public static void DumpForegroundWindow() {
		var hwnd = GetForegroundWindow();
		string title = GetTitle(hwnd);
		Console.WriteLine($"[Active Window] HWND={hwnd}, Title=\"{title}\"");
	}

        #endregion
}