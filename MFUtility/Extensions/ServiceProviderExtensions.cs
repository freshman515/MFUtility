using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace MFUtility.Extensions;

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
	/// 注册开放泛型接口与其实现（如 IRepository&lt;&gt; → Repository&lt;&gt;）
	/// </summary>
	public static IServiceCollection AddOpenGeneric(
		this IServiceCollection services,
		Type openGenericInterface,
		Type openGenericImplementation,
		ServiceLifetime lifetime = ServiceLifetime.Transient) {
		if (!openGenericInterface.IsGenericTypeDefinition || !openGenericImplementation.IsGenericTypeDefinition)
			throw new ArgumentException("必须传入开放泛型类型定义，如 typeof(IRepository<>)");

		services.Add(new ServiceDescriptor(openGenericInterface, openGenericImplementation, lifetime));
		return services;
	}
	public static IServiceCollection AddOpenGeneric<TInterface, TImplementation>(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Transient)
		where TInterface : class
		where TImplementation : class {
		return services.AddOpenGeneric(typeof(TInterface), typeof(TImplementation), lifetime);
	}
	/// <summary>
	/// 批量注册指定程序集中的服务类型（接口+实现 或 自身）
	/// </summary>
	public static IServiceCollection AddServicesFromAssembly(
		this IServiceCollection services,
		Assembly assembly,
		ServiceLifetime lifetime = ServiceLifetime.Singleton,
		IEnumerable<Type>? ignoreServiceTypes = null
	) {
		ignoreServiceTypes ??= Enumerable.Empty<Type>();

		var types = SafeGetTypes(assembly);

		foreach (var implType in types.Where(t => t.IsClass && !t.IsAbstract && !t.Name.Contains("<"))) {
			// ❌ 如果在忽略列表中 → 跳过
			if (ignoreServiceTypes.Contains(implType))
				continue;

			var interfaces = implType.GetInterfaces();

			if (interfaces.Length > 0) {
				foreach (var itf in interfaces) {
					services.Add(new ServiceDescriptor(itf, implType, lifetime));
				}
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
		ServiceLifetime lifetime = ServiceLifetime.Singleton, IEnumerable<Type>? ignoreServiceTypes = null) {
		var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		return services.AddServicesFromAssembly(asm, lifetime, ignoreServiceTypes);
	}

	/// <summary>
	/// 批量注册指定类型所在程序集的服务（类库使用）
	/// </summary>
	public static IServiceCollection AddServicesFrom<T>(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Singleton, IEnumerable<Type>? ignoreServiceTypes = null) {
		return services.AddServicesFromAssembly(typeof(T).Assembly, lifetime, ignoreServiceTypes);
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

	#region === Quick Register (显式批量注册) ===

	/// <summary>
	/// 批量注册多个类型为单例（Singleton）
	/// </summary>
	/// <param name="services">IServiceCollection</param>
	/// <param name="types">要注册的类型集合</param>
	public static IServiceCollection AddSingletons(this IServiceCollection services, params Type[] types) {
		foreach (var type in types) {
			if (type == null) continue;

			// 若类型有接口则绑定接口，否则绑定自身
			var interfaces = type.GetInterfaces();
			if (interfaces.Length > 0) {
				foreach (var itf in interfaces)
					services.AddSingleton(itf, type);
			} else {
				services.AddSingleton(type);
			}
		}

		return services;
	}

	/// <summary>
	/// 批量注册多个类型为作用域（Scoped）
	/// </summary>
	public static IServiceCollection AddScopes(this IServiceCollection services, params Type[] types) {
		foreach (var type in types) {
			if (type == null) continue;

			var interfaces = type.GetInterfaces();
			if (interfaces.Length > 0) {
				foreach (var itf in interfaces)
					services.AddScoped(itf, type);
			} else {
				services.AddScoped(type);
			}
		}

		return services;
	}

	/// <summary>
	/// 批量注册多个类型为瞬态（Transient）
	/// </summary>
	public static IServiceCollection AddTransients(this IServiceCollection services, params Type[] types) {
		foreach (var type in types) {
			if (type == null) continue;

			var interfaces = type.GetInterfaces();
			if (interfaces.Length > 0) {
				foreach (var itf in interfaces)
					services.AddTransient(itf, type);
			} else {
				services.AddTransient(type);
			}
		}

		return services;
	}

	#endregion

	/// <summary>
	/// 打印当前 IoC 容器中注册的所有服务（调试用）
	/// </summary>
	public static void DumpServices(this IServiceCollection services, bool includeLifetime = true) {
		Console.WriteLine("🔍 Registered services:");
		foreach (var s in services) {
			var lifetime = includeLifetime ? $" [{s.Lifetime}]" : string.Empty;
			Console.WriteLine($"  {s.ServiceType.FullName} → {s.ImplementationType?.FullName ?? "(factory)"}{lifetime}");
		}
	}

	public static bool IsRegistered<T>(this IServiceCollection services) {
		return services.Any(s => s.ServiceType == typeof(T));
	}

	public static T? TryGetService<T>(this IServiceProvider sp) where T : class {
		try {
			return sp.GetService(typeof(T)) as T;
		} catch {
			return null;
		}
	}

	public static TWindow CreateWindow<TWindow>(this IServiceProvider sp)
		where TWindow : Window, new() {
		var window = new TWindow();
		var vmType = ResolveViewModelType(typeof(TWindow));
		if (vmType != null)
			window.DataContext = sp.GetService(vmType) ?? ActivatorUtilities.CreateInstance(sp, vmType);
		return window;
	}

	public static object CreateInstance(this IServiceProvider sp, Type type) {
		return ActivatorUtilities.CreateInstance(sp, type);
	}

	public static IServiceCollection AddDerivedFrom<TBase>(
		this IServiceCollection services,
		Assembly assembly,
		ServiceLifetime lifetime = ServiceLifetime.Transient) {
		var baseType = typeof(TBase);
		var derivedTypes = assembly.GetTypes()
			.Where(t => baseType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

		foreach (var type in derivedTypes)
			services.Add(new ServiceDescriptor(type, type, lifetime));

		return services;
	}

	public static IServiceProvider Build(this IServiceCollection services, bool log = false) {
		if (log)
			Console.WriteLine($"🔧 Building ServiceProvider ({services.Count} services)...");
		return services.BuildServiceProvider();
	}

	public static IServiceCollection AddGenericImplementations(
		this IServiceCollection services,
		Assembly assembly,
		Type genericInterface,
		ServiceLifetime lifetime = ServiceLifetime.Transient) {
		var impls = assembly.GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract)
			.Select(t => new {
				Type = t,
				Interfaces = t.GetInterfaces()
					.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface)
					.ToList()
			})
			.Where(x => x.Interfaces.Count > 0);

		foreach (var impl in impls) {
			foreach (var iface in impl.Interfaces)
				services.Add(new ServiceDescriptor(iface, impl.Type, lifetime));
		}

		return services;
	}

	public static IServiceCollection AddByConvention(this IServiceCollection services, Assembly assembly) {
		var types = assembly.GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract)
			.ToList();

		foreach (var impl in types) {
			var itf = impl.GetInterfaces()
				.FirstOrDefault(i => i.Name == $"I{impl.Name}");
			if (itf != null)
				services.AddTransient(itf, impl);
		}

		return services;
	}

	public static IServiceCollection AddFactory<TService>(
		this IServiceCollection services,
		Func<IServiceProvider, TService> factory,
		ServiceLifetime lifetime = ServiceLifetime.Singleton)
		where TService : class {
		services.Add(new ServiceDescriptor(typeof(TService), sp => factory(sp)!, lifetime));
		return services;
	}

	public static IServiceCollection AddNamed<TService>(
		this IServiceCollection services,
		string name,
		Func<IServiceProvider, TService> factory)
		where TService : class {
		services.AddSingleton<NamedService<TService>>(new NamedService<TService>(name, factory));
		return services;
	}

	public static TService? GetNamed<TService>(this IServiceProvider sp, string name)
		where TService : class {
		var all = sp.GetServices<NamedService<TService>>();
		return all.FirstOrDefault(n => n.Name == name)?.Factory(sp);
	}

	public class NamedService<T> {
		public string Name { get; }
		public Func<IServiceProvider, T> Factory { get; }

		public NamedService(string name, Func<IServiceProvider, T> factory) {
			Name = name;
			Factory = factory;
		}
	}
}