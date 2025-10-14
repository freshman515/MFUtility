using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ReportView.Extension;

/// <summary>
/// IoC 容器批量注册 View / ViewModel / Service 的辅助扩展
/// </summary>
public static class ServiceProviderExtensions {
	#region === CreateView with Auto-ViewModel Binding ===

	/// <summary>
	/// 创建一个 View 并自动绑定对应的 ViewModel（根据命名约定）
	/// </summary>
	public static TView CreateView<TView>(this IServiceProvider sp)
		where TView : FrameworkElement, new() {
		var view = new TView();
		var vmType = ResolveViewModelType(typeof(TView));

		if (vmType != null) {
			// 从容器获取或自动构造
			var vm = sp.GetService(vmType) ?? ActivatorUtilities.CreateInstance(sp, vmType);
			view.DataContext = vm;
		}

		return view;
	}

	private static Type? ResolveViewModelType(Type viewType) {
		var fullName = viewType.FullName;
		if (string.IsNullOrEmpty(fullName))
			return null;

		string ns = viewType.Namespace ?? string.Empty;
		string typeName = viewType.Name;

		// 生成对应 VM 名称
		string vmName = typeName switch {
			_ when typeName.EndsWith("View", StringComparison.Ordinal) =>
				typeName.Substring(0, typeName.Length - "View".Length) + "ViewModel",

			_ when typeName.EndsWith("Window", StringComparison.Ordinal) =>
				typeName.Substring(0, typeName.Length - "Window".Length) + "ViewModel",

			_ when typeName.EndsWith("Dialog", StringComparison.Ordinal) =>
				typeName.Substring(0, typeName.Length - "Dialog".Length) + "ViewModel",

			_ => null
		};
		if (vmName == null)
			return null;

		// 命名空间替换规则
		if (ns.Contains(".Views"))
			ns = ns.Replace(".Views", ".ViewModels");
		else if (!string.IsNullOrEmpty(ns))
			ns += ".ViewModels";

		string finalName = string.IsNullOrEmpty(ns) ? vmName : $"{ns}.{vmName}";

		// 优先在当前程序集查找
		var vmType = viewType.Assembly.GetType(finalName);

		// 如果没找到，尝试在入口程序集查找（跨程序集支持）
		return vmType ?? Assembly.GetEntryAssembly()?.GetType(finalName);
	}

	#endregion

	#region === Register ViewModels ===

	/// <summary>
	/// 批量注册指定程序集下所有 *ViewModel 类型
	/// </summary>
	public static IServiceCollection AddViewModelsFromAssembly(
		this IServiceCollection services,
		Assembly assembly,
		ServiceLifetime lifetime = ServiceLifetime.Transient) {
		var vmTypes = SafeGetTypes(assembly)
			.Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("ViewModel"))
			.ToList();

		foreach (var vmType in vmTypes)
			services.Add(new ServiceDescriptor(vmType, vmType, lifetime));

		return services;
	}

	/// <summary>
	/// 批量注册当前“入口程序集”的所有 ViewModel（主程序使用）
	/// </summary>
	public static IServiceCollection AddViewModels(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Transient) {
		var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		return services.AddViewModelsFromAssembly(asm, lifetime);
	}

	/// <summary>
	/// 批量注册指定类型所在程序集的 ViewModel（类库使用）
	/// </summary>
	public static IServiceCollection AddViewModelsFrom<T>(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Transient) {
		return services.AddViewModelsFromAssembly(typeof(T).Assembly, lifetime);
	}

	#endregion

	#region === Register Services ===

	/// <summary>
	/// 批量注册指定程序集中的服务类型（接口+实现 或 自身）
	/// </summary>
	public static IServiceCollection AddServicesFromAssembly(
		this IServiceCollection services,
		Assembly assembly,
		ServiceLifetime lifetime = ServiceLifetime.Singleton) {
		var types = SafeGetTypes(assembly);

		foreach (var implType in types.Where(t => t.IsClass && !t.IsAbstract && !t.Name.Contains("<"))) {
			var interfaces = implType.GetInterfaces();

			if (interfaces.Length > 0) {
				// 注册所有接口（而非仅第一个）
				foreach (var itf in interfaces)
					services.Add(new ServiceDescriptor(itf, implType, lifetime));
			} else {
				services.Add(new ServiceDescriptor(implType, implType, lifetime));
			}
		}


		return services;
	}

	/// <summary>
	/// 批量注册主程序集中的所有服务（入口项目使用）
	/// </summary>
	public static IServiceCollection AddServices(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Singleton) {
		var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		return services.AddServicesFromAssembly(asm, lifetime);
	}

	/// <summary>
	/// 批量注册指定类型所在程序集的服务（类库使用）
	/// </summary>
	public static IServiceCollection AddServicesFrom<T>(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Singleton) {
		return services.AddServicesFromAssembly(typeof(T).Assembly, lifetime);
	}

	#endregion

	#region === Internal Helper ===

	private static Type[] SafeGetTypes(Assembly asm) {
		try {
			return asm.GetTypes();
		} catch (ReflectionTypeLoadException ex) {
			return ex.Types.Where(t => t != null).ToArray()!;
		}
	}

	#endregion
}