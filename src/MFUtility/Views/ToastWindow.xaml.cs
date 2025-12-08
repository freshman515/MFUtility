using System.Windows.Media.Animation;
using MFUtility.Enums;
using MFUtility.Notifications.Enums;

namespace MFUtility.Views {
	/// <summary>
	/// ToastWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ToastWindow : Window {
		public string Status { get; }
		private readonly TimeSpan _duration;
		public string Message { get; }

		public ToastWindow(string message, NotificationType status = NotificationType.Success, TimeSpan? duration = null) {
			_duration = duration ?? TimeSpan.FromSeconds(1);
			Status = status.ToString();
			Message = string.IsNullOrEmpty(message) ? GetDefaultMessage(status) : message;

			InitializeComponent();
			DataContext = this;
			Opacity = 0;
		}

		private string GetDefaultMessage(NotificationType status) {
			var isEnglish = false;
			switch (status) {
				case NotificationType.Success:
					return isEnglish ? "OK" : "成功";
				case NotificationType.Error:
					return isEnglish ? "Error" : "失败";
				case NotificationType.Warning:
					return isEnglish ? "Warning" : "提示";
				case NotificationType.Info:
					return isEnglish ? "Info" : "消息";
				default:
					return isEnglish ? "OK" : "成功";

			}
		}


		private void ToastWindow_Loaded(object sender, RoutedEventArgs e) {
			var anim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
			this.BeginAnimation(OpacityProperty, anim);

			Task.Delay(_duration).ContinueWith(_ => {
				Dispatcher.Invoke(() => {
					var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(300)));
					fadeOut.Completed += (s2, e2) => this.Close();
					this.BeginAnimation(OpacityProperty, fadeOut);
				});
			});
		}

	}
}