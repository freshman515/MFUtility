using MFUtility.Ioc.Enums;
using MFUtility.Ioc.Interfaces;

namespace MFUtility.Ioc.Models;

public class ServiceDescriptor {
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public Lifetime Lifetime { get; }

    // 预先创建好的实例（Singleton 或手动注入 Instance 时用）
    public object? Implementation { get; set; }

    // 工厂（可选）
    public Func<IResolver, object>? Factory { get; }

    public ServiceDescriptor(
        Type serviceType,
        Type implementationType,
        Lifetime lifetime,
        Func<IResolver, object>? factory = null) {

        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
        Factory = factory;
    }
}
