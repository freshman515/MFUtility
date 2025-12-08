namespace MFUtility.Logging.Builders;

public class IncludeBuilder {
	private readonly LogBuilder _root;
	public IncludeBuilder(LogBuilder root) { _root = root; }
	public IncludeBuilder Assembly() {
		_root.Config.Format.IncludeAssembly = true;
		return this;
	}

	public IncludeBuilder ClassName() {
		_root.Config.Format.IncludeClassName = true;
		return this;
	}
	public IncludeBuilder IncludeAll() {
		_root.Config.Format.IncludeAssembly = true;
		_root.Config.Format.IncludeClassName = true;
		_root.Config.Format.IncludeMethodName = true;
		_root.Config.Format.IncludeLineNumber = true;
		_root.Config.Format.IncludeThreadId = true;
		_root.Config.Format.IncludeThreadType = true;
		_root.Config.Format.IncludeThreadName = true;
		_root.Config.Format.IncludeTaskId = true;

		return this;
	}
	public IncludeBuilder LineNumber() {
		_root.Config.Format.IncludeLineNumber = true;
		return this;
	}

	public IncludeBuilder MethodName() {
		_root.Config.Format.IncludeMethodName = true;
		return this;
	}
	public IncludeBuilder ThreadId() {
		_root.Config.Format.IncludeThreadId = true;
		return this;
	}
	public IncludeBuilder ThreadType() {
		_root.Config.Format.IncludeThreadType = true;
		return this;
	}
	public IncludeBuilder ThreadName() {
		_root.Config.Format.IncludeThreadName = true;
		return this;
	}
	public IncludeBuilder TaskId() {
		_root.Config.Format.IncludeTaskId = true;
		return this;
	}
}