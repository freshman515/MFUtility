using MFUtility.Ioc.Enums;

namespace MFUtility.Ioc.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class InjectAttribute : Attribute {
    public Lifetime? Lifetime { get; } =Enums.Lifetime.Singleton;


    public InjectAttribute(Lifetime? lifetime= Enums.Lifetime.Singleton)
    {
        Lifetime = lifetime;
    }
}