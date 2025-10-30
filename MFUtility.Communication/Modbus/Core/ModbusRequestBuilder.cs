using System;
using System.Net;

namespace MFUtility.Communication.Modbus.Core;

public static class ModbusRequestBuilder
{
    private static ushort _transactionId = 0;

    private static byte[] Header(byte slave, byte func, ushort startAddr, ushort count)
    {
        _transactionId++;
        byte[] msg = new byte[12];
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)_transactionId)), 0, msg, 0, 2);
        msg[2] = msg[3] = 0;
        msg[4] = 0;
        msg[5] = 6;
        msg[6] = slave;
        msg[7] = func;
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddr)), 0, msg, 8, 2);
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)count)), 0, msg, 10, 2);
        return msg;
    }

    // === 读取 ===
    public static byte[] ReadCoils(byte slave, ushort startAddr, ushort count) => Header(slave, 1, startAddr, count);
    public static byte[] ReadDiscreteInputs(byte slave, ushort startAddr, ushort count) => Header(slave, 2, startAddr, count);
    public static byte[] ReadHoldingRegisters(byte slave, ushort startAddr, ushort count) => Header(slave, 3, startAddr, count);
    public static byte[] ReadInputRegisters(byte slave, ushort startAddr, ushort count) => Header(slave, 4, startAddr, count);

    // === 写入 ===
    public static byte[] WriteSingleCoil(byte slave, ushort address, bool value)
    {
        _transactionId++;
        byte[] msg = new byte[12];
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)_transactionId)), 0, msg, 0, 2);
        msg[4] = 0; msg[5] = 6;
        msg[6] = slave; msg[7] = 5;
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)address)), 0, msg, 8, 2);
        Array.Copy(BitConverter.GetBytes((short)(value ? 0xFF00 : 0x0000)), 0, msg, 10, 2);
        return msg;
    }

    public static byte[] WriteSingleRegister(byte slave, ushort address, ushort value)
    {
        _transactionId++;
        byte[] msg = new byte[12];
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)_transactionId)), 0, msg, 0, 2);
        msg[4] = 0; msg[5] = 6;
        msg[6] = slave; msg[7] = 6;
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)address)), 0, msg, 8, 2);
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)value)), 0, msg, 10, 2);
        return msg;
    }

    public static byte[] WriteMultipleRegisters(byte slave, ushort startAddr, ushort[] values)
    {
        ushort byteCount = (ushort)(values.Length * 2);
        ushort len = (ushort)(7 + byteCount);
        _transactionId++;
        byte[] msg = new byte[13 + byteCount];
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)_transactionId)), 0, msg, 0, 2);
        msg[4] = 0; msg[5] = (byte)(len - 6);
        msg[6] = slave; msg[7] = 16;
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddr)), 0, msg, 8, 2);
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)values.Length)), 0, msg, 10, 2);
        msg[12] = (byte)byteCount;
        for (int i = 0; i < values.Length; i++)
        {
            byte[] val = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)values[i]));
            msg[13 + i * 2] = val[0];
            msg[14 + i * 2] = val[1];
        }
        return msg;
    }

    public static byte[] WriteMultipleCoils(byte slave, ushort startAddr, bool[] values)
    {
        int byteCount = (values.Length + 7) / 8;
        ushort len = (ushort)(7 + byteCount);
        _transactionId++;
        byte[] msg = new byte[13 + byteCount];
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)_transactionId)), 0, msg, 0, 2);
        msg[4] = 0; msg[5] = (byte)(len - 6);
        msg[6] = slave; msg[7] = 15;
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddr)), 0, msg, 8, 2);
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)values.Length)), 0, msg, 10, 2);
        msg[12] = (byte)byteCount;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i]) msg[13 + (i / 8)] |= (byte)(1 << (i % 8));
        }

        return msg;
    }
}
