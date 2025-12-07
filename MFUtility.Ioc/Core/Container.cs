using System.Reflection;
using MFUtility.Ioc.Enums;
using MFUtility.Ioc.Interfaces;
using MFUtility.Ioc.Models;

namespace MFUtility.Ioc.Core;

public class Container : IResolver, IScopeFactory, IDisposable {
	private readonly Dictionary<Type, ServiceDescriptor> _services = new();
	public static event Action<Container>? OnContainerCreated;
	private readonly Dictionary<Type, object> _singletonInstances = new();
	private static readonly List<IIocAutoModule> _pendingModules = new();
	private bool _disposed;
	public Container() {
		IoC.Default = this;
		OnContainerCreated?.Invoke(this);
		LoadModulesFrom(AppDomain.CurrentDomain.GetAssemblies());
		foreach (var module in _pendingModules) {
			module.Load(this);
		}
	}
	private static void LoadModulesFrom(Assembly[] assemblies) {
		foreach (var asm in assemblies) {
			try {
				foreach (var type in asm.GetTypes()) {
					if (typeof(IIocAutoModule).IsAssignableFrom(type) &&
					    type.IsClass && !type.IsAbstract) {
						var module = (IIocAutoModule)Activator.CreateInstance(type);
						_pendingModules.Add(module);
					}
				}
			} catch { }
		}
	}
	private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args) {
		var asm = args.LoadedAssembly;

		try {
			foreach (var type in asm.GetTypes()) {
				if (typeof(IIocAutoModule).IsAssignableFrom(type) &&
				    type.IsClass && !type.IsAbstract) {
					var module = (IIocAutoModule)Activator.CreateInstance(type);
					_pendingModules.Add(module);
				}
			}
		} catch {
			// 某些程序集可能无法反射类型，忽略即可
		}
	}
	#region 注册 API

	#region Add / TryAdd API (Singleton / Scoped / Transient)

//
// ------------ AddSingleton --------------
//
	public void AddSingleton<TService>() {
		Register(typeof(TService), typeof(TService), Lifetime.Singleton);
	}

	public void AddSingleton<TService, TImplementation>()
		where TImplementation : TService {
		Register(typeof(TService), typeof(TImplementation), Lifetime.Singleton);
	}

	public void AddSingleton<TService>(TService instance) {
		RegisterInstance(instance);
	}

	public void AddSingleton(Type serviceType) {
		Register(serviceType, serviceType, Lifetime.Singleton);
	}

	public void AddSingleton(Type serviceType, Type implType) {
		Register(serviceType, implType, Lifetime.Singleton);
	}

//
// ------------ TryAddSingleton --------------
//
	public bool TryAddSingleton<TService>() {
		var type = typeof(TService);
		if (_services.ContainsKey(type)) return false;

		AddSingleton<TService>();
		return true;
	}

	public bool TryAddSingleton<TService, TImplementation>()
		where TImplementation : TService {
		var type = typeof(TService);
		if (_services.ContainsKey(type)) return false;

		AddSingleton<TService, TImplementation>();
		return true;
	}

	public bool TryAddSingleton<TService>(TService instance) {
		var type = typeof(TService);
		if (_services.ContainsKey(type)) return false;

		AddSingleton(instance);
		return true;
	}

//
// ------------ AddTransient --------------
//
	public void AddTransient<TService>() {
		Register(typeof(TService), typeof(TService), Lifetime.Transient);
	}

	public void AddTransient<TService, TImplementation>()
		where TImplementation : TService {
		Register(typeof(TService), typeof(TImplementation), Lifetime.Transient);
	}

	public void AddTransient(Type serviceType) {
		Register(serviceType, serviceType, Lifetime.Transient);
	}

	public void AddTransient(Type serviceType, Type implType) {
		Register(serviceType, implType, Lifetime.Transient);
	}

//
// ------------ TryAddTransient --------------
//
	public bool TryAddTransient<TService>() {
		var type = typeof(TService);
		if (_services.ContainsKey(type)) return false;

		AddTransient<TService>();
		return true;
	}

	public bool TryAddTransient<TService, TImplementation>()
		where TImplementation : TService {
		var type = typeof(TService);
		if (_services.ContainsKey(type)) return false;

		AddTransient<TService, TImplementation>();
		return true;
	}

//
// ------------ AddScoped --------------
//
	public void AddScoped<TService>() {
		Register(typeof(TService), typeof(TService), Lifetime.Scoped);
	}

	public void AddScoped<TService, TImplementation>()
		where TImplementation : TService {
		Register(typeof(TService), typeof(TImplementation), Lifetime.Scoped);
	}

	public void AddScoped(Type serviceType) {
		Register(serviceType, serviceType, Lifetime.Scoped);
	}

	public void AddScoped(Type serviceType, Type implType) {
		Register(serviceType, implType, Lifetime.Scoped);
	}

