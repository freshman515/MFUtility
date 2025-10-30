namespace MFUtility.Communication.Modbus.Helpers;

public static class ByteHelper
{
    public static byte[] ToEndian(byte[] bytes, bool bigEndian)
    {
        if (BitConverter.IsLittleEndian && bigEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }
}