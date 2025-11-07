using System.Collections.Concurrent;
using System.Windows.Threading;

namespace MFUtility.Bus;

public sealed class EventScope {
		private readonly Dispatcher _dispatcher;
		private readonly ConcurrentDictionary<string, List<Subscription>> _named = new();
		private readonly ConcurrentDictionary<string, object[]> _sticky = new();

		public EventScope(Dispatcher dispatcher) => _dispatcher = dispatcher;

		// ===================== 删除特定事件的订阅 =====================
		public void RemoveSubscriptions(string eventName) {
			if (_named.TryRemove(eventName, out var subscriptions)) {
				subscriptions.Clear();
			}
		}

		// ===================== 删除所有事件的订阅 =====================
		public void RemoveAllSubscriptions() {
			_named.Clear();
			_sticky.Clear();
		}

		// ===================== 订阅 =====================
		public IDisposable Subscribe(
			string eventName,
			Action<object[]> handler,
			bool once = false,
			bool uiThread = false,
			bool sticky = false) {
			var list = _named.GetOrAdd(eventName, _ => new List<Subscription>());
			var sub = new Subscription(handler, once, uiThread);
			lock (list) list.Add(sub);

			if (sticky && _sticky.TryGetValue(eventName, out var args))
				SafeInvoke(sub, args);

			return new Unsubscriber(() => Remove(sub, eventName));
		}

		public IDisposable Subscribe<T>(string eventName, Action<T> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => handler((T)args[0]), once, uiThread, sticky);

		public IDisposable Subscribe<T1, T2>(string eventName, Action<T1, T2> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => handler((T1)args[0], (T2)args[1]), once, uiThread, sticky);

		public IDisposable Subscribe<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => handler((T1)args[0], (T2)args[1], (T3)args[2]), once, uiThread, sticky);

		public IDisposable Subscribe<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]), once, uiThread, sticky);

		public IDisposable SubscribeEvent<TEvent>(
			Action<TEvent> handler,
			bool once = false,
			bool uiThread = false,
			bool sticky = false) {
			string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
			return Subscribe(eventName, args => {
				if (args.Length > 0 && args[0] is TEvent e)
					handler(e);
			}, once, uiThread, sticky);
		}

		public void PublishEvent<TEvent>(TEvent data) {
			string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
			Publish(eventName, data!);
		}

		public Task PublishEventAsync<TEvent>(TEvent data) {
			string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
			return PublishAsync(eventName, data!);
		}

		public void Publish(string eventName, params object[] args) {
			_sticky[eventName] = args;
			if (!_named.TryGetValue(eventName, out var list)) return;

			List<Subscription> current;
			lock (list) current = list.ToList();

			foreach (var s in current) {
				SafeInvoke(s, args);
				if (s.Once)
					Remove(s, eventName);
			}
		}

		public async Task PublishAsync(string eventName, params object[] args) {
			_sticky[eventName] = args;
			if (!_named.TryGetValue(eventName, out var list)) return;

			List<Subscription> current;
			lock (list) current = list.ToList();

			var tasks = current.Select(s => Task.Run(() => {
				SafeInvoke(s, args);
				if (s.Once)
					Remove(s, eventName);
			}));

			await Task.WhenAll(tasks);
		}

		private void SafeInvoke(Subscription s, object[] args) {
			if (s.OnUiThread && !_dispatcher.CheckAccess())
				_dispatcher.BeginInvoke(new Action(() => ((Action<object[]>)s.Handler)(args)));
			else
				((Action<object[]>)s.Handler)(args);
		}

		private void Remove(Subscription s, string eventName) {
			if (_named.TryGetValue(eventName, out var list)) {
				lock (list) list.Remove(s);
			}
		}
	}