//
// ------------ TryAddScoped --------------
//
	public bool TryAddScoped<TService>() {
		var type = typeof(TService);
		if (_services.ContainsKey(type)) return false;

		AddScoped<TService>();
		return true;
	}

	public bool TryAddScoped<TService, TImplementation>()
		where TImplementation : TService {
		var type = typeof(TService);
		if (_services.ContainsKey(type)) return false;

		AddScoped<TService, TImplementation>();
		return true;
	}

	#endregion
	public void RegisterInstance<TService>(TService instance) {
		if (instance == null) throw new ArgumentNullException(nameof(instance));

		var type = typeof(TService);

		var descriptor = new ServiceDescriptor(
			serviceType: type,
			implementationType: type,
			lifetime: Lifetime.Singleton) {
			Implementation = instance
		};

		_services[type] = descriptor;
		_singletonInstances[type] = instance!;
	}


	public void Register<TService>(Lifetime lifetime = Lifetime.Transient) {
		Register(typeof(TService), typeof(TService), lifetime);
	}

	public void Register<TService, TImplementation>(
		Lifetime lifetime = Lifetime.Transient)
		where TImplementation : TService {

		Register(typeof(TService), typeof(TImplementation), lifetime);
	}

	public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient) {
		Register(serviceType, serviceType, lifetime);
	}

	public void Register(
		Type serviceType,
		Type implementationType,
		Lifetime lifetime = Lifetime.Transient,
		Func<IResolver, object>? factory = null) {

		if (!serviceType.IsAssignableFrom(implementationType) && serviceType != implementationType)
			throw new InvalidOperationException(
				$"{implementationType.FullName} 不可赋值给 {serviceType.FullName}");

		var descriptor = new ServiceDescriptor(
			serviceType,
			implementationType,
			lifetime,
			factory);

		_services[serviceType] = descriptor;
	}
	public void AutoRegister(
		Assembly assembly,
		Func<Type, bool>? filter = null,
		Lifetime lifetime = Lifetime.Transient,
		bool registerInterfaces = true) {
		var types = assembly.GetTypes()
		                    .Where(t => t.IsClass && !t.IsAbstract)
		                    .Where(t => filter == null || filter(t));

		foreach (var type in types) {
			// 注册自身
			TryAdd(type, type, lifetime);

			if (!registerInterfaces)
				continue;

			// 注册接口 → 实现
			foreach (var iface in type.GetInterfaces()) {
				// 避免注册系统接口
				if (iface.Namespace != null && iface.Namespace.StartsWith("System"))
					continue;

				TryAdd(iface, type, lifetime);
			}
		}
	}
	private bool TryAdd(Type serviceType, Type implType, Lifetime lifetime) {
		if (_services.ContainsKey(serviceType))
			return false;

		Register(serviceType, implType, lifetime);
		return true;
	}

	#endregion

	#region 解析 API（对外）

	public TService Resolve<TService>() {
		return (TService)Resolve(typeof(TService));
	}
	public void TryRegisterIfNotExists(Type type, Lifetime lifetime) {
		if (!_services.ContainsKey(type)) {
			Register(type, lifetime);
		}
	}

	public object Resolve(Type serviceType, Lifetime lifetime = Lifetime.Transient) {
		return ResolveInternal(serviceType, scope: null, lifetime);
	}

	public IScope CreateScope() {
		if (_disposed)
			throw new ObjectDisposedException(nameof(Container));

		return new Scope(this);
	}

	#endregion

	#region 内部解析逻辑

	internal object ResolveInternal(Type serviceType, Scope? scope, Lifetime lifetime = Lifetime.Singleton) {
		if (_disposed)
			throw new ObjectDisposedException(nameof(Container));

		// 自身解析
		if (serviceType == typeof(Container) || serviceType == typeof(IResolver))
			return this;

		// 如果没注册，但又是具体类，做一次隐式注册（和你原来逻辑保持一致）
		if (!_services.ContainsKey(serviceType) && serviceType.IsClass && !serviceType.IsAbstract) {
			Register(serviceType, serviceType, lifetime);
		}

		if (!_services.TryGetValue(serviceType, out var descriptor))
			throw new Exception($"Service {serviceType.Name} not registered.");

		// Singleton：从容器级缓存取
		if (descriptor.Lifetime == Lifetime.Singleton) {
			if (_singletonInstances.TryGetValue(serviceType, out var singleton))
				return singleton;
		}

		// Scoped：从 Scope 内缓存取
		if (descriptor.Lifetime == Lifetime.Scoped) {
			if (scope == null)
				throw new InvalidOperationException(
					$"解析 Scoped 服务 {serviceType.Name} 时必须在 Scope 中解析。");

			if (scope.TryGetScopedInstance(serviceType, out var scopedInstance)) {
				return scopedInstance!;
			}
		}

		// 创建实例
		var implementation = CreateInstance(descriptor, scope ?? null);

		// 缓存单例
		if (descriptor.Lifetime == Lifetime.Singleton) {
			_singletonInstances[serviceType] = implementation;
		}

		// 缓存 Scoped
		if (descriptor.Lifetime == Lifetime.Scoped && scope != null) {
			scope.SetScopedInstance(serviceType, implementation);
		}

		return implementation;
	}

	private object CreateInstance(ServiceDescriptor descriptor, Scope? scope) {
		// 优先使用 Factory
		if (descriptor.Factory != null) {
			return descriptor.Factory(this);
		}

		var implType = descriptor.ImplementationType;

		// 找参数最多的构造函数（保持和你原来的行为一致）
		var constructor = implType
		                  .GetConstructors()
		                  .OrderByDescending(c => c.GetParameters().Length)
		                  .FirstOrDefault();

		if (constructor == null) {
			throw new Exception($"类型 {implType.Name} 没有公共构造函数。");
		}

		var parameters = constructor.GetParameters();
		if (parameters.Length == 0)
			return Activator.CreateInstance(implType)!;

		var args = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++) {
			var pType = parameters[i].ParameterType;
			args[i] = ResolveInternal(pType, scope);
		}

		return Activator.CreateInstance(implType, args)!;
	}

	#endregion

	#region Dispose

	public void Dispose() {
		if (_disposed) return;
		_disposed = true;

		// 处理单例里的 IDisposable
		foreach (var obj in _singletonInstances.Values) {
			if (obj is IDisposable d) {
				d.Dispose();
			}
		}
		_singletonInstances.Clear();
		_services.Clear();
	}

	#endregion
}