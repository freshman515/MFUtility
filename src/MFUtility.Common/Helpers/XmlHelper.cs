using System.Xml.Serialization;

namespace MFUtility.Common.Helpers;

/// <summary>
/// 🌟 高可靠 XML 文件读写辅助类
/// - 支持异步与同步模式
/// - 原子写入（临时文件 + 替换）
/// - 读写共享、防止文件损坏
/// - 自动建目录、失败重试、异常回调
/// </summary>
public static class XmlHelper {
	private static readonly SemaphoreSlim FileLock = new(1, 1);

	/// <summary>
	/// 异步保存 XML（原子写入 + 重试 + 共享访问）
	/// </summary>
	public static async Task SaveAsync<T>(
		string filePath,
		T data,
		int retryCount = 3,
		Action<Exception>? onError = null) {
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
		string tempFile = filePath + ".tmp";
		var serializer = new XmlSerializer(typeof(T));

		await FileLock.WaitAsync();
		try {
			for (int i = 0; i < retryCount; i++) {
				try {
					using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.Read)) {
						var ns = new XmlSerializerNamespaces();
						ns.Add("", ""); // 清空 namespace
						serializer.Serialize(fs, data, ns);
					}

					// ✅ 原子替换
					if (File.Exists(filePath))
						File.Replace(tempFile, filePath, filePath + ".bak", true);
					else
						File.Move(tempFile, filePath);

					return;
				} catch (IOException) {
					await Task.Delay(50 * (i + 1));
				}
			}
		} catch (Exception ex) {
			onError?.Invoke(ex);
			throw;
		} finally {
			FileLock.Release();
			TryDelete(tempFile);
		}
	}

	/// <summary>
	/// 同步保存 XML（带重试与原子替换）
	/// </summary>
	public static void Save<T>(
		string filePath,
		T data,
		int retryCount = 3,
		Action<Exception>? onError = null) {
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
		string tempFile = filePath + ".tmp";
		var serializer = new XmlSerializer(typeof(T));

		lock (FileLock) {
			try {
				for (int i = 0; i < retryCount; i++) {
					try {
						using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.Read)) {
							var ns = new XmlSerializerNamespaces();
							ns.Add("", ""); // 清空 namespace
							serializer.Serialize(fs, data, ns);
						}

						if (File.Exists(filePath))
							File.Replace(tempFile, filePath, filePath + ".bak", true);
						else
							File.Move(tempFile, filePath);

						return;
					} catch (IOException) {
						Thread.Sleep(50 * (i + 1));
					}
				}
			} catch (Exception ex) {
				onError?.Invoke(ex);
				throw;
			} finally {
				TryDelete(tempFile);
			}
		}
	}

	/// <summary>
	/// 异步读取 XML（带共享访问与容错）
	/// </summary>
	public static async Task<T?> LoadAsync<T>(
		string filePath,
		Action<Exception>? onError = null) {
		if (!File.Exists(filePath))
			return default;

		var serializer = new XmlSerializer(typeof(T));

		await FileLock.WaitAsync();
		try {
			using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return (T?)serializer.Deserialize(fs);
		} catch (InvalidOperationException ex) {
			onError?.Invoke(ex);
			CreateBackup(filePath);
			return default;
		} catch (Exception ex) {
			onError?.Invoke(ex);
			throw;
		} finally {
			FileLock.Release();
		}
	}

	/// <summary>
	/// 同步读取 XML（带容错）
	/// </summary>
	public static T? Load<T>(
		string filePath,
		Action<Exception>? onError = null) {
		if (!File.Exists(filePath))
			return default;
		if (new FileInfo(filePath).Length == 0) {
			var newObj = Activator.CreateInstance<T>();
			Save(filePath, newObj);
			return newObj;
		}
		var serializer = new XmlSerializer(typeof(T));


		lock (FileLock) {
			try {


				using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				return (T?)serializer.Deserialize(fs);
			} catch (InvalidOperationException ex) {
				onError?.Invoke(ex);
				CreateBackup(filePath);
				return default;
			} catch (Exception ex) {
				onError?.Invoke(ex);
				throw;
			}
		}
	}

	private static void CreateBackup(string filePath) {
		try {
			if (File.Exists(filePath)) {
				string backupPath = $"{filePath}.corrupt-{DateTime.Now:yyyyMMdd_HHmmss}.bak";
				File.Copy(filePath, backupPath, true);
			}
		} catch { /* 忽略 */
		}
	}

	private static void TryDelete(string path) {
		try {
			if (File.Exists(path))
				File.Delete(path);
		} catch { /* 忽略 */
		}
	}
}