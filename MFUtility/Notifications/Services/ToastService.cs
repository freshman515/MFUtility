using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using MFUtility.Notifications.Enums;
using MFUtility.Views;
using MFUtility.Win32;

namespace MFUtility.Notifications.Services;

public static class ToastService
{
	private static readonly Dictionary<(IntPtr monitor, NotifycationPosition pos), List<ToastWindow>> _toastsByPosition = new();
	private static readonly Dictionary<(IntPtr monitor, NotifycationPosition pos), SemaphoreSlim> _positionLocks = new();
	private static readonly Dictionary<ToastWindow, double> _toastOffsets = new();
	private const double Gap = 12;
	private static IntPtr _currentMonitor = IntPtr.Zero;

	#region === 绑定主窗体 ===

	public static void BindToWindow(Window mainWindow)
	{
		if (mainWindow == null) return;

		void UpdateLater()
		{
			Task.Delay(80).ContinueWith(_ =>
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					_currentMonitor = Win32Display.GetMonitorFromWindow(mainWindow.GetHandle());
				});
			});
		}

		mainWindow.SourceInitialized += (_, _) => UpdateLater();
		mainWindow.LocationChanged += (_, _) => UpdateLater();
		mainWindow.SizeChanged += (_, _) => UpdateLater();
	}

	#endregion

	#region === 快捷方法 ===

	public static void ShowSuccess(string message, int staySeconds = 2,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Success, mode, staySeconds, owner, position);

	public static void ShowError(string message, int staySeconds =2,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Error, mode, staySeconds, owner, position);

	public static void ShowWarning(string message, int staySeconds = 2,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Warning, mode, staySeconds, owner, position);

	public static void ShowInfo(string message, int staySeconds = 2,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Info, mode, staySeconds, owner, position);

	#endregion

	public static void Show(string message,
		NotificationType status = NotificationType.Success,
		NotificationMode mode = NotificationMode.Stack,
		int staySeconds = 4,
		Window owner = null,
		NotifycationPosition position = NotifycationPosition.Center)
	{
		Application.Current.Dispatcher.Invoke(async () =>
		{
			var monitor = _currentMonitor;
			if (monitor == IntPtr.Zero)
			{
				var baseWindow = owner ?? Application.Current.MainWindow;
				monitor = Win32Display.GetMonitorFromWindow(new WindowInteropHelper(baseWindow).Handle);
				_currentMonitor = monitor;
			}

			var key = (monitor, position);

			if (!_toastsByPosition.TryGetValue(key, out var list))
				_toastsByPosition[key] = list = new List<ToastWindow>();

			if (!_positionLocks.TryGetValue(key, out var sem))
				_positionLocks[key] = sem = new SemaphoreSlim(1, 1);

			if (mode == NotificationMode.Replace)
			{
				foreach (var t in list.ToList())
					await ToastAnimator.PlayCloseAsync(t, position);
				list.Clear();
			}

			var toast = new ToastWindow(message, status, TimeSpan.FromSeconds(staySeconds))
			{
				Owner = owner ?? Application.Current.MainWindow,
				Topmost = true,
				Tag = (monitor, position)
			};

			toast.Show();
			toast.UpdateLayout();

			double height = toast.ActualHeight > 0 ? toast.ActualHeight : 120;
			double width = toast.ActualWidth > 0 ? toast.ActualWidth : 300;

			list.Add(toast);

			var (left, top) = CalculatePosition(monitor, position, width, height, mode, list, list.Count - 1);
			toast.Left = left;
			toast.Top = top;
			_toastOffsets[toast] = top;

			toast.Closed += ToastClosed;
			ToastAnimator.PlayOpen(toast, position);

			if (mode != NotificationMode.Persistent)
			{
				await Task.Delay(staySeconds * 1000);
				if (toast.IsVisible)
					await ToastAnimator.PlayCloseAsync(toast, position);
			}
		});
	}

	private static (double left, double top) CalculatePosition(
		IntPtr monitor,
		NotifycationPosition position,
		double width,
		double height,
		NotificationMode mode,
		List<ToastWindow> list,
		int index)
	{
		var info = Win32Display.GetMonitorInfo(monitor);
		double screenLeft = info.rcWork.Left;
		double screenTop = info.rcWork.Top;
		double screenW = info.rcWork.Width;
		double screenH = info.rcWork.Height;

		double left = position switch
		{
			NotifycationPosition.TopLeft => screenLeft + 20,
			NotifycationPosition.TopRight => screenLeft + screenW - width - 20,
			NotifycationPosition.BottomLeft => screenLeft + 20,
			NotifycationPosition.BottomRight => screenLeft + screenW - width - 20,
			NotifycationPosition.TopCenter => screenLeft + (screenW - width) / 2,
			NotifycationPosition.BottomCenter => screenLeft + (screenW - width) / 2,
			_ => screenLeft + (screenW - width) / 2
		};

		double top = position switch
		{
			NotifycationPosition.TopLeft or NotifycationPosition.TopRight or NotifycationPosition.TopCenter
				=> screenTop + 20,
			NotifycationPosition.BottomLeft or NotifycationPosition.BottomRight or NotifycationPosition.BottomCenter
				=> screenTop + screenH - height - 20,
			_ => screenTop + (screenH - height) / 2
		};

		// 堆叠逻辑
		if (mode == NotificationMode.Stack && list.Count > 1)
		{
			bool fromTop = IsTopPosition(position);
			var last = list.Count > 1 ? list[list.Count - 2] : null;
			double lastTop = last != null && _toastOffsets.TryGetValue(last, out var v) ? v : top;

			top = fromTop
				? lastTop + height + Gap
				: lastTop - height - Gap;
		}

		return (left, top);
	}

	private static bool IsTopPosition(NotifycationPosition position)
		=> position.ToString().Contains("Top");

	private static async void ToastClosed(object sender, EventArgs e)
	{
		if (sender is not ToastWindow closed) return;

		var key = closed.Tag is ValueTuple<IntPtr, NotifycationPosition> t
			? t
			: (IntPtr.Zero, NotifycationPosition.Center);

		if (!_toastsByPosition.TryGetValue(key, out var list) ||
		    !_positionLocks.TryGetValue(key, out var sem)) return;

		await sem.WaitAsync();
		try
		{
			list.Remove(closed);
			_toastOffsets.Remove(closed);

			if (list.Count == 0) return;

			bool topLayout = IsTopPosition(key.Item2);
			var moveList = topLayout
				? list.Where(t => t.Top > closed.Top).OrderBy(t => t.Top).ToList()
				: list.Where(t => t.Top < closed.Top).OrderByDescending(t => t.Top).ToList();

			foreach (var t2 in moveList)
			{
				double newTop = topLayout
					? _toastOffsets[t2] - (closed.ActualHeight + Gap)
					: _toastOffsets[t2] + (closed.ActualHeight + Gap);

				_toastOffsets[t2] = newTop;
				ToastAnimator.PlayDrop(t2, newTop);
			}
		}
		finally
		{
			sem.Release();
		}
	}
}


