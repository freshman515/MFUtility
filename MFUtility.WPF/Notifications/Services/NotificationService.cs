using System.Media;
using System.Windows;
using System.Windows.Media.Animation;
using MFUtility.Notifications.Enums;
using MFUtility.Notifications.Interfaces;
using MFUtility.WPF.Views;

namespace MFUtility.WPF.Notifications.Services;

public static class NotificationService
{
	private static readonly List<Window> _dialogs = new();
	private const double Gap = 10;

	#region === 快捷方法 ===
	public static void ShowSuccess(string title, string content, int staySeconds = 4,
		NotificationMode mode = NotificationMode.Stack, bool enableBorderColor = true)
		=> Show(title, content, NotificationType.Success, mode, staySeconds, enableBorderColor);

	public static void ShowError(string title, string content, int staySeconds = 4,
		NotificationMode mode = NotificationMode.Stack, bool enableBorderColor = true)
		=> Show(title, content, NotificationType.Error, mode, staySeconds, enableBorderColor);

	public static void ShowWarning(string title, string content, int staySeconds = 4,
		NotificationMode mode = NotificationMode.Stack, bool enableBorderColor = true)
		=> Show(title, content, NotificationType.Warning, mode, staySeconds, enableBorderColor);

	public static void ShowInfo(string title, string content, int staySeconds = 4,
		NotificationMode mode = NotificationMode.Stack, bool enableBorderColor = true)
		=> Show(title, content, NotificationType.Info, mode, staySeconds, enableBorderColor);
	#endregion

	// =========================================================
	// 默认通知（使用内置 DefaultNotificationWindow）
	// =========================================================
	public static void Show(
		string title,
		string content,
		NotificationType type = NotificationType.Info,
		NotificationMode mode = NotificationMode.Stack,
		int staySeconds = 5,
		bool enableBorderColor = true)
	{
		Show<DefaultNotificationWindow>(title, content, type, mode, staySeconds, enableBorderColor);
	}

	// =========================================================
	// 泛型重载（允许用户自定义窗口）
	// =========================================================
	public static void Show<T>(
		string title,
		string content,
		NotificationType type = NotificationType.Info,
		NotificationMode mode = NotificationMode.Stack,
		int staySeconds = 5,
		bool enableBorderColor = false)
		where T : Window, INotificationDialog, new()
	{
		Application.Current.Dispatcher.Invoke(async () =>
		{
			if (mode == NotificationMode.Replace)
			{
				foreach (var dlg in _dialogs.ToList())
					await NotificationAnimator.PlayCloseAsync(dlg);
				_dialogs.Clear();
			}

			var dialog = new T
			{
				Topmost = true,
				Owner = Application.Current.MainWindow,
				MessageTitle = title,
				Message = content,
				Type = type
			};

			// ✅ 如果支持边框颜色，则赋值
			if (dialog is DefaultNotificationWindow defWin)
				defWin.EnableBorderColor = enableBorderColor;

			dialog.Show();
			dialog.UpdateLayout();

			double height = dialog.ActualHeight > 0 ? dialog.ActualHeight : 140;
			double width = dialog.ActualWidth > 0 ? dialog.ActualWidth : 400;

			double left = SystemParameters.WorkArea.Right - width - 20;
			double top = GetNextTop(height, mode);

			_dialogs.Add(dialog);
			dialog.Closed += DialogClosed;

			NotificationAnimator.PlayOpen(dialog, left, top);

			if (mode != NotificationMode.Persistent)
			{
				await Task.Delay(staySeconds * 1000);
				if (dialog.IsVisible)
					await NotificationAnimator.PlayCloseAsync(dialog);
			}
		});
	}

	// =========================================================
	// 补位与定位逻辑
	// =========================================================
	private static void DialogClosed(object sender, EventArgs e)
	{
		if (sender is not Window closed) return;
		_dialogs.Remove(closed);

		if (_dialogs.Count == 0) return;

		var ordered = _dialogs.OrderByDescending(w => w.Top).ToList();
		double bottom = SystemParameters.WorkArea.Bottom;
		double offset = bottom;

		foreach (var dlg in ordered)
		{
			double height = dlg.ActualHeight > 0 ? dlg.ActualHeight : 140;
			offset -= (height + Gap);
			NotificationAnimator.PlayDrop(dlg, offset);
		}
	}

	private static double GetNextTop(double height, NotificationMode mode)
	{
		double bottom = SystemParameters.WorkArea.Bottom;

		if (mode == NotificationMode.Replace)
			return bottom - height - Gap;

		if (_dialogs.Count == 0)
			return bottom - height - Gap;

		var last = _dialogs.Last();
		double nextTop = last.Top - (height + Gap);
		if (nextTop < 0)
			nextTop = bottom - height - Gap;

		return nextTop;
	}
}

/// <summary>
/// 🌟 通知动画控制器
/// </summary>
public static class NotificationAnimator
{
	public static void PlayOpen(Window window, double targetLeft, double targetTop, int duration = 300)
	{
		if (window == null) return;

		window.Left = SystemParameters.WorkArea.Right; // 从屏幕右侧外开始
		window.Top = targetTop;
		window.Opacity = 0;

		var slideIn = new DoubleAnimation
		{
			From = SystemParameters.WorkArea.Right,
			To = targetLeft,
			Duration = TimeSpan.FromMilliseconds(duration),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
		};

		var fadeIn = new DoubleAnimation
		{
			From = 0,
			To = 1,
			Duration = TimeSpan.FromMilliseconds(duration / 1.2)
		};

		window.BeginAnimation(Window.LeftProperty, slideIn);
		window.BeginAnimation(Window.OpacityProperty, fadeIn);
		try { SystemSounds.Asterisk.Play(); } catch { }
	}

	public static async Task PlayCloseAsync(Window window, int duration = 400)
	{
		if (window == null) return;

		double right = SystemParameters.WorkArea.Right;
		var slideOut = new DoubleAnimation
		{
			From = window.Left,
			To = right,
			Duration = TimeSpan.FromMilliseconds(duration),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
		};

		var fadeOut = new DoubleAnimation
		{
			From = 1,
			To = 0,
			Duration = TimeSpan.FromMilliseconds(duration / 1.5)
		};

		var tcs = new TaskCompletionSource<object>();
		slideOut.Completed += (_, _) => tcs.TrySetResult(null);

		window.BeginAnimation(Window.LeftProperty, slideOut);
		window.BeginAnimation(Window.OpacityProperty, fadeOut);

		await tcs.Task;
		window.Close();
	}

	public static void PlayDrop(Window window, double newTop)
	{
		var anim = new DoubleAnimation
		{
			From = window.Top,
			To = newTop,
			Duration = TimeSpan.FromMilliseconds(300),
			EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
		};
		window.BeginAnimation(Window.TopProperty, anim);
	}
}
