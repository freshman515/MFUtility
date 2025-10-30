using System.Collections.Concurrent;
using MFUtility.Communication.Modbus.Interfaces;

namespace MFUtility.Communication.Modbus.Manager;

public class ModbusManager {
	private readonly ConcurrentDictionary<int, IModbusClient> _clients = new();

	public IModbusClient GetOrCreate(int id, string ip, int port) {
		return _clients.GetOrAdd(id, _ => new Core.ModbusTcpClient(id, ip, port));
	}

	public void StopAll() {
		foreach (var client in _clients.Values)
			client.Dispose();
		_clients.Clear();
	}
}