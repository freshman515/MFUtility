namespace MFUtility.Core.Bus;

public sealed class Subscription
{
    public Action<object[]> Handler { get; }
    public bool Once { get; }
    public bool OnUiThread { get; } // 参数保留，不再派发 UI

    public Subscription(Action<object[]> handler, bool once, bool onUiThread)
    {
        Handler = handler;
        Once = once;
        OnUiThread = onUiThread;
    }
}
