using MFUtility.Logging.Enums;

namespace MFUtility.Logging.Builders;

public class LevelBuilder {
	private readonly LogBuilder _parent;
	public LevelBuilder(LogBuilder parent) {
		_parent = parent;
	}
	public LogBuilder Minimum(LogLevel level) {
		_parent.Config.Level.Minimum = level;
		return _parent;
	}
}