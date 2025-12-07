using MFUtility.Ioc.Core;

namespace MFUtility.Ioc.Interfaces;

public interface IIocAutoModule
{
    void Load(Container container);
}