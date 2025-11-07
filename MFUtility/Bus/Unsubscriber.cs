namespace MFUtility.Bus;

internal sealed class Unsubscriber : IDisposable {
		private readonly Action _unsubscribe;
		private bool _disposed;

		public Unsubscriber(Action unsubscribe) => _unsubscribe = unsubscribe;

		public void Dispose() {
			if (_disposed) return;
			_unsubscribe();
			_disposed = true;
		}
	}