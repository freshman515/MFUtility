using System.IO;
using System.Text;

namespace MFUtility.Communication.Socket.Messages;

public static class MessageProtocol {
	public static byte[] Encode(string message) {
		var bytes = Encoding.UTF8.GetBytes(message);
		var lengthPrefix = BitConverter.GetBytes(bytes.Length);
		return lengthPrefix.Concat(bytes).ToArray();
	}

	public static bool TryDecode(ref MemoryStream buffer, out string message) {
		message = "";
		if (buffer.Length < 4) {
			return false;
		}

		buffer.Position = 0;
		var lengthBytes = new byte[4];
		buffer.Read(lengthBytes, 0, 4);
		int messageLength = BitConverter.ToInt32(lengthBytes, 0);
		if (buffer.Length - 4 < messageLength) {
			buffer.Position = buffer.Length;
			return false;
		}

		var body = new byte[messageLength];
		var bytesRead = buffer.Read(body, 0, messageLength);

		var array = buffer.ToArray();

// 计算剩余长度
		int offset = 4 + messageLength;
		int remainLength = array.Length - offset;

// 如果没有剩余数据就直接清空
		if (remainLength <= 0) {
			buffer = new MemoryStream();
		} else {
			// 用 Array.Copy 复制剩余部分
			var remain = new byte[remainLength];
			Array.Copy(array, offset, remain, 0, remainLength);

			// 重建 buffer
			buffer = new MemoryStream();
			buffer.Write(remain, 0, remain.Length);
		}

		message = Encoding.UTF8.GetString(body, 0, bytesRead);
		return true;
	}
}