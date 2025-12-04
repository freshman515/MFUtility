using MFUtility.Logging.Providers;

namespace MFUtility.Logging.Builders;

public class JsonFileBuilder {
	private readonly LogBuilder _root;

	public JsonFileBuilder(LogBuilder root) {
		_root = root;
		_root.AddProvider(new JsonLogProvider());
	}

	public JsonFileBuilder Indented(bool enabled = true) {
		_root.Config.Json.Indented = enabled;
		return this;
	}
	// 是否每个日志文件一个 JSON 数组（高级需求）

	public JsonFileBuilder UseJsonArrayFile(bool enabled = true) {
		_root.Config.Json.UseJsonArray = enabled;
		return this;
	}

	// 将 JSON 单行输出压缩去除 null
	public JsonFileBuilder IgnoreNullValues(bool enabled = true) {
		_root.Config.Json.IgnoreNullValues = enabled;
		return this;
	}

	public JsonFileBuilder UseDateFolder(bool enabled = true) {
		_root.Config.Json.UseDateFolder = enabled;
		return this;
	}
	public JsonFileBuilder InheritFromFile(bool enabled = true) {
		_root.Config.Json.UseDateFolder = _root.Config.Output.UseDateFolder;
		_root.Config.Json.Async = _root.Config.Output.Async;
		_root.Config.Json.MaxFileSizeMB = _root.Config.Output.MaxFileSizeMB;
		_root.Config.Json.SplitDaily = _root.Config.Output.SplitDaily;
		_root.Config.Json.RootPath = _root.Config.Output.RootPath;
		return this;
	}
	public JsonFileBuilder SetPath(string path = null) {
		_root.Config.Json.RootPath = path;
		return this;
	}
	public JsonFileBuilder SplitDaily(bool enabled = true) {
		_root.Config.Json.SplitDaily = enabled;
		return this;
	}

	public JsonFileBuilder MaxFileSizeMB(long size) {
		_root.Config.Json.MaxFileSizeMB = size;
		return this;
	}
}