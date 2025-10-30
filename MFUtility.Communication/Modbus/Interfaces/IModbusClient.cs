namespace MFUtility.Communication.Modbus.Interfaces;

public interface IModbusClient : IDisposable
{
    int Id { get; }
    bool IsConnected { get; }
    Task<bool> ConnectAsync();
    Task DisconnectAsync();

    // === 读取 ===
    Task<bool[]> ReadCoilsAsync(int startAddress, int count);
    Task<bool[]> ReadDiscreteInputsAsync(int startAddress, int count);
    Task<ushort[]> ReadHoldingRegistersAsync(int startAddress, int count);
    Task<ushort[]> ReadInputRegistersAsync(int startAddress, int count);

    Task<float?> ReadFloatAsync(int address, bool bigEndian = true);
    Task<int?> ReadIntAsync(int address, bool bigEndian = true);

    // === 写入 ===
    Task<bool> WriteSingleCoilAsync(int address, bool value);
    Task<bool> WriteSingleRegisterAsync(int address, ushort value);
    Task<bool> WriteMultipleCoilsAsync(int startAddress, bool[] values);
    Task<bool> WriteMultipleRegistersAsync(int startAddress, ushort[] values);

    event Action<int, bool> ConnectionChanged;
}