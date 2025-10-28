using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket;

public interface ISocketClient {
    string ClientName { get; }
    event Action<SocketMessage>? MessageReceived;
    event Action<List<string>>? ClientListUpdated;
    event Action? Disconnected;
    event Action<ConnectionState>? HeartbeatStateChanged;
    Task<bool> ConnectAsync(string host, int port, string name);
    void OnUi(string command, Action<List<MessageParam>> handler);
    void OnBackground(string command, Action<List<MessageParam>> handler);

    Task SendToAsync(string target, string command, params object[] args);
    Task BroadcastAsync(string command, params object[] args);
}