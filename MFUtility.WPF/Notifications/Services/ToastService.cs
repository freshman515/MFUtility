
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using MFUtility.Notifications.Enums;
using MFUtility.WPF.Views;


namespace MFUtility.Notifications.Services;

public static class ToastService
{
	private static readonly Dictionary<NotifycationPosition, List<ToastWindow>> _toastsByPosition = new();
	private static readonly Dictionary<NotifycationPosition, SemaphoreSlim> _positionLocks = new();
	private static readonly Dictionary<ToastWindow, double> _toastOffsets = new();
	private const double Gap = 12;

	#region === 快捷方法 ===
	public static void ShowSuccess(string message, int staySeconds = 2,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Success, mode, staySeconds, owner, position);

	public static void ShowError(string message, int staySeconds = 2,
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

	public static async void Show(string message,
		NotificationType status = NotificationType.Success,
		NotificationMode mode = NotificationMode.Stack,
		int staySeconds = 4,
		Window owner = null,
		NotifycationPosition position = NotifycationPosition.Center)
	{
		await Application.Current.Dispatcher.InvokeAsync(async () =>
		{
			var key = position;

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
				Tag = position
			};

			toast.Show();
			toast.UpdateLayout();

			double height = toast.ActualHeight > 0 ? toast.ActualHeight : 120;
			double width = toast.ActualWidth > 0 ? toast.ActualWidth : 300;

			list.Add(toast);

			var (left, top) = CalculatePosition(owner, position, width, height, mode, list, list.Count - 1);
			toast.Left = left;
			toast.Top = top;
			_toastOffsets[toast] = top;

			toast.Closed += ToastClosed;
			ToastAnimator.PlayOpen(toast, position);

			if (mode != NotificationMode.Persistent)
			{
				try
				{
					await Task.Delay(staySeconds * 1000);
					if (toast.IsVisible && toast.IsLoaded)
						await ToastAnimator.PlayCloseAsync(toast, position);
				}
				catch { /* ignore */ }
			}
		});
	}

	private static (double left, double top) CalculatePosition(
		Window owner,
		NotifycationPosition position,
		double width,
		double height,
		NotificationMode mode,
		List<ToastWindow> list,
		int index)
	{
		double screenW = SystemParameters.WorkArea.Width;
		double screenH = SystemParameters.WorkArea.Height;
		double screenLeft = SystemParameters.WorkArea.Left;
		double screenTop = SystemParameters.WorkArea.Top;

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
		var pos = closed.Tag is NotifycationPosition p ? p : NotifycationPosition.Center;

		if (!_toastsByPosition.TryGetValue(pos, out var list) ||
		    !_positionLocks.TryGetValue(pos, out var sem)) return;

		await sem.WaitAsync();
		try
		{
			if (list.Contains(closed))
			{
				list.Remove(closed);
				_toastOffsets.Remove(closed);
			}

			if (list.Count == 0) return;

			bool topLayout = IsTopPosition(pos);
			var moveList = topLayout
				? list.Where(t => t.Top > closed.Top).OrderBy(t => t.Top).ToList()
				: list.Where(t => t.Top < closed.Top).OrderByDescending(t => t.Top).ToList();

			foreach (var t2 in moveList)
			{
				if (!_toastOffsets.TryGetValue(t2, out var origTop))
					continue;

				double newTop = topLayout
					? origTop - (closed.ActualHeight + Gap)
					: origTop + (closed.ActualHeight + Gap);

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



public static class ToastAnimator
{
	public static void PlayOpen(Window window, NotifycationPosition pos, int duration = 300)
	{
		if (window == null) return;

		double targetTop = window.Top;
		bool fromTop = pos.ToString().Contains("Top");

		window.Top += fromTop ? -25 : 25;
		window.Opacity = 0;

		var slide = new DoubleAnimation
		{
			From = window.Top,
			To = targetTop,
			Duration = TimeSpan.FromMilliseconds(duration),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
		};

		var fade = new DoubleAnimation
		{
			From = 0,
			To = 1,
			Duration = TimeSpan.FromMilliseconds(duration)
		};

		slide.Completed += (_, _) => window.Top = targetTop;
		fade.Completed += (_, _) => window.Opacity = 1;

		window.BeginAnimation(Window.TopProperty, slide);
		window.BeginAnimation(Window.OpacityProperty, fade);

		try { SystemSounds.Asterisk.Play(); } catch { }
	}

	public static async Task PlayCloseAsync(Window window, NotifycationPosition pos, int duration = 300)
	{
		if (window == null || !window.IsVisible || !window.IsLoaded)
			return;

		bool toTop = pos.ToString().Contains("Top");

		var fadeOut = new DoubleAnimation
		{
			From = 1,
			To = 0,
			Duration = TimeSpan.FromMilliseconds(duration)
		};

		var slideOut = new DoubleAnimation
		{
			From = window.Top,
			To = window.Top + (toTop ? -25 : 25),
			Duration = TimeSpan.FromMilliseconds(duration),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
		};

		var tcs = new TaskCompletionSource<object?>();
		fadeOut.Completed += (_, _) => tcs.TrySetResult(null);

		window.BeginAnimation(Window.OpacityProperty, fadeOut);
		window.BeginAnimation(Window.TopProperty, slideOut);

		await tcs.Task;

		if (window.IsVisible)
			window.Close();
	}

	public static void PlayDrop(Window window, double newTop)
	{
		if (window == null || !window.IsLoaded)
			return;

		var anim = new DoubleAnimation
		{
			From = window.Top,
			To = newTop,
			Duration = TimeSpan.FromMilliseconds(250),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
		};

		anim.Completed += (_, _) => window.Top = newTop;
		window.BeginAnimation(Window.TopProperty, anim);
	}
}

