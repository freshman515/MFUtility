using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket;

public interface ISocketServer {
    event Action<SocketMessage>? MessageReceived;
    event Action<string>? ClientConnected;
    event Action<string>? ClientDisconnected;

    void Start();
    void Stop();

    void SendTo(string target, string command, params object[] args);
    void Broadcast(string command, params object[] args);

    void OnBackground(string command, Action<List<MessageParam>> handler);
    void OnUi(string command, Action<List<MessageParam>> handler);
}