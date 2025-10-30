using System.Media;
using System.Windows.Media.Animation;
using MFUtility.Notifications.Enums;
using MFUtility.Views;

namespace MFUtility.Notifications.Services;

public static class ToastService
{
	private static readonly Dictionary<NotifycationPosition, List<ToastWindow>> _toastsByPosition = new();
	private static readonly Dictionary<NotifycationPosition, SemaphoreSlim> _positionLocks = new(); // ✅ 锁
	private static readonly Dictionary<ToastWindow, double> _toastOffsets = new(); // ✅ 逻辑位置记录
	private const double Gap = 12;

	#region === 快捷方法 ===
	public static void ShowSuccess(string message, int staySeconds = 4,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Success, mode, staySeconds, owner, position);

	public static void ShowError(string message, int staySeconds = 4,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Error, mode, staySeconds, owner, position);

	public static void ShowWarning(string message, int staySeconds = 4,
		NotifycationPosition position = NotifycationPosition.Center,
		NotificationMode mode = NotificationMode.Stack, Window owner = null)
		=> Show(message, NotificationType.Warning, mode, staySeconds, owner, position);

	public static void ShowInfo(string message, int staySeconds = 4,
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
			if (!_toastsByPosition.TryGetValue(position, out var list))
				_toastsByPosition[position] = list = new List<ToastWindow>();
			if (!_positionLocks.TryGetValue(position, out var sem))
				_positionLocks[position] = sem = new SemaphoreSlim(1, 1);

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

			// ✅ 提前加入队列（占位）
			list.Add(toast);

			// ✅ 根据“我在第几个”计算高度偏移
			var (left, top) = CalculatePosition(position, width, height, mode, list, list.Count - 1);
			toast.Left = left;
			toast.Top = top;
			_toastOffsets[toast] = top; // ✅ 保存逻辑位置

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
		NotifycationPosition position,
		double width,
		double height,
		NotificationMode mode,
		List<ToastWindow> list,
		int index)
	{
		double screenW = SystemParameters.WorkArea.Width;
		double screenH = SystemParameters.WorkArea.Height;
		double left = 0, top = 0;

		switch (position)
		{
			case NotifycationPosition.TopLeft:
				left = 20;
				top = 20;
				break;
			case NotifycationPosition.TopRight:
				left = screenW - width - 20;
				top = 20;
				break;
			case NotifycationPosition.BottomLeft:
				left = 20;
				top = screenH - height - 20;
				break;
			case NotifycationPosition.BottomRight:
				left = screenW - width - 20;
				top = screenH - height - 20;
				break;
			case NotifycationPosition.TopCenter:
				left = (screenW - width) / 2;
				top = 40;
				break;
			case NotifycationPosition.BottomCenter:
				left = (screenW - width) / 2;
				top = screenH - height - 40;
				break;
			case NotifycationPosition.Center:
			default:
				left = (screenW - width) / 2;
				top = (screenH - height) / 2;
				break;
		}

		// ✅ 堆叠逻辑
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

	private static bool IsTopPosition(NotifycationPosition position) =>
		position.ToString().Contains("Top");

	private static async void ToastClosed(object sender, EventArgs e)
	{
		if (sender is not ToastWindow closed) return;
		var pos = closed.Tag as NotifycationPosition? ?? NotifycationPosition.Center;

		if (!_toastsByPosition.TryGetValue(pos, out var list) ||
		    !_positionLocks.TryGetValue(pos, out var sem)) return;

		await sem.WaitAsync(); // ✅ 锁定同位置动画
		try
		{
			list.Remove(closed);
			_toastOffsets.Remove(closed);

			if (list.Count == 0) return;

			bool topLayout = IsTopPosition(pos);
			var moveList = topLayout
				? list.Where(t => t.Top > closed.Top).OrderBy(t => t.Top).ToList()
				: list.Where(t => t.Top < closed.Top).OrderByDescending(t => t.Top).ToList();

			foreach (var t in moveList)
			{
				double newTop = topLayout
					? _toastOffsets[t] - (closed.ActualHeight + Gap)
					: _toastOffsets[t] + (closed.ActualHeight + Gap);

				_toastOffsets[t] = newTop; // ✅ 更新逻辑位置
				ToastAnimator.PlayDrop(t, newTop);
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

		window.BeginAnimation(Window.TopProperty, slide);
		window.BeginAnimation(Window.OpacityProperty, fade);

		try { SystemSounds.Asterisk.Play(); } catch { }
	}

	public static async Task PlayCloseAsync(Window window, NotifycationPosition pos, int duration = 300)
	{
		if (window == null) return;

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

		var tcs = new TaskCompletionSource<object>();
		fadeOut.Completed += (_, _) => tcs.TrySetResult(null);

		window.BeginAnimation(Window.OpacityProperty, fadeOut);
		window.BeginAnimation(Window.TopProperty, slideOut);

		await tcs.Task;
		window.Close();
	}

	public static void PlayDrop(Window window, double newTop)
	{
		var anim = new DoubleAnimation
		{
			From = window.Top,
			To = newTop,
			Duration = TimeSpan.FromMilliseconds(250),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
		};

		anim.Completed += (_, _) => window.Top = newTop; // ✅ 同步真实位置
		window.BeginAnimation(Window.TopProperty, anim);
	}
}

