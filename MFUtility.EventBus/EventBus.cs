using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace MFUtility.EventBus;

public static class EventBus {
	private static readonly ConcurrentDictionary<string, EventScope> _scopes = new();
	private const string _key = "__eventbus_default";

	private static SocketBridge? _ipc;

	internal static EventScope GetScope(string? name) =>
		_scopes.GetOrAdd(name ?? _key, _ => new EventScope());

	public static EventScope Scope(string? name = null) => GetScope(name);

	// ===================== IPC =====================
	public static void EnableRemote(string ip) {
		_ipc ??= new SocketBridge(ip, OnIpcMessageReceived);
	}

	private static void OnIpcMessageReceived(string json) {
		try {
			var msg = JsonConvert.DeserializeObject<IpcMessage>(json);
			if (msg == null) return;
			GetScope(msg.Scope).Publish(msg.MessageName, msg.Args ?? Array.Empty<object>());
		} catch { }
	}

	public static void RemoveSubscription(string eventName, string? scopeName = null) =>
		GetScope(scopeName).RemoveSubscriptions(eventName);

	public static void RemoveScopeSubscriptions(string? scopeName = null) =>
		GetScope(scopeName).RemoveAllSubscriptions();

	// ===================== 发布 =====================
	public static void Publish(string eventName, params object[] args) =>
		GetScope(_key).Publish(eventName, args);

	public static void PublishRemote(string eventName, params object[] args) =>
		_ipc?.Broadcast(new IpcMessage(_key, eventName, args));

	public static void PublishAll(string eventName, params object[] args) {
		Publish(eventName, args);
		PublishRemote(eventName, args);
	}

	public static Task PublishAsync(string eventName, params object[] args) =>
		GetScope(_key).PublishAsync(eventName, args);

	public static Task PublishRemoteAsync(string eventName, params object[] args) {
		PublishRemote(eventName, args);
		return Task.CompletedTask;
	}

	public static async Task PublishAllAsync(string eventName, params object[] args) {
		await PublishAsync(eventName, args);
		PublishRemote(eventName, args);
	}

	// ===================== 泛型订阅 =====================
	public static IDisposable SubscribeEvent<TEvent>(
		Action<TEvent> handler,
		bool once = false,
		bool uiThread = false,
		bool sticky = false) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;

		return GetScope(_key).Subscribe(eventName, args => {
			if (args.Length > 0 && args[0] is TEvent evt)
				handler(evt);
		}, once, uiThread, sticky);
	}


	// ===================== 字符串事件订阅 =====================
	public static IDisposable Subscribe(string eventName, Action handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, _ => handler(), once, uiThread, sticky);

	public static IDisposable Subscribe(string eventName, Action<object[]> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, handler, once, uiThread, sticky);

// ===================== 泛型订阅（T） =====================
	public static IDisposable Subscribe<T>(string eventName, Action<T> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args => handler((T)args[0]), once, uiThread, sticky);


// ===================== 泛型订阅（T1, T2） =====================
	public static IDisposable Subscribe<T1, T2>(string eventName, Action<T1, T2> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args =>
			handler((T1)args[0], (T2)args[1]), once, uiThread, sticky);


// ===================== 泛型订阅（T1, T2, T3） =====================
	public static IDisposable Subscribe<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args =>
			handler((T1)args[0], (T2)args[1], (T3)args[2]), once, uiThread, sticky);


// ===================== 泛型订阅（T1, T2, T3, T4） =====================
	public static IDisposable Subscribe<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args =>
			handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]), once, uiThread, sticky);

	// ===================== 泛型发布 =====================
	public static void PublishEvent<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		GetScope(scope).Publish(eventName, data!);
	}

	public static void PublishEventRemote<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		_ipc?.Broadcast(new IpcMessage(scope ?? _key, eventName, new[] { (object)data! }));
	}

	public static void PublishEventAll<TEvent>(TEvent data, string? scope = null) {
		PublishEvent(data, scope);
		PublishEventRemote(data, scope);
	}

	public static Task PublishEventAsync<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		return GetScope(scope).PublishAsync(eventName, data!);
	}

	public static Task PublishEventRemoteAsync<TEvent>(TEvent data, string? scope = null) {
		PublishEventRemote(data, scope);
		return Task.CompletedTask;
	}

	public static async Task PublishEventAllAsync<TEvent>(TEvent data, string? scope = null) {
		await PublishEventAsync(data, scope);
		PublishEventRemote(data, scope);
	}
}