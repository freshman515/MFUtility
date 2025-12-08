using MFUtility.Ioc.Core;

namespace MFUtility.Ioc;

public static class IoC {
	public static Container Default { get; internal set; } = new Container();
}