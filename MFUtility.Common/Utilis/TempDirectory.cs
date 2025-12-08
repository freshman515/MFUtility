namespace MFUtility.Common.Utilis;

public sealed class TempDirectory : IDisposable {
	public string Path { get; }
	private readonly bool _deleteOnDispose;
	private bool _disposed;

	/// <summary>
	/// 创建一个安全的随机临时目录。
	/// </summary>
	public TempDirectory(bool deleteOnDispose = true)
		: this(CreateSafeRandomPath(), deleteOnDispose) {
	}

	/// <summary>
	/// 使用指定路径创建临时目录（不推荐直接指定）
	/// </summary>
	public TempDirectory(string directoryPath, bool deleteOnDispose = true) {
		GuardSafe(directoryPath);

		Path = directoryPath;
		_deleteOnDispose = deleteOnDispose;

		if (Directory.Exists(Path))
			Directory.Delete(Path, recursive: true);

		Directory.CreateDirectory(Path);
	}

	/// <summary>
	/// 移动临时目录到指定位置（原子操作）
	/// </summary>
	public void MoveTo(string targetPath) {
		GuardSafe(targetPath);

		if (!Directory.Exists(Path))
			throw new DirectoryNotFoundException($"Temp directory not found: {Path}");

		if (Directory.Exists(targetPath))
			throw new IOException($"Target directory already exists: {targetPath}");

		Directory.Move(Path, targetPath);
	}

	public void Dispose() {
		if (!_disposed) {
			if (_deleteOnDispose && Directory.Exists(Path)) {
				try {
					Directory.Delete(Path, recursive: true);
				} catch (Exception ex) {
					Console.WriteLine($"[TempDirectory] Cleanup failed: {ex.Message}");
				}
			}

			_disposed = true;
		}
	}

	// ---------------------
	// Helper Methods
	// ---------------------

	private static string CreateSafeRandomPath() {
		var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MyApp");

		Directory.CreateDirectory(root);

		return System.IO.Path.Combine(root, Guid.NewGuid().ToString("N"));
	}

	private static void GuardSafe(string path) {
		if (string.IsNullOrWhiteSpace(path))
			throw new ArgumentException("Directory path cannot be null or empty", nameof(path));

		// 防止删系统目录
		path = System.IO.Path.GetFullPath(path);

		var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
		var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var root = System.IO.Path.GetPathRoot(path);

		if (path.Equals(windows, StringComparison.OrdinalIgnoreCase) ||
		    path.Equals(user, StringComparison.OrdinalIgnoreCase) ||
		    path.Equals(root, StringComparison.OrdinalIgnoreCase)) {
			throw new InvalidOperationException($"Refusing to operate on a protected directory: {path}");
		}
	}
}