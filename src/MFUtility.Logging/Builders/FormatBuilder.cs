using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Builders;

public class FormatBuilder {
	private readonly LogBuilder _root;

	public FormatBuilder(LogBuilder root) { _root = root; }
	public FormatBuilder Include(Action<IncludeBuilder> action) {
		var includeBuilder = new IncludeBuilder(_root);
		action(includeBuilder);
		return this;
	}
	public FormatBuilder Style(Action<StyleBuilder> action) {
		var style = new StyleBuilder(_root);
		action(style);
		return this;
	}

	public FormatBuilder Order(Action<FieldOrderBuilder> config) {
		var builder = new FieldOrderBuilder(_root.Config.Format.FieldOrder);
		config(builder);
		return this;
	}
	public FormatBuilder OrderDefault() {
		_root.Config.Format.FieldOrder = new List<LogField> {
			LogField.Time,
			LogField.Level,
			LogField.Assembly,
			LogField.ClassName,
			LogField.MethodName,
			LogField.ThreadId,
			LogField.ThreadType,
			LogField.ThreadName,
			LogField.TaskId,
			LogField.LineNumber,
			LogField.Message
		};
		return this;
	}


}