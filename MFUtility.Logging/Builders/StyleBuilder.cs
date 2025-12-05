namespace MFUtility.Logging.Builders;

public class StyleBuilder {
	private readonly LogBuilder _root;
	public StyleBuilder(LogBuilder root) {
		_root = root;
	}
	public StyleBuilder TimeFormat(string fmt) {
		_root.Config.Format.TimeFormat = fmt;
		return this;
	}

	public StyleBuilder UseFieldBrackets(bool enabled = true) {
		_root.Config.Format.UseFieldBrackets = enabled;
		return this;
	}

	public StyleBuilder UseFieldTag(bool enabled = true) {
		_root.Config.Format.ShowFieldTag = enabled;
		return this;
	}

	public StyleBuilder Brackets(string left, string right) {
		_root.Config.Format.LeftBracket = left;
		_root.Config.Format.RightBracket = right;
		return this;
	}

	public StyleBuilder UseMessageBrackets(bool enabled = true) {
		_root.Config.Format.MessageUseBrackets = enabled;
		return this;
	}

	public StyleBuilder ExceptionNewLine(bool enabled = true) {
		_root.Config.Format.ExceptionNewLine = enabled;
		return this;
	}
}