using System.Windows;
using MFUtility.Communication.Socket.Extensions;
using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket;

public class SocketServerApi :ISocketServer{
    private readonly SocketServer _server;
    private readonly MessageRouter _backgroundRouter = new();
    private readonly MessageRouter _uiRouter = new();

    public event Action<SocketMessage>? MessageReceived;
    public event Action<string>? ClientConnected;
    public event Action<string>? ClientDisconnected;

    public SocketServerApi(int port) {
        _server = new SocketServer(port);


        _server.MessageReceived += sm => {
            MessageReceived?.Invoke(sm);
            _backgroundRouter.Dispatch(sm, false);
            if (Application.Current != null) {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    _uiRouter.Dispatch(sm, true);
                }));
            }
        };
        _server.ClientConnected += name => ClientConnected?.Invoke(name);
        _server.ClientDisConnected += name => ClientDisconnected?.Invoke(name);
    }

    public void Start() => _server.Start();


    public void Stop() => _server.Stop();


    public void SendTo(string target, string command, params object[] args) {
        var msg = new SocketMessage("Server", target);
        msg.SetCommand(command);
        foreach (var arg in args)
            msg.AddArg(arg);
        _server.Send(msg);
    }


    public void Broadcast(string command, params object[] args) {
        var msg = new SocketMessage("Server", "All");
        msg.SetCommand(command);
        foreach (var arg in args)
            msg.AddArg(arg);
        _server.Send(msg);
    }


    public void OnBackground(string command, Action<List<MessageParam>> handler) {
        _backgroundRouter.On(command, handler);
    }


    public void OnUi(string command, Action<List<MessageParam>> handler) {
        _uiRouter.On(command, handler);
    }
}