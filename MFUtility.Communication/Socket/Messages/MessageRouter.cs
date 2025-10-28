using System.Collections.Concurrent;
using System.Windows;
using MFUtility.Communication.Socket.Extensions;

namespace MFUtility.Communication.Socket.Messages;

public class MessageRouter {
    private readonly ConcurrentDictionary<string, Action<List<MessageParam>>> _routes = new();
    /// <summary>
    /// 注册一个命令处理器
    /// </summary>
    public void On(string command, Action<List<MessageParam>> handler) {
        _routes[command] = handler;
    }

    public void Dispatch(SocketMessage msg, bool isUiRouter = false) {
        if (msg.Parameters.Count == 0)
            return;

        var command = msg.GetCommand();
        if (string.IsNullOrEmpty(command))
            return;

        if (_routes.TryGetValue(command, out var handler)) {
            if (!isUiRouter) {
                if (Application.Current != null && Application.Current.Dispatcher.CheckAccess()) {
                    throw new InvalidOperationException(
                        $"检测到在后台路由中执行 UI 线程代码: {command}。\n" +
                        "请改用 OnUi 注册此命令。");
                }
            }

            handler(msg.Parameters.Skip(1).ToList());
        }
    }
}