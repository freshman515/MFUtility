using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using MFUtility.Communication.Socket.Extensions;
using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket {
	public class SocketClient {
		private TcpClient? _client;
		private string _host;
		private int _port;
		private bool _isRunning;
		public event Action<SocketMessage>? MessageReceived;
		public event Action<ConnectionState>? HeartbeatStateChanged;
		public event Action? Disconnected;
		private CancellationTokenSource? _cts;
		private DateTime _lastPong;

		public int MaxReconnectCount = 6;

		//public DateTime ReconnectInterval = 2000;
		public string? ClientName { get; private set; }
		public List<string> OnlineClients { get; private set; } = new();
		public event Action<List<string>>? ClientListUpdated;

		public async Task<bool> ConnectAsync(string host, int port, string name) {
			try {
				_client = new TcpClient();
				_host = host;
				_port = port;

				await _client.ConnectAsync(host, port);

				if (!_client.Connected)
					return false;

				_cts = new CancellationTokenSource();
				_isRunning = true;
				ClientName = name;
				_lastPong = DateTime.UtcNow;

				await SendAsync(new SocketMessage(ClientName, "Server", "Register"));

				_ = Task.Run(() => ReceiveLoop(_cts.Token));
				_ = Task.Run(() => HeartbeatLoop(_cts.Token));

				return true;
			} catch (Exception ex) {
				// ✅ 不抛异常，只记录或忽略
				Debug.WriteLine($"连接失败: {ex.Message}");
				_isRunning = false;
				_client?.Close();
				return false;
			}
		}

		private async Task ReceiveLoop(CancellationToken token) {
			var buffer = new byte[1024];
			var ms = new MemoryStream();

			try {
				while (_isRunning && !token.IsCancellationRequested) {
					int bytesRead = await _client!.GetStream().ReadAsync(buffer, 0, buffer.Length, token);
					if (bytesRead == 0) {
						Console.WriteLine("[Client] 远端关闭连接");
						break; // 正常断开
					}

					ms.Write(buffer, 0, bytesRead);
					while (MessageProtocol.TryDecode(ref ms, out var msg)) {
						var sm = SocketMessage.Deserialize(msg);
						if (sm == null)
							continue;
						var cmd = sm.GetCommand();
						if (cmd == "Pong") {
							_lastPong = DateTime.UtcNow;
						} else if (cmd == "Chat") {
							MessageReceived?.Invoke(sm);
						} else if (cmd == "ClientList") {
							OnlineClients.Clear();
							OnlineClients.AddRange(sm.GetArg<string>(0)?.Split(';') ?? Array.Empty<string>());
							ClientListUpdated?.Invoke(OnlineClients);
						} else {
							MessageReceived?.Invoke(sm);
						}
					}
				}
			} catch (Exception ex) {
				Console.WriteLine($"[Client] 接收异常: {ex.Message}");
			} finally {
				HandleDisconnect();
			}
		}

		private async Task? HeartbeatLoop(CancellationToken token) {
			while (!token.IsCancellationRequested) {
				await Task.Delay(2000, token);
				if (_client == null)
					break;
				await SendAsync(new SocketMessage(ClientName!, "Server", "Ping"));
				var elapsed = (DateTime.UtcNow - _lastPong).TotalSeconds;

				if (elapsed > 6) {
					HeartbeatStateChanged?.Invoke(ConnectionState.Reconnecting);
					HandleDisconnect();
					await TryReconnectedAsync();
					return;
				}

				if (elapsed > 3) {
					HeartbeatStateChanged?.Invoke(ConnectionState.Delayed);
				} else {
					HeartbeatStateChanged?.Invoke(ConnectionState.Connected);
				}
			}
		}

		private async Task TryReconnectedAsync() {
			int retry = 0;
			while (!_isRunning && retry < MaxReconnectCount) {
				retry++;
				try {
					await ConnectAsync(_host, _port, ClientName!);
					return;
				} catch {
					await Task.Delay(2000);
				}
			}
		}

		public async Task SendAsync(SocketMessage? message) {
			if (_client == null || message == null)
				return;
			var data = MessageProtocol.Encode(SocketMessage.Serialize(message));
			await _client.GetStream().WriteAsync(data, 0, data.Length);
		}

		public async Task BroadcastAsync(string message) {
			var msg = new SocketMessage {
				Sender = ClientName!,
				Target = "All"
			};
			msg.SetCommand("Chat");
			msg.AddArg(msg);
			await SendAsync(msg);
		}

		private void HandleDisconnect() {
			if (_isRunning) {
				_isRunning = false;
				_cts?.Cancel();
				Disconnected?.Invoke();
			}
		}
	}
}