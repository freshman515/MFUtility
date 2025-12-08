using System.Text;
using Newtonsoft.Json;

namespace MFUtility.Common.Helpers;

/// <summary>
/// 🌟 高可靠 JSON 文件读写辅助类
/// - 支持共享访问、异步读写、重试、原子写入、防止损坏
/// - 异常时自动备份 .bak 文件
/// </summary>
public static class JsonHelper {
	private static readonly UTF8Encoding Utf8NoBom = new(false);
	private static readonly SemaphoreSlim FileLock = new(1, 1); // 全局锁，防止同进程竞争

	/// <summary>
	/// 异步保存对象为 JSON 文件（带原子写入与重试）
	/// </summary>
	public static async Task SaveAsync<T>(
		string filePath,
		T data,
		bool indented = true,
		int retryCount = 3,
		Action<Exception>? onError = null) {
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
		string tempFile = filePath + ".tmp";

		try {
			string json = JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None);

			await FileLock.WaitAsync();
			for (int i = 0; i < retryCount; i++) {
				try {
					using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.Read))
					using (var writer = new StreamWriter(fs, Utf8NoBom))
						await writer.WriteAsync(json);

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
	/// 同步保存（带重试与原子替换）
	/// </summary>
	public static void Save<T>(
		string filePath,
		T data,
		bool indented = true,
		int retryCount = 3,
		Action<Exception>? onError = null) {
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
		string tempFile = filePath + ".tmp";

		try {
			string json = JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None);

			lock (FileLock) {
				for (int i = 0; i < retryCount; i++) {
					try {
						using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.Read))
						using (var writer = new StreamWriter(fs, Utf8NoBom))
							writer.Write(json);

						if (File.Exists(filePath))
							File.Replace(tempFile, filePath, filePath + ".bak", true);
						else
							File.Move(tempFile, filePath);

						return;
					} catch (IOException) {
						Thread.Sleep(50 * (i + 1));
					}
				}
			}
		} catch (Exception ex) {
			onError?.Invoke(ex);
			throw;
		} finally {
			TryDelete(tempFile);
		}
	}

	/// <summary>
	/// 异步读取 JSON 文件（带共享访问和备份恢复）
	/// </summary>
	public static async Task<T?> LoadAsync<T>(
		string filePath,
		Action<Exception>? onError = null) {
		if (!File.Exists(filePath))
			return default;

		try {
			await FileLock.WaitAsync();
			using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var reader = new StreamReader(fs, Encoding.UTF8);
			string json = await reader.ReadToEndAsync();
			return JsonConvert.DeserializeObject<T>(json);
		} catch (JsonException ex) {
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
	/// 同步读取 JSON 文件（带容错）
	/// </summary>
	public static T? Load<T>(
		string filePath,
		Action<Exception>? onError = null) {
		if (!File.Exists(filePath))
			return default;

		try {
			lock (FileLock) {
				using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using var reader = new StreamReader(fs, Encoding.UTF8);
				string json = reader.ReadToEnd();
				return JsonConvert.DeserializeObject<T>(json);
			}
		} catch (JsonException ex) {
			onError?.Invoke(ex);
			CreateBackup(filePath);
			return default;
		} catch (Exception ex) {
			onError?.Invoke(ex);
			throw;
		}
	}

	/// <summary>
	/// 备份损坏的文件
	/// </summary>
	private static void CreateBackup(string filePath) {
		try {
			if (File.Exists(filePath)) {
				string backupPath = filePath + ".corrupt-" + DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss") + ".bak";
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