namespace MFUtility.Communication.Socket.Messages;
public enum ConnectionState {
	Disconnected,
	Connecting,
	Connected,
	Delayed,
	Reconnecting
}