using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MFUtility.Communication.Socket.Extensions;
using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket;

public class SocketServer {
	private TcpListener? _listener;
	private bool _isRunning;
	private ConcurrentDictionary<string, TcpClient> _clients = new();
	public event Action<SocketMessage>? MessageReceived;
	public event Action<string>? ClientConnected;
	public event Action<string>? ClientDisConnected;

	public SocketServer(int port) {
		_listener = new TcpListener(IPAddress.Any, port);
	}

	public void Start() {
		_isRunning = true;
		_listener.Start();

		Console.WriteLine("Socket Server started...");
		Task.Run(AcceptLoop);
	}

	private async Task? AcceptLoop() {
		while (_isRunning) {
			var client = await _listener?.AcceptTcpClientAsync();
			_ = Task.Run(() => HandleClientAsync(client));
		}
	}

	private async Task? HandleClientAsync(TcpClient client) {
		using var stream = client.GetStream();
		var buffer = new byte[1024];
		var ms = new MemoryStream();
		var bytesRead = 0;
		string? clientName = null;

		try {
			while (_isRunning) {
				bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				if (bytesRead == 0)
					break;
				ms.Write(buffer, 0, bytesRead);
				while (MessageProtocol.TryDecode(ref ms, out var message)) {
					var sm = SocketMessage.Deserialize(message);
					var cmd = sm.GetCommand();
					if (cmd == "Ping")
						Send(new SocketMessage("Server", clientName, "Pong"));
					else if (cmd == "Register") {
						clientName = sm.Sender;
						if (_clients.TryAdd(clientName, client)) {
							Console.WriteLine($"[Server] {clientName} 注册成功");
							ClientConnected?.Invoke(clientName);
							BroadcastClientList();
						}
					} else if (cmd == "Chat") {
						Send(sm);
						//MessageReceived?.Invoke(sm);
					} else {
						MessageReceived?.Invoke(sm);
					}
				}
			}
		} finally {
			_clients.TryRemove(clientName, out _);
			if (clientName != null) ClientDisConnected?.Invoke(clientName);
			try {
				client.Dispose(); // 用 Dispose 更安全
			} catch {
			}
		}
	}

	private void BroadcastClientList() {
		var list = string.Join(";", _clients.Keys);
		var msg = new SocketMessage("Server", "All"); // ✅ 使用参数传 list 字符串
		msg.SetCommand("ClientList");
		msg.AddArg(list);
		Send(msg);
	}

	public void Send(SocketMessage message) {
		var data = MessageProtocol.Encode(SocketMessage.Serialize(message));
		if (message.Target != "All") {
			if (!_clients.TryGetValue(message.Target, out var client)) {
				if (message.Target == "Server") {
					MessageReceived?.Invoke(message);
				}

				return;
			}

			if (message.Target != "All") {
				client.GetStream().Write(data, 0, data.Length);
			}

			return;
		}

		foreach (var entry in _clients.Values) {
			entry.GetStream().Write(data, 0, data.Length);
		}
	}

	public void Broadcast(string msg) {
		Send(new SocketMessage("Server", "All", "Chat", msg));
	}

	public void Stop() {
		_isRunning = false;
		_listener?.Stop();
	}
}