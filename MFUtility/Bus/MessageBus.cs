using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace MFUtility.Bus;

public static class MessageBus {
	private static readonly ConcurrentDictionary<string, EventScope> _scopes = new();
	private const string _key = "__eventbus_default";
	private static Dispatcher Dispatcher =>
		Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

	private static SocketBridge? _ipc;

	internal static EventScope GetScope(string? name) =>
		_scopes.GetOrAdd(name ?? _key, _ => new EventScope(Dispatcher));

	public static EventScope Scope(string? name = null) => GetScope(name);

	// ===================== 启用跨进程通信 =====================
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

	// ===================== 删除某个事件的订阅 =====================
	public static void RemoveSubscription(string eventName, string? scopeName = null) {
		GetScope(scopeName).RemoveSubscriptions(eventName);
	}

	// ===================== 删除某作用域下所有事件的订阅 =====================
	public static void RemoveScopeSubscriptions(string scopeName = null) {
		GetScope(scopeName).RemoveAllSubscriptions();
	}

	// =====================================================
	// 🌟 发布（字符串事件）三种模式
	// =====================================================

	/// <summary>仅触发本地事件</summary>
	public static void Publish(string eventName, params object[] args) =>
		GetScope(_key).Publish(eventName, args);

	/// <summary>仅进行跨进程广播</summary>
	public static void PublishRemote(string eventName, params object[] args) =>
		_ipc?.Broadcast(new IpcMessage(_key, eventName, args));

	/// <summary>同时触发本地 + 跨进程广播</summary>
	public static void PublishAll(string eventName, params object[] args) {
		GetScope(_key).Publish(eventName, args);
		_ipc?.Broadcast(new IpcMessage(_key, eventName, args));
	}

	/// <summary>异步版本（仅本地）</summary>
	public static Task PublishAsync(string eventName, params object[] args) =>
		GetScope(_key).PublishAsync(eventName, args);

	/// <summary>异步版本（仅IPC）</summary>
	public static Task PublishRemoteAsync(string eventName, params object[] args) {
		_ipc?.Broadcast(new IpcMessage(_key, eventName, args));
		return Task.CompletedTask;
	}

	/// <summary>异步版本（本地+IPC）</summary>
	public static async Task PublishAllAsync(string eventName, params object[] args) {
		await GetScope(_key).PublishAsync(eventName, args);
		_ipc?.Broadcast(new IpcMessage(_key, eventName, args));
	}

	// =====================================================
	// 🚀 泛型支持（T、T1~T4）
	// =====================================================

	public static IDisposable Subscribe<T>(string eventName, Action<T> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args => handler((T)args[0]), once, uiThread, sticky);

	public static IDisposable Subscribe<T1, T2>(string eventName, Action<T1, T2> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args => handler((T1)args[0], (T2)args[1]), once, uiThread, sticky);

	public static IDisposable Subscribe<T1, T2, T3>(string eventName, Action<T1, T2, T3> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args => handler((T1)args[0], (T2)args[1], (T3)args[2]), once, uiThread, sticky);

	public static IDisposable Subscribe<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> handler,
		bool once = false, bool uiThread = false, bool sticky = false)
		=> GetScope(_key).Subscribe(eventName, args => handler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]), once, uiThread, sticky);

	// 泛型发布：仅本地
	public static void PublishEvent<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		GetScope(scope).Publish(eventName, data!);
	}

	// 泛型发布：仅IPC
	public static void PublishEventRemote<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		_ipc?.Broadcast(new IpcMessage(scope ?? _key, eventName, new object[] { data! }));
	}

	// 泛型发布：本地 + IPC
	public static void PublishEventAll<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		GetScope(scope).Publish(eventName, data!);
		_ipc?.Broadcast(new IpcMessage(scope ?? _key, eventName, new object[] { data! }));
	}

	// 泛型异步版本
	public static Task PublishEventAsync<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		return GetScope(scope).PublishAsync(eventName, data!);
	}

	public static Task PublishEventRemoteAsync<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		_ipc?.Broadcast(new IpcMessage(scope ?? _key, eventName, new object[] { data! }));
		return Task.CompletedTask;
	}

	public static async Task PublishEventAllAsync<TEvent>(TEvent data, string? scope = null) {
		string eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
		await GetScope(scope).PublishAsync(eventName, data!);
		_ipc?.Broadcast(new IpcMessage(scope ?? _key, eventName, new object[] { data! }));
	}
}