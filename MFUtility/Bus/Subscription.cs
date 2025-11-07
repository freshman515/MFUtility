namespace MFUtility.Bus;

	public sealed class Subscription {
		public Delegate Handler { get; }
		public bool Once { get; }
		public bool OnUiThread { get; }

		public Subscription(Delegate handler, bool once, bool ui) {
			Handler = handler;
			Once = once;
			OnUiThread = ui;
		}
	}