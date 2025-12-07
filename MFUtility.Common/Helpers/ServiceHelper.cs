using Microsoft.Extensions.DependencyInjection;

namespace MFUtility.Helpers;

/// <summary>
/// 🌍 全局服务定位器（ServiceProvider 助手）
/// 用于在非注入环境中快速获取依赖服务。
/// </summary>
/// <remarks>
/// ⚠️ 请确保在程序启动时调用 <see cref="Initialize"/> 设置 ServiceProvider。
/// 适用于 WPF、控制台、工具库等无法构造依赖的场景。
/// </remarks>
public class ServiceHelper {
	private static IServiceProvider? _provider;

	/// <summary>
	/// 初始化全局 ServiceProvider（通常在 App.xaml.cs 的 OnStartup 调用）
	/// </summary>
	private static readonly object _lock = new();

	public static void Initialize(IServiceProvider provider) {
		lock (_lock) {
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}
	}

	/// <summary>
	/// 获取当前全局 ServiceProvider
	/// </summary>
	public static IServiceProvider Provider =>
		_provider ?? throw new InvalidOperationException("❌ ServiceHelper 未初始化，请先调用 Initialize()。");

	/// <summary>
	/// 获取指定类型的服务（未注册则返回 null）
	/// </summary>
	public static T? Get<T>() where T : class =>
		Provider.GetService<T>();

	/// <summary>
	/// 获取指定类型的服务（未注册则抛异常）
	/// </summary>
	public static T GetRequired<T>() where T : class =>
		Provider.GetRequiredService<T>();

	/// <summary>
	/// 获取非泛型服务（动态类型）
	/// </summary>
	public static object? Get(Type type) =>
		Provider.GetService(type);

	/// <summary>
	/// 创建一个带依赖注入的实例（适用于 ViewModel、Dialog 等）
	/// </summary>
	public static T Create<T>(params object[] args) =>
		ActivatorUtilities.CreateInstance<T>(Provider, args);

	/// <summary>
	/// 尝试获取服务，如果不存在则返回默认值
	/// </summary>
	public static T? TryGet<T>(Func<T>? fallback = null) where T : class {
		var service = Get<T>();
		return service ?? fallback?.Invoke();
	}

	/// <summary>
	/// 判断某类型是否已注册
	/// </summary>
	public static bool IsRegistered<T>() =>
		Provider.GetService<T>() != null;
}