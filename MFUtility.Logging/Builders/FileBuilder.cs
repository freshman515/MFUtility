using MFUtility.Logging.Enums;
using MFUtility.Logging.Providers;

namespace MFUtility.Logging.Builders;

public class FileBuilder {
	private readonly LogBuilder _root;

	public FileBuilder(LogBuilder root) {
		_root = root;
		_root.AddProvider(FileLogProvider.Instance);
	}

	public FileBuilder UseDateFolder(bool enabled = true) {
		_root.Config.File.UseDateFolder = enabled;
		return this;
	}
	public FileBuilder UseAppFolder(bool enabled = true) {
		_root.Config.File.UseAppFolder = enabled;
		return this;
	}
	
    public FileBuilder UseBasePath(string folder="logs")
    {
        _root.Config.File.EnableAppBasePath = true;
        _root.Config.File.AppBasePath = folder;
        return this;
    }

    public FileBuilder UseSolutionPath(string folder = "logs")
    {
        _root.Config.File.EnableSolutionPath = true;
        _root.Config.File.SolutionSubFolder = folder;
        return this;
    }

    public FileBuilder UseAbsolutePath(string fullPath)
    {
        _root.Config.File.EnableAbsolutePath = true;
        _root.Config.File.AbsolutePath = fullPath;
        return this;
    }
    public FileBuilder EnableException(bool enabled = true) {
	    _root.Config.File.EnabelExceptionInfo = enabled;
	    return this;
    }
	public FileBuilder MaxFileSizeMB(long size) {
		_root.Config.File.MaxFileSizeMB = size;
		return this;
	}

	public FileBuilder Async(bool enabled = true) {
		_root.Config.File.Async = enabled;
		return this;
	}
}