using System.Windows;
using MFUtility.Communication.Socket.Extensions;
using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket;

public class SocketClientApi :ISocketClient{
    private readonly SocketClient _client;
    private readonly MessageRouter _uiRouter = new();
    private readonly MessageRouter _backgroundRouter = new();
    public string ClientName => _client.ClientName!;
    public event Action<SocketMessage>? MessageReceived;
    public event Action<List<string>>? ClientListUpdated;
    public event Action? Disconnected;
    public event Action<ConnectionState>? HeartbeatStateChanged;


    public SocketClientApi() {
        _client = new SocketClient();
        _client.MessageReceived += msg => {
            MessageReceived?.Invoke(msg);
            _backgroundRouter.Dispatch(msg,false);
            if (Application.Current != null) {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { _uiRouter.Dispatch(msg,true); }));
            }
        };

        _client.ClientListUpdated += list => { ClientListUpdated?.Invoke(list); };

        _client.Disconnected += () => { Disconnected?.Invoke(); };

        _client.HeartbeatStateChanged += state => { HeartbeatStateChanged?.Invoke(state); };
    }
    public async Task<bool> ConnectAsync(string host, int port, string name) {
        return await _client.ConnectAsync(host, port, name);
    }
    public void OnUi(string command, Action<List<MessageParam>> handler) =>
        _uiRouter.On(command, handler);

    public void OnBackground(string command, Action<List<MessageParam>> handler) =>
        _backgroundRouter.On(command, handler);

    public async Task SendToAsync(string target, string command, params object[] args) {
        var msg = new SocketMessage {
            Sender = ClientName,
            Target = target
        };
        msg.SetCommand(command);
        foreach (var arg in args)
            msg.AddArg(arg);
        await _client.SendAsync(msg);
    }
    public async Task BroadcastAsync(string command, params object[] args) {
        var msg = new SocketMessage {
            Sender = ClientName,
            Target = "All"
        };
        msg.SetCommand(command);
        foreach (var arg in args)
            msg.AddArg(arg);
        await _client.SendAsync(msg);
    }

}