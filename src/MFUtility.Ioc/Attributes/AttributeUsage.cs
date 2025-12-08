namespace MFUtility.Ioc.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewForAttribute : Attribute {
	public Type ViewModelType { get; }

	public ViewForAttribute(Type vmType) {
		ViewModelType = vmType;
	}
}