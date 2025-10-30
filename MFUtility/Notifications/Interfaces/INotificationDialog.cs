using MFUtility.Notifications.Enums;

namespace MFUtility.Notifications.Interfaces;

public interface INotificationDialog {
	string MessageTitle { get; set; }
	string Message { get; set; }
	NotificationType Type { get; set; }
}