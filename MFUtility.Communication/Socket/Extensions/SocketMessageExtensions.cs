using MFUtility.Communication.Socket.Messages;

namespace MFUtility.Communication.Socket.Extensions;

public static class SocketMessageExtensions {
    /// <summary>
    /// 获取命令字符串（第 0 个参数）
    /// </summary>
    public static string GetCommand(this SocketMessage msg) {
        return msg.Parameters.Count > 0 ? msg.Parameters[0].GetValue<string>() : "";
    }

    /// <summary>
    /// 获取第 n 个参数（从第 1 个起，索引 = 参数位置）
    /// </summary>
    public static T? GetArg<T>(this SocketMessage msg, int index) {
        int actualIndex = index + 1; // index=0 表示参数1
        if (actualIndex >= 0 && actualIndex < msg.Parameters.Count)
            return msg.Parameters[actualIndex].GetValue<T>();
        return default;
    }
    /// <summary>
    /// 设置命令为第一个参数（index = 0）
    /// </summary>
    public static void SetCommand(this SocketMessage msg, string command) {
        if (msg.Parameters.Count == 0)
            msg.Parameters.Add(new MessageParam(command));
        else
            msg.Parameters[0].SetValue(command);
    }

    /// <summary>
    /// 添加一个参数（作为 Args，index >= 1）
    /// </summary>
    public static void AddArg(this SocketMessage msg, object value) {
        msg.Parameters.Add(new MessageParam(value));
    }
}