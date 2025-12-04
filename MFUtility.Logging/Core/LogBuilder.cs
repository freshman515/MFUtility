using MFUtility.Logging.Builders;
using MFUtility.Logging.Configs;
using MFUtility.Logging.Enums;
using MFUtility.Logging.Interfaces;

namespace MFUtility.Logging;

public class LogBuilder {
	private readonly LogConfiguration _config = new();
	private readonly List<ILogProvider> _extraProviders = new();

	// ============================
	// WriteTo 子构建器
	// ============================
	public LogBuilder WriteTo(Action<WriteToBuilder> action) {
        var b = new WriteToBuilder(this);
        action(b);
        return this; // 返回配置根节点
    }

    // ===========================
    // Lambda Format
    // ===========================
    public LogBuilder Format(Action<FormatBuilder> action) {
        var f = new FormatBuilder(this);
        action(f);
        return this;
    }

    // ===========================
    // Lambda Level
    // ===========================
    public LogBuilder Level(LogLevel level) {
        _config.Level.Minimum = level;
        return this;
    }
	internal LogConfiguration Config => _config;


	internal void AddProvider(ILogProvider provider) {
		_extraProviders.Add(provider);
	}
	public void Apply() {
		LogManager.Initialize(cfg => {
			cfg.Output = _config.Output;
			cfg.Format = _config.Format;
			cfg.Level = _config.Level;
			cfg.Json = _config.Json;
		});
		foreach (var p in _extraProviders)
			LogManager.AddProvider(p);
	}
}