using MFUtility.Logging.Enums;
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
		_root.Config.Json.Async = _root.Config.File.Async;
		_root.Config.Json.MaxFileSizeMB = _root.Config.File.MaxFileSizeMB;
		_root.Config.Json.SplitDaily = _root.Config.File.SplitDaily;
		
		_root.Config.Json.EnableAppBasePath = _root.Config.File.EnableAppBasePath;
		_root.Config.Json.EnableSolutionPath = _root.Config.File.EnableSolutionPath;
		_root.Config.Json.EnableAbsolutePath = _root.Config.File.EnableAbsolutePath;
		_root.Config.Json.AppBasePath = _root.Config.File.AppBasePath;
		_root.Config.Json.SolutionSubFolder = _root.Config.File.SolutionSubFolder;
		_root.Config.Json.AbsolutePath = _root.Config.File.AbsolutePath;
		
		_root.Config.Json.UseAppFolder = _root.Config.File.UseAppFolder;
		_root.Config.Json.UseDateFolder = _root.Config.File.UseDateFolder;
		return this;
	}
	public JsonFileBuilder UseBasePath(string folder = "logs") {
		_root.Config.Json.EnableAppBasePath = true;
		_root.Config.Json.AppBasePath = folder;
		return this;
	}

	public JsonFileBuilder UseSolutionPath(string folder = "logs") {
		_root.Config.Json.EnableSolutionPath = true;
		_root.Config.Json.SolutionSubFolder = folder;
		return this;
	}

	public JsonFileBuilder UseAbsolutePath(string fullPath) {
		_root.Config.Json.EnableAbsolutePath = true;
		_root.Config.Json.AbsolutePath = fullPath;
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