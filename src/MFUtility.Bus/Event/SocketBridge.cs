using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;

namespace MFUtility.Bus.Event;

internal sealed class SocketBridge {
		private readonly string _host;
		private readonly int _port;
		private readonly Action<string> _onMessage;
		private readonly CancellationTokenSource _cts = new();
		private readonly Channel<string> _sendQueue = Channel.CreateUnbounded<string>(
			new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });

		private TcpListener? _listener;
		private readonly List<TcpClient> _clients = new();
		private bool _isServer;
		private TcpClient? _clientReceiver;

		public SocketBridge(string ip, Action<string> onMessage) {
			ParseEndpoint(ip, out _host, out _port);
			_onMessage = onMessage;
			Task.Run(InitializeAsync, _cts.Token);
		}

		private async Task InitializeAsync() {
			if (await TryConnectAsync()) {
				_isServer = false;
				Console.WriteLine($"[IPC] 作为客户端连接 {_host}:{_port}");
				Task.Run(ClientReceiverLoop, _cts.Token); // ✅ 客户端启动接收线程
			}
			else {
				_isServer = true;
				Console.WriteLine($"[IPC] 启动服务器监听 {_host}:{_port}");
				Task.Run(ServerLoop, _cts.Token);
			}

			await Task.Delay(200);
			Task.Run(ClientSender, _cts.Token);
		}

		// ==================== 服务端逻辑 ====================
		private async Task ServerLoop() {
			try {
				_listener = new TcpListener(IPAddress.Parse(_host), _port);
				_listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				_listener.Start(100);
				Console.WriteLine($"[IPC] Server started on {_host}:{_port}");
			}
			catch (SocketException ex) {
				Console.WriteLine($"[IPC] 启动监听失败：{ex.Message}");
				return;
			}

			while (!_cts.IsCancellationRequested) {
				try {
					var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
					lock (_clients) _clients.Add(client);
					Console.WriteLine($"[IPC] 客户端连接 ({_clients.Count})");
					_ = Task.Run(() => HandleClient(client), _cts.Token);
				}
				catch { await Task.Delay(100); }
			}
		}

		private async Task HandleClient(TcpClient client) {
			using (client)
			using (var stream = client.GetStream()) {
				var buffer = new byte[4096];
				var sb = new StringBuilder();

				try {
					while (!_cts.IsCancellationRequested) {
						var n = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
						if (n <= 0) break;

						sb.Append(Encoding.UTF8.GetString(buffer, 0, n));
						while (true) {
							var idx = sb.ToString().IndexOf('\n');
							if (idx < 0) break;
							var line = sb.ToString(0, idx).TrimEnd('\r');
							sb.Remove(0, idx + 1);
							if (line.Length == 0) continue;

							_onMessage(line);
							await BroadcastToClientsAsync(line, except: client);
						}
					}
				}
				catch { }
				finally {
					lock (_clients) _clients.Remove(client);
				}
			}
		}

		private async Task BroadcastToClientsAsync(string msg, TcpClient? except = null) {
			byte[] bytes = Encoding.UTF8.GetBytes(msg + "\n");
			List<TcpClient> removeList = new();

			lock (_clients) {
				foreach (var c in _clients.ToArray()) {
					if (c == except) continue;
					try { c.GetStream().WriteAsync(bytes, 0, bytes.Length); }
					catch { removeList.Add(c); }
				}

				foreach (var dead in removeList)
					_clients.Remove(dead);
			}
		}

		// ==================== 客户端逻辑 ====================
		private async Task ClientReceiverLoop() {
			while (!_cts.IsCancellationRequested) {
				try {
					_clientReceiver = new TcpClient();
					await _clientReceiver.ConnectAsync(_host, _port);
					Console.WriteLine("[IPC] 客户端接收线程已连接");
					using var stream = _clientReceiver.GetStream();
					var buffer = new byte[4096];
					var sb = new StringBuilder();

					while (!_cts.IsCancellationRequested) {
						var n = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
						if (n <= 0) break;
						sb.Append(Encoding.UTF8.GetString(buffer, 0, n));

						while (true) {
							var idx = sb.ToString().IndexOf('\n');
							if (idx < 0) break;
							var line = sb.ToString(0, idx).TrimEnd('\r');
							sb.Remove(0, idx + 1);
							if (line.Length == 0) continue;
							_onMessage(line);
						}
					}
				}
				catch {
					await Task.Delay(500); // 自动重连
				}
			}
		}

		private async Task ClientSender() {
			while (await _sendQueue.Reader.WaitToReadAsync(_cts.Token)) {
				var msg = await _sendQueue.Reader.ReadAsync(_cts.Token);
				await SendAsync(msg);
			}
		}

		private async Task<bool> TryConnectAsync() {
			try {
				using var probe = new TcpClient();
				var connectTask = probe.ConnectAsync(_host, _port);
				var finished = await Task.WhenAny(connectTask, Task.Delay(500));
				if (finished != connectTask)
					return false;
				return true;
			}
			catch { return false; }
		}

		private async Task SendAsync(string msg) {
			for (int retry = 0; retry < 3; retry++) {
				TcpClient? client = null;
				try {
					client = new TcpClient();
					client.NoDelay = true;
					await client.ConnectAsync(_host, _port);
					using var stream = client.GetStream();
					var bytes = Encoding.UTF8.GetBytes(msg + "\n");
					await stream.WriteAsync(bytes, 0, bytes.Length);
					await stream.FlushAsync();
					await Task.Delay(100);
					client.Close();
					break;
				}
				catch {
					try { client?.Close(); } catch { }
					await Task.Delay(150);
				}
			}
		}

		public void Broadcast(IpcMessage msg) {
			try {
				var json = JsonConvert.SerializeObject(msg);
				_sendQueue.Writer.TryWrite(json);
			} catch { }
		}

		private static void ParseEndpoint(string text, out string host, out int port) {
			host = "127.0.0.1";
			if (int.TryParse(text, out var p)) { port = p; return; }
			var parts = text.Split(':');
			if (parts.Length == 2 && int.TryParse(parts[1], out var pp)) {
				host = string.IsNullOrWhiteSpace(parts[0]) ? "127.0.0.1" : parts[0];
				port = pp;
				return;
			}
			port = 5055;
		}
	}