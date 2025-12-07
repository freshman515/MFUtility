using System;
using System.IO;
using System.Threading.Tasks;
using MFUtility.WPF.Helpers;

namespace MFUtility.Helpers;

/// <summary>
/// 🌟 通用配置文件工具：自动识别 JSON / XML 后缀并调用对应 Helper。
/// ✅ 自动创建目录、异步安全、支持原子写入与重试机制。
/// ✅ 推荐统一入口使用此类：ConfigHelper.Save / Load（可自动判断格式）。
/// </summary>
public static class ConfigHelper
{
	#region === 通用自动判断 ===

	/// <summary>
	/// 同步保存对象，根据文件后缀自动判断使用 JSON 或 XML。
	/// </summary>
	public static void Save<T>(string filePath, T data, bool indented = true)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		string ext = Path.GetExtension(filePath).ToLowerInvariant();

		switch (ext)
		{
			case ".json":
				JsonHelper.Save(filePath, data, indented);
				break;

			case ".xml":
				XmlHelper.Save(filePath, data);
				break;

			default:
				throw new NotSupportedException($"不支持的配置文件格式: {ext}");
		}
	}

	/// <summary>
	/// 异步保存对象，根据文件后缀自动判断使用 JSON 或 XML。
	/// </summary>
	public static async Task SaveAsync<T>(string filePath, T data, bool indented = true)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		string ext = Path.GetExtension(filePath).ToLowerInvariant();

		switch (ext)
		{
			case ".json":
				await JsonHelper.SaveAsync(filePath, data, indented);
				break;

			case ".xml":
				await XmlHelper.SaveAsync(filePath, data);
				break;

			default:
				throw new NotSupportedException($"不支持的配置文件格式: {ext}");
		}
	}

	/// <summary>
	/// 同步读取对象，根据文件后缀自动判断使用 JSON 或 XML。
	/// </summary>
	public static T? Load<T>(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		string ext = Path.GetExtension(filePath).ToLowerInvariant();

		return ext switch
		{
			".json" => JsonHelper.Load<T>(filePath),
			".xml"  => XmlHelper.Load<T>(filePath),
			_ => throw new NotSupportedException($"不支持的配置文件格式: {ext}")
		};
	}

	/// <summary>
	/// 异步读取对象，根据文件后缀自动判断使用 JSON 或 XML。
	/// </summary>
	public static async Task<T?> LoadAsync<T>(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
			throw new ArgumentNullException(nameof(filePath));

		string ext = Path.GetExtension(filePath).ToLowerInvariant();

		return ext switch
		{
			".json" => await JsonHelper.LoadAsync<T>(filePath),
			".xml"  => await XmlHelper.LoadAsync<T>(filePath),
			_ => throw new NotSupportedException($"不支持的配置文件格式: {ext}")
		};
	}

	#endregion

	#region === 明确调用（兼容旧代码）===

	// 保留原有的显式方法，避免破坏旧代码引用
	public static Task SaveJsonAsync<T>(string filePath, T data, bool indented = true)
		=> JsonHelper.SaveAsync(filePath, data, indented);

	public static void SaveJson<T>(string filePath, T data, bool indented = true)
		=> JsonHelper.Save(filePath, data, indented);

	public static Task<T?> LoadJsonAsync<T>(string filePath)
		=> JsonHelper.LoadAsync<T>(filePath);

	public static T? LoadJson<T>(string filePath)
		=> JsonHelper.Load<T>(filePath);

	public static Task SaveXmlAsync<T>(string filePath, T data)
		=> XmlHelper.SaveAsync(filePath, data);

	public static void SaveXml<T>(string filePath, T data)
		=> XmlHelper.Save(filePath, data);

	public static Task<T?> LoadXmlAsync<T>(string filePath)
		=> XmlHelper.LoadAsync<T>(filePath);

	public static T? LoadXml<T>(string filePath)
		=> XmlHelper.Load<T>(filePath);

	#endregion
}
