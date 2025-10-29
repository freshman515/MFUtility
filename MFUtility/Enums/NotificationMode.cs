namespace MFUtility.Enums;

public enum NotificationMode {
	/// <summary>新通知向上叠加（默认）</summary>
	Stack,
	/// <summary>覆盖上一个通知，只保留一个</summary>
	Replace,
	/// <summary>持久显示，直到手动关闭</summary>
	Persistent
}