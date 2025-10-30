using System.Collections.Concurrent;
using System.Windows.Threading;

namespace MFUtility.Event {
    /// <summary>
    /// 🌟 EventBus v18（统一版 + 强类型支持）
    /// ✅ 默认作用域与命名作用域（Scope）
    /// ✅ 支持 UIThread / Once / Sticky
    /// ✅ 支持泛型（T / T1~T4）
    /// ✅ ✅ 强类型事件：class / record / struct / record struct
    /// ✅ ✅ EventBus 与 EventScope 都支持 Subscribe<T> / Publish<T>
    /// ✅ 无缝兼容旧字符串写法
    /// </summary>
    public static class EventBus {
        private static readonly ConcurrentDictionary<string, EventScope> _scopes = new();
        private const string _key = "__eventbus_default";
        private static Dispatcher Dispatcher =>
            Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        internal static EventScope GetScope(string? name) =>
            _scopes.GetOrAdd(name ?? _key, _ => new EventScope(Dispatcher));

        public static EventScope Scope(string? name = null) => GetScope(name);

        // ===================== 删除某个事件的订阅 =====================
        public static void RemoveSubscription(string eventName, string? scopeName = null) {
            var scope = GetScope(scopeName);
            scope.RemoveSubscriptions(eventName);
        }

        // ===================== 删除某作用域下所有事件的订阅 =====================
        public static void RemoveScopeSubscriptions(string scopeName = null) {
            var scope = GetScope(scopeName);
            scope.RemoveAllSubscriptions();
        }

        // ===================== 默认作用域（字符串事件） =====================
        public static IDisposable Subscribe(
            string eventName,
            Action<object[]> handler,
            bool once = false,
            bool uiThread = false,
            bool sticky = false)
            => GetScope(_key).Subscribe(eventName, handler, once, uiThread, sticky);

        public static void Publish(string eventName, params object[] args) =>
            GetScope(_key).Publish(eventName, args);

        public static Task PublishAsync(string eventName, params object[] args) =>
            GetScope(_key).PublishAsync(eventName, args);

        // ===================== 泛型支持（默认作用域） =====================
        public static IDisposable Subscribe<T>(
            string eventName,
            Action<T> handler,
            bool once = false,
            bool uiThread = false,
            bool sticky = false)
            => GetScope(_key).Subscribe(eventName, args => handler((T)args[0]), once, uiThread, sticky);

        public static IDisposable Subscribe<T1, T2>(
            string eventName,
            Action<T1, T2> handler,
            bool once = false,
            bool uiThread = false,
            bool sticky = false)
            => GetScope(_key).Subscribe(eventName, args => handler((T1)args[0], (T2)args[1]), once, uiThread, sticky);

        public static IDisposable Subscribe<T1, T2, T3>(
            string eventName,
            Action<T1, T2, T3> handler,
            bool once = false,
            bool uiThread = false,
            bool sticky = false)
            => GetScope(_key).Subscribe(eventName, args =>
                handler((T1)args[0], (T2)args[1], (T3)args[2]), once, uiThread, sticky);

        public static IDisposable Subscribe<T1, T2, T3, T4>(
            string eventName,
            Action<T1, T2, T3, T4> handler,
            bool once = false,
            bool uiThread = false,
            bool sticky = false)
            => GetScope(_key).Subscribe(eventName, args =>
                handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]), once, uiThread, sticky);

        // =====================================================
        // 🚀 新增：强类型事件（class / record / struct）
        // =====================================================
        public static IDisposable Subscribe<TEvent>(
            Action<TEvent> handler,
            bool once = false,
            bool uiThread = false,
            bool sticky = false,
            string? scope = null) {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
            return GetScope(scope).Subscribe(eventName, args => {
                if (args.Length > 0 && args[0] is TEvent e)
                    handler(e);
            }, once, uiThread, sticky);
        }

        public static void Publish<TEvent>(TEvent data, string? scope = null) {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
            GetScope(scope).Publish(eventName, data!);
        }

        public static Task PublishAsync<TEvent>(TEvent data, string? scope = null) {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
            return GetScope(scope).PublishAsync(eventName, data!);
        }
    }

    // =====================================================
    // 🌐 EventScope：可独立作用域容器
    // =====================================================
    public sealed class EventScope {
        private readonly Dispatcher _dispatcher;
        private readonly ConcurrentDictionary<string, List<Subscription>> _named = new();
        private readonly ConcurrentDictionary<string, object[]> _sticky = new();

        public EventScope(Dispatcher dispatcher) => _dispatcher = dispatcher;

        // ===================== 删除特定事件的订阅 =====================
        public void RemoveSubscriptions(string eventName) {
            if (_named.TryRemove(eventName, out var subscriptions)) {
                // 清除事件的所有订阅
                subscriptions.Clear();
            }
        }

        // ===================== 删除所有事件的订阅 =====================
        public void RemoveAllSubscriptions() {
            // 清除当前作用域中的所有订阅
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

        // 泛型版本（1~4 参数）
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

        // =====================================================
        // 🚀 新增：强类型事件（与 EventBus 一致）
        // =====================================================
        public IDisposable Subscribe<TEvent>(
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

        public void Publish<TEvent>(TEvent data) {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
            Publish(eventName, data!);
        }

        public Task PublishAsync<TEvent>(TEvent data) {
            string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
            return PublishAsync(eventName, data!);
        }

        // ===================== 发布 =====================
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

        // ===================== 内部工具 =====================
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

    // =====================================================
    // 🌱 Subscription & Unsubscriber
    // =====================================================
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
