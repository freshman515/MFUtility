using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MFUtility.Communication.Event {
	/// <summary>
	/// 🌟 EventBus Ultimate v3
	/// 功能：
	/// ✅ 泛型事件（强类型 T）
	/// ✅ 命名事件（字符串）
	/// ✅ 多参数事件（T1~T4）
	/// ✅ 支持 Once / UIThread / Sticky
	/// ✅ 异步 PublishAsync
	/// ✅ Scope 模块隔离
	/// ✅ 全局清理、状态检查
	/// ✅ WeakReference 自动释放订阅对象
	/// </summary>
	public static class EventBus {
		private static readonly ConcurrentDictionary<string, EventScope> _scopes = new();
		private static readonly Dispatcher? _dispatcher = Application.Current?.Dispatcher;

		internal static EventScope GetScope(string? name) =>
			_scopes.GetOrAdd(name ?? "default", _ => new EventScope(_dispatcher));

		// ===================== Scope访问 =====================
		public static EventScope Scope(string? name = null) => GetScope(name);

		// ===================== 泛型事件 =====================
		public static IDisposable Subscribe<T>(
			Action<T> handler,
			object? subscriber = null,
			bool once = false,
			bool uiThread = false,
			bool sticky = false,
			string? scope = null)
			=> GetScope(scope).Subscribe(handler, subscriber, once, uiThread, sticky);

		public static void Publish<T>(T ev, string? scope = null) =>
			GetScope(scope).Publish(ev);

		public static Task PublishAsync<T>(T ev, string? scope = null, CancellationToken token = default) =>
			GetScope(scope).PublishAsync(ev, token);

		// ===================== 命名事件 =====================
		public static IDisposable Subscribe(
			string eventName,
			Action<object[]> handler,
			object? subscriber = null,
			bool once = false,
			bool uiThread = false,
			bool sticky = false,
			string? scope = null)
			=> GetScope(scope).Subscribe(eventName, handler, subscriber, once, uiThread, sticky);

		public static void Publish(string eventName, params object[] args) =>
			GetScope(null).Publish(eventName, args);

		public static Task PublishAsync(string eventName, params object[] args) =>
			GetScope(null).PublishAsync(eventName, args);

		// ===================== 状态管理 =====================
		public static void ClearScope(string? scope = null) =>
			GetScope(scope).Clear();

		public static void ClearAllScopes() {
			foreach (var kv in _scopes)
				kv.Value.Clear();
			_scopes.Clear();
		}

		public static bool Exists(string eventName, string? scope = null) =>
			GetScope(scope).Exists(eventName);

		public static bool HasSticky(string eventName, string? scope = null) =>
			GetScope(scope).HasSticky(eventName);

		public static void RemoveSticky(string eventName, string? scope = null) =>
			GetScope(scope).RemoveSticky(eventName);

		// ===================== 强类型命名事件（T1~T4） =====================
		public static IDisposable Subscribe<T1>(
			string eventName,
			Action<T1> handler,
			bool once = false,
			bool uiThread = false,
			bool sticky = false,
			string? scope = null)
			=> GetScope(scope).Subscribe(eventName, handler, once, uiThread, sticky);

		public static IDisposable Subscribe<T1, T2>(
			string eventName,
			Action<T1, T2> handler,
			bool once = false,
			bool uiThread = false,
			bool sticky = false,
			string? scope = null)
			=> GetScope(scope).Subscribe(eventName, handler, once, uiThread, sticky);

		public static IDisposable Subscribe<T1, T2, T3>(
			string eventName,
			Action<T1, T2, T3> handler,
			bool once = false,
			bool uiThread = false,
			bool sticky = false,
			string? scope = null)
			=> GetScope(scope).Subscribe(eventName, handler, once, uiThread, sticky);

		public static IDisposable Subscribe<T1, T2, T3, T4>(
			string eventName,
			Action<T1, T2, T3, T4> handler,
			bool once = false,
			bool uiThread = false,
			bool sticky = false,
			string? scope = null)
			=> GetScope(scope).Subscribe(eventName, handler, once, uiThread, sticky);

		public static void Publish<T1>(string eventName, T1 arg1, string? scope = null) =>
			GetScope(scope).Publish(eventName, arg1);

		public static void Publish<T1, T2>(string eventName, T1 arg1, T2 arg2, string? scope = null) =>
			GetScope(scope).Publish(eventName, arg1, arg2);

		public static void Publish<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3, string? scope = null) =>
			GetScope(scope).Publish(eventName, arg1, arg2, arg3);

		public static void Publish<T1, T2, T3, T4>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4, string? scope = null) =>
			GetScope(scope).Publish(eventName, arg1, arg2, arg3, arg4);
	}

	// =====================================================
	// 🌐 EventScope：模块独立事件域
	// =====================================================
	public sealed class EventScope {
		private readonly Dispatcher? _dispatcher;
		private readonly ReaderWriterLockSlim _lock = new();
		private readonly ConcurrentDictionary<Type, List<Subscription>> _typed = new();
		private readonly ConcurrentDictionary<string, List<Subscription>> _named = new();
		private readonly ConcurrentDictionary<Type, object> _stickyTyped = new();
		private readonly ConcurrentDictionary<string, object[]> _stickyNamed = new();

		public EventScope(Dispatcher? dispatcher) => _dispatcher = dispatcher;

		// ========== 泛型事件 ==========
		public IDisposable Subscribe<T>(
			Action<T> handler, object? subscriber = null, bool once = false,
			bool uiThread = false, bool sticky = false) {
			var list = _typed.GetOrAdd(typeof(T), _ => new List<Subscription>());
			var sub = new Subscription(subscriber ?? handler.Target ?? new object(), handler, once, uiThread);

			lock (list) list.Add(sub);
			if (sticky && _stickyTyped.TryGetValue(typeof(T), out var last) && last is T t)
				SafeInvoke(sub, t);

			return new Unsubscriber(() => Remove(sub, typeof(T), null));
		}

		public void Publish<T>(T ev) {
			_stickyTyped[typeof(T)] = ev;
			if (!_typed.TryGetValue(typeof(T), out var list)) return;
			foreach (var s in list.ToList()) {
				if (!s.Target.IsAlive) {
					list.Remove(s);
					continue;
				}
				SafeInvoke(s, ev);
				if (s.Once) list.Remove(s);
			}
		}

		public async Task PublishAsync<T>(T ev, CancellationToken token = default) {
			_stickyTyped[typeof(T)] = ev;
			if (!_typed.TryGetValue(typeof(T), out var list)) return;
			var tasks = list.Where(s => s.Target.IsAlive)
				.Select(s => Task.Run(() => SafeInvoke(s, ev), token));
			await Task.WhenAll(tasks);
		}

		// ========== 命名事件 ==========
		public IDisposable Subscribe(
			string name, Action<object[]> handler,
			object? subscriber = null, bool once = false,
			bool uiThread = false, bool sticky = false) {
			var list = _named.GetOrAdd(name, _ => new List<Subscription>());
			var sub = new Subscription(subscriber ?? handler.Target ?? new object(), handler, once, uiThread);

			lock (list) list.Add(sub);
			if (sticky && _stickyNamed.TryGetValue(name, out var args))
				SafeInvoke(sub, args);

			return new Unsubscriber(() => Remove(sub, null, name));
		}

		public void Publish(string name, params object[] args) {
			_stickyNamed[name] = args;
			if (!_named.TryGetValue(name, out var list)) return;
			foreach (var s in list.ToList()) {
				if (!s.Target.IsAlive) {
					list.Remove(s);
					continue;
				}
				SafeInvoke(s, args);
				if (s.Once) list.Remove(s);
			}
		}

		public async Task PublishAsync(string name, params object[] args) {
			_stickyNamed[name] = args;
			if (!_named.TryGetValue(name, out var list)) return;
			var tasks = list.Where(s => s.Target.IsAlive)
				.Select(s => Task.Run(() => SafeInvoke(s, args)));
			await Task.WhenAll(tasks);
		}

		// ========== 强类型命名事件（T1~T4） ==========
		public IDisposable Subscribe<T1>(
			string eventName, Action<T1> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => {
				if (args.Length >= 1) handler((T1)args[0]);
			}, null, once, uiThread, sticky);

		public IDisposable Subscribe<T1, T2>(
			string eventName, Action<T1, T2> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => {
				if (args.Length >= 2) handler((T1)args[0], (T2)args[1]);
			}, null, once, uiThread, sticky);

		public IDisposable Subscribe<T1, T2, T3>(
			string eventName, Action<T1, T2, T3> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => {
				if (args.Length >= 3) handler((T1)args[0], (T2)args[1], (T3)args[2]);
			}, null, once, uiThread, sticky);

		public IDisposable Subscribe<T1, T2, T3, T4>(
			string eventName, Action<T1, T2, T3, T4> handler,
			bool once = false, bool uiThread = false, bool sticky = false)
			=> Subscribe(eventName, args => {
				if (args.Length >= 4) handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]);
			}, null, once, uiThread, sticky);

		public void Publish<T1>(string eventName, T1 arg1) =>
			Publish(eventName, new object[] { arg1 });

		public void Publish<T1, T2>(string eventName, T1 arg1, T2 arg2) =>
			Publish(eventName, new object[] { arg1, arg2 });

		public void Publish<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3) =>
			Publish(eventName, new object[] { arg1, arg2, arg3 });

		public void Publish<T1, T2, T3, T4>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4) =>
			Publish(eventName, new object[] { arg1, arg2, arg3, arg4 });

		// ========== 状态与清理 ==========
		public bool Exists(string eventName) => _named.ContainsKey(eventName);
		public bool HasSticky(string eventName) => _stickyNamed.ContainsKey(eventName);
		public void RemoveSticky(string eventName) => _stickyNamed.TryRemove(eventName, out _);
		public void Clear() {
			_typed.Clear();
			_named.Clear();
			_stickyTyped.Clear();
			_stickyNamed.Clear();
		}

		// ========== 安全调用 ==========
		private void SafeInvoke<T>(Subscription s, T arg) {
			if (s.OnUiThread && _dispatcher != null && !_dispatcher.CheckAccess())
				_dispatcher.BeginInvoke(new Action(() => ((Action<T>)s.Handler)(arg)));
			else
				((Action<T>)s.Handler)(arg);
		}

		private void SafeInvoke(Subscription s, object[] args) {
			if (s.Handler is not Action<object[]> h) return;
			if (s.OnUiThread && _dispatcher != null && !_dispatcher.CheckAccess())
				_dispatcher.BeginInvoke(new Action(() => h(args)));
			else
				h(args);
		}

		private void Remove(Subscription sub, Type? type, string? name) {
			if (type != null && _typed.TryGetValue(type, out var list))
				lock (list)
					list.Remove(sub);
			if (name != null && _named.TryGetValue(name, out var list2))
				lock (list2)
					list2.Remove(sub);
		}
	}

	/// <summary> 单个订阅项：弱引用 + 执行选项 </summary>
	public sealed class Subscription {
		public WeakReference Target { get; }
		public Delegate Handler { get; }
		public bool Once { get; }
		public bool OnUiThread { get; }

		public Subscription(object target, Delegate handler, bool once, bool ui) {
			Target = new WeakReference(target);
			Handler = handler;
			Once = once;
			OnUiThread = ui;
		}
	}

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
}