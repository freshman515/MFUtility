using MFUtility.Enums;
using MFUtility.Services;

namespace MFUtility.Interfaces;

public interface INotificationDialog {
	string MessageTitle { get; set; }
	string Message { get; set; }
	NotificationType Type { get; set; }
}