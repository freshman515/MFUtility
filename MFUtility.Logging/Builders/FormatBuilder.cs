namespace MFUtility.Logging.Builders;

public class FormatBuilder {
	  private readonly LogBuilder _root;

    public FormatBuilder(LogBuilder root) {
        _root = root;
    }
	public FormatBuilder IncludeAssembly() {
		_root.Config.Format.IncludeAssembly = true;
		return this;
	}

	public FormatBuilder IncludeClassName() {
		_root.Config.Format.IncludeClassName = true;
		return this;
	}

	public FormatBuilder IncludeLineNumber() {
		_root.Config.Format.IncludeLineNumber = true;
		return this;
	}
	
	public FormatBuilder IncludeMethodName() {
		_root.Config.Format.IncludeMethodName = true;
		return this;
	}

	public FormatBuilder UseTimeFormat(string fmt) {
		_root.Config.Format.TimeFormat = fmt;
		return this;
	}

	public FormatBuilder UseFieldBrackets(bool enabled = true) {
		_root.Config.Format.UseFieldBrackets = enabled;
		return this;
	}

	public FormatBuilder ShowFieldTag(bool enabled = true) {
		_root.Config.Format.ShowFieldTag = enabled;
		return this;
	}

	public FormatBuilder SetBrackets(string left, string right) {
		_root.Config.Format.LeftBracket = left;
		_root.Config.Format.RightBracket = right;
		return this;
	}

	public FormatBuilder MessageUseBrackets(bool enabled = true) {
		_root.Config.Format.MessageUseBrackets = enabled;
		return this;
	}

	public FormatBuilder ExceptionNewLine(bool enabled = true) {
		_root.Config.Format.ExceptionNewLine = enabled;
		return this;
	}

}