namespace MFUtility.Bus.Event;

public sealed class Unsubscriber : IDisposable
{
    private readonly Action _dispose;

    public Unsubscriber(Action dispose)
    {
        _dispose = dispose;
    }

    public void Dispose() => _dispose();
}
