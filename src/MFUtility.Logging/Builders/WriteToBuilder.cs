using MFUtility.Logging.Interfaces;
using MFUtility.Logging.Providers;

namespace MFUtility.Logging.Builders;

public class WriteToBuilder {
	private readonly LogBuilder _root;

	public WriteToBuilder(LogBuilder root) {
		_root = root;
	}

	public WriteToBuilder Console(bool color = false) {
		_root.Config.File.ConsoleColor = color;
		_root.AddProvider(new ConsoleLogProvider());
		return this;
	}

	public WriteToBuilder File() {
		// 仅启用 provider，不做配置
		_root.AddProvider(FileLogProvider.Instance);
		return this;
	}

	public WriteToBuilder File(Action<FileBuilder> action) {
		var builder = new FileBuilder(_root);
		action(builder);
		return this; // 返回 WriteToBuilder
	}

	 public WriteToBuilder JsonFile() {
        _root.AddProvider(new JsonLogProvider());
        return this;
    }
	public WriteToBuilder JsonFile(Action<JsonFileBuilder> action) {
		var builder = new JsonFileBuilder(_root);
		action(builder);
		return this;
	}

	public WriteToBuilder Provider(ILogProvider provider) {
		_root.AddProvider(provider);
		return this;
	}
}