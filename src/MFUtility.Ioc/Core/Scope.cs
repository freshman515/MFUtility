using MFUtility.Ioc.Enums;

namespace MFUtility.Ioc.Core;

internal class Scope : IScope {
	private readonly Container _rootContainer;
	private readonly Dictionary<Type, object> _scopedInstances = new();
	private bool _disposed;

	public Scope(Container rootContainer) {
		_rootContainer = rootContainer;
	}
	public object Resolve(Type serviceType, Lifetime lifetime=Lifetime.Transient) {
		if (_disposed)
			throw new ObjectDisposedException(nameof(Scope));

		return _rootContainer.ResolveInternal(serviceType, this);
	}
	public void TryRegisterIfNotExists(Type type, Lifetime lifetime) {
		_rootContainer.TryRegisterIfNotExists(type,lifetime);
	}
	internal bool TryGetScopedInstance(Type serviceType, out object? instance) {
		return _scopedInstances.TryGetValue(serviceType, out instance!);
	}
	internal void SetScopedInstance(Type serviceType, object instance) {
		_scopedInstances[serviceType] = instance;
	}

	public void Dispose() {
		if (_disposed) return;
		_disposed = true;

		// 这里可以附带处理 IDisposable 的 Scoped 对象
		foreach (var obj in _scopedInstances.Values) {
			if (obj is IDisposable d) {
				d.Dispose();
			}
		}
		_scopedInstances.Clear();
	}
	public T Resolve<T>() {
		return (T)Resolve(typeof(T));
	}

}