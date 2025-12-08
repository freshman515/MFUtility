namespace MFUtility.Common.Helpers;

/// <summary>
/// 文件读写重试机制辅助类。
/// 支持同步与异步，适用于文件占用等暂时性异常。
/// </summary>
internal static class FileRetryHelper
{
    public static void Retry(Action action, int retryCount = 3, int delayMs = 200)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                action();
                return;
            }
            catch (IOException)
            {
                if (i == retryCount - 1)
                    throw;
                Thread.Sleep(delayMs);
            }
        }
    }

    public static async Task RetryAsync(Action action, int retryCount = 3, int delayMs = 200)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                action();
                return;
            }
            catch (IOException)
            {
                if (i == retryCount - 1)
                    throw;
                await Task.Delay(delayMs);
            }
        }
    }
}
