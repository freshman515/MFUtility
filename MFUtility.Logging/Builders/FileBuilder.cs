using MFUtility.Logging.Providers;

namespace MFUtility.Logging.Builders;

public class FileBuilder {
    private readonly LogBuilder _root;

    public FileBuilder(LogBuilder root) {
        _root = root;

        _root.AddProvider(FileLogProvider.Instance);
    }

    public FileBuilder UseDateFolder(bool enabled = true) {
        _root.Config.Output.UseDateFolder = enabled;
        return this;
    }

    public FileBuilder MaxFileSizeMB(long size) {
        _root.Config.Output.MaxFileSizeMB = size;
        return this;
    }

    public FileBuilder Async(bool enabled = true) {
        _root.Config.Output.Async = enabled;
        return this;
    }
}
