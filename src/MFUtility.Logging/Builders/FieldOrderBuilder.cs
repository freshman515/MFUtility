using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Builders;

public class FieldOrderBuilder {
	private readonly List<LogField> _order;

	public FieldOrderBuilder(List<LogField> order) {
		_order = order;
		_order.Clear(); 
	}
	public FieldOrderBuilder Time() { _order.Add(LogField.Time); return this; }
    public FieldOrderBuilder Level() { _order.Add(LogField.Level); return this; }
    public FieldOrderBuilder Assembly() { _order.Add(LogField.Assembly); return this; }
    public FieldOrderBuilder ClassName() { _order.Add(LogField.ClassName); return this; }
    public FieldOrderBuilder MethodName() { _order.Add(LogField.MethodName); return this; }
    public FieldOrderBuilder ThreadId() { _order.Add(LogField.ThreadId); return this; }
    public FieldOrderBuilder ThreadType() { _order.Add(LogField.ThreadType); return this; }
    public FieldOrderBuilder ThreadName() { _order.Add(LogField.ThreadName); return this; }
    public FieldOrderBuilder TaskId() { _order.Add(LogField.TaskId); return this; }
    public FieldOrderBuilder LineNumber() { _order.Add(LogField.LineNumber); return this; }
    public FieldOrderBuilder Message() { _order.Add(LogField.Message); return this; }
}