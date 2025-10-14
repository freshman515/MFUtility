using System.Diagnostics;
using System.Windows.Threading;

namespace MFUtility.Extensions;

public static class DispatcherExtensions
{
    public static void InvokeSafe(this Dispatcher dispatcher, Action action)
    {
        try
        {
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DispatcherExtensions.InvokeSafe] {ex.Message}");
        }
    }

    public static async Task InvokeAsyncSafe(this Dispatcher dispatcher, Action action)
    {
        try
        {
            if (dispatcher.CheckAccess())
                action();
            else
                await dispatcher.InvokeAsync(action);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DispatcherExtensions.InvokeAsyncSafe] {ex.Message}");
        }
    }
}