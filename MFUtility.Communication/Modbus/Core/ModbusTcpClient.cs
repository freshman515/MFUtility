using System.Net.Sockets;
using MFUtility.Communication.Modbus.Helpers;
using MFUtility.Communication.Modbus.Interfaces;

namespace MFUtility.Communication.Modbus.Core;

public class ModbusTcpClient : IModbusClient {
	private TcpClient _client;
	private NetworkStream _stream;
	private CancellationTokenSource _cts;
	private Task _monitorTask;

	public int Id { get; }
	public string Ip { get; }
	public int Port { get; }
	public bool IsConnected => _client?.Connected == true;
	public event Action<int, bool> ConnectionChanged;

	public ModbusTcpClient(int id, string ip, int port) {
		Id = id;
		Ip = ip;
		Port = port;
	}

	public async Task<bool> ConnectAsync() {
		try {
			_client = new TcpClient();
			var connectTask = _client.ConnectAsync(Ip, Port);
			if (await Task.WhenAny(connectTask, Task.Delay(2000)) != connectTask)
				return false;

			_stream = _client.GetStream();
			ConnectionChanged?.Invoke(Id, true);
			StartMonitor();
			return true;
		} catch {
			return false;
		}
	}

	public async Task DisconnectAsync() {
		_cts?.Cancel();
		await Task.Delay(100);
		_stream?.Close();
		_client?.Close();
		ConnectionChanged?.Invoke(Id, false);
	}

	public void Dispose() => _ = DisconnectAsync();

	private void StartMonitor() {
		_cts?.Cancel();
		_cts = new CancellationTokenSource();
		_monitorTask = Task.Run(async () => {
			while (!_cts.Token.IsCancellationRequested) {
				if (!IsConnected) {
					ConnectionChanged?.Invoke(Id, false);
					await Task.Delay(1000);
					await ConnectAsync();
				}
				await Task.Delay(1000);
			}
		});
	}

	private async Task<byte[]> ReadResponseAsync(int size) {
		var buffer = new byte[size];
		int offset = 0;
		while (offset < size) {
			int read = await _stream.ReadAsync(buffer, offset, size - offset);
			if (read == 0) return null;
			offset += read;
		}
		return buffer;
	}

	// === 读取部分 ===
	public async Task<bool[]> ReadCoilsAsync(int startAddress, int count) {
		byte[] frame = ModbusRequestBuilder.ReadCoils(1, (ushort)startAddress, (ushort)count);
		await _stream.WriteAsync(frame, 0, frame.Length);
		byte[] response = await ReadResponseAsync(9 + (count + 7) / 8);
		if (response == null) return null;

		int dataStart = 9;
		int bitCount = count;
		bool[] result = new bool[count];
		for (int i = 0; i < count; i++) {
			int byteIndex = dataStart + i / 8;
			int bitIndex = i % 8;
			result[i] = (response[byteIndex] & (1 << bitIndex)) != 0;
		}
		return result;
	}

	public async Task<bool[]> ReadDiscreteInputsAsync(int startAddress, int count) {
		byte[] frame = ModbusRequestBuilder.ReadDiscreteInputs(1, (ushort)startAddress, (ushort)count);
		await _stream.WriteAsync(frame, 0, frame.Length);
		byte[] response = await ReadResponseAsync(9 + (count + 7) / 8);
		if (response == null) return null;

		int dataStart = 9;
		bool[] result = new bool[count];
		for (int i = 0; i < count; i++) {
			int byteIndex = dataStart + i / 8;
			int bitIndex = i % 8;
			result[i] = (response[byteIndex] & (1 << bitIndex)) != 0;
		}
		return result;
	}

	public async Task<ushort[]> ReadHoldingRegistersAsync(int startAddress, int count) {
		byte[] frame = ModbusRequestBuilder.ReadHoldingRegisters(1, (ushort)startAddress, (ushort)count);
		await _stream.WriteAsync(frame, 0, frame.Length);
		byte[] response = await ReadResponseAsync(9 + count * 2);
		if (response == null) return null;

		ushort[] result = new ushort[count];
		for (int i = 0; i < count; i++) {
			result[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
		}
		return result;
	}

	public async Task<ushort[]> ReadInputRegistersAsync(int startAddress, int count) {
		byte[] frame = ModbusRequestBuilder.ReadInputRegisters(1, (ushort)startAddress, (ushort)count);
		await _stream.WriteAsync(frame, 0, frame.Length);
		byte[] response = await ReadResponseAsync(9 + count * 2);
		if (response == null) return null;

		ushort[] result = new ushort[count];
		for (int i = 0; i < count; i++) {
			result[i] = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
		}
		return result;
	}

	public async Task<float?> ReadFloatAsync(int address, bool bigEndian = true) {
		var regs = await ReadHoldingRegistersAsync(address, 2);
		if (regs == null) return null;

		byte[] bytes = {
			(byte)(regs[0] >> 8), (byte)regs[0],
			(byte)(regs[1] >> 8), (byte)regs[1]
		};

		return BitConverter.ToSingle(ByteHelper.ToEndian(bytes, bigEndian), 0);
	}

	public async Task<int?> ReadIntAsync(int address, bool bigEndian = true) {
		var regs = await ReadHoldingRegistersAsync(address, 2);
		if (regs == null) return null;

		byte[] bytes = {
			(byte)(regs[0] >> 8), (byte)regs[0],
			(byte)(regs[1] >> 8), (byte)regs[1]
		};
		return BitConverter.ToInt32(ByteHelper.ToEndian(bytes, bigEndian), 0);
	}

	// === 写入部分 ===
	public async Task<bool> WriteSingleCoilAsync(int address, bool value) {
		byte[] frame = ModbusRequestBuilder.WriteSingleCoil(1, (ushort)address, value);
		await _stream.WriteAsync(frame, 0, frame.Length);
		var resp = await ReadResponseAsync(12);
		return resp != null;
	}

	public async Task<bool> WriteSingleRegisterAsync(int address, ushort value) {
		byte[] frame = ModbusRequestBuilder.WriteSingleRegister(1, (ushort)address, value);
		await _stream.WriteAsync(frame, 0, frame.Length);
		var resp = await ReadResponseAsync(12);
		return resp != null;
	}

	public async Task<bool> WriteMultipleCoilsAsync(int startAddress, bool[] values) {
		byte[] frame = ModbusRequestBuilder.WriteMultipleCoils(1, (ushort)startAddress, values);
		await _stream.WriteAsync(frame, 0, frame.Length);
		var resp = await ReadResponseAsync(12);
		return resp != null;
	}

	public async Task<bool> WriteMultipleRegistersAsync(int startAddress, ushort[] values) {
		byte[] frame = ModbusRequestBuilder.WriteMultipleRegisters(1, (ushort)startAddress, values);
		await _stream.WriteAsync(frame, 0, frame.Length);
		var resp = await ReadResponseAsync(12);
		return resp != null;
	}
}