public static class ToastAnimator {
	public static void PlayOpen(Window window, NotifycationPosition pos, int duration = 300) {
		if (window == null) return;

		double targetTop = window.Top;
		bool fromTop = pos.ToString().Contains("Top");

		window.Top += fromTop ? -25 : 25;
		window.Opacity = 0;

		var slide = new DoubleAnimation {
			From = window.Top,
			To = targetTop,
			Duration = TimeSpan.FromMilliseconds(duration),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
		};

		var fade = new DoubleAnimation {
			From = 0,
			To = 1,
			Duration = TimeSpan.FromMilliseconds(duration)
		};

		window.BeginAnimation(Window.TopProperty, slide);
		window.BeginAnimation(Window.OpacityProperty, fade);

		try { SystemSounds.Asterisk.Play(); } catch { }
	}

	public static async Task PlayCloseAsync(Window window, NotifycationPosition pos, int duration = 300) {
		if (window == null) return;

		bool toTop = pos.ToString().Contains("Top");

		var fadeOut = new DoubleAnimation {
			From = 1,
			To = 0,
			Duration = TimeSpan.FromMilliseconds(duration)
		};

		var slideOut = new DoubleAnimation {
			From = window.Top,
			To = window.Top + (toTop ? -25 : 25),
			Duration = TimeSpan.FromMilliseconds(duration),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
		};

		var tcs = new TaskCompletionSource<object>();
		fadeOut.Completed += (_, _) => tcs.TrySetResult(null);

		window.BeginAnimation(Window.OpacityProperty, fadeOut);
		window.BeginAnimation(Window.TopProperty, slideOut);

		await tcs.Task;
		window.Close();
	}

	public static void PlayDrop(Window window, double newTop) {
		var anim = new DoubleAnimation {
			From = window.Top,
			To = newTop,
			Duration = TimeSpan.FromMilliseconds(250),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
		};

		anim.Completed += (_, _) => window.Top = newTop;
		window.BeginAnimation(Window.TopProperty, anim);
	}
}