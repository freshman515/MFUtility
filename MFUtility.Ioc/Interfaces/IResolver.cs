using MFUtility.Ioc.Enums;

namespace MFUtility.Ioc.Interfaces;

public interface IResolver {
	
    object Resolve(Type type,Lifetime lifetime=Lifetime.Transient);
    T Resolve<T>();
    void TryRegisterIfNotExists(Type type, Lifetime lifetime);
}