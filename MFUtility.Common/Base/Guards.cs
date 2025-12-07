using MFUtility.Extensions;

namespace MFUtility.Core.Base;

public static class Guards {
	/// <summary>对象为null时抛出异常</summary>
	/// <param name="parameters"></param>
	/// <exception cref="T:System.ArgumentNullException"></exception>
	public static void ThrowIfNull(params object[] parameters) {
		if (((IEnumerable<object>)parameters).Any<object>((Func<object, bool>)(item => item == null)))
			throw new ArgumentNullException();
	}

	/// <summary>对象为null时抛出异常，并提示message信息</summary>
	/// <param name="obj"></param>
	/// <param name="message"></param>
	/// <exception cref="T:System.ArgumentNullException"></exception>
	public static void ThrowMessageIfNull(string message, params object[] parameters) {
		if (((IEnumerable<object>)parameters).Any<object>((Func<object, bool>)(item => item == null)))
			throw new Exception(message);
	}

	/// <summary>
	/// 当任意一个字符串为 null 或空时抛出 ArgumentNullException
	/// </summary>
	/// <exception cref="ArgumentNullException"></exception>
	public static void ThrowIfNullOrEmpty(params string[] parameters) {
		if (parameters == null)
			throw new ArgumentNullException(nameof(parameters));

		if (parameters.Any(string.IsNullOrEmpty))
			throw new ArgumentNullException("One or more parameters are null or empty.");
	}

	/// <summary>
	/// 当任意一个字符串为 null 或空时，抛出带 message 的异常。
	/// </summary>
	public static void ThrowMessageIfNullOrEmpty(string message, params string[] parameters) {
		if (parameters == null)
			throw new ArgumentNullException(nameof(parameters));

		if (parameters.Any(string.IsNullOrEmpty))
			throw new Exception(message);
	}

	/// <summary>字符串集合中有空字符串时抛出异常</summary>
	/// <param name="strings"></param>
	public static void ThrowIfNullOrEmptyStringArray(IEnumerable<string> strings) {
		Guards.ThrowIfNull((object)strings);
		Guards.ThrowIfNullOrEmpty(strings.ToArray<string>());
	}

	/// <summary>集合为空时抛出异常</summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="items"></param>
	public static void ThrowIfNullOrEmptyArray<T>(IEnumerable<T> items) {
		Guards.ThrowIfNull((object)items);
		Guards.ThrowIfNot(items.HadItems<T>());
	}

	/// <summary>文件不存在时抛出异常</summary>
	/// <param name="path"></param>
	/// <exception cref="T:System.IO.FileNotFoundException"></exception>
	public static void ThrowIfFileNotFound(string path) {
		if (!File.Exists(path))
			throw new FileNotFoundException("Can not found the specified file path. ", path);
	}

	/// <summary>文件不存在时抛出异常</summary>
	/// <param name="path"></param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException"></exception>
	public static void ThrowIfFolderNotFount(string path) {
		if (!Directory.Exists(path))
			throw new DirectoryNotFoundException($"Can not found the specified path {path}. ");
	}

	/// <summary>如果不是有效路径，抛出异常</summary>
	/// <param name="path"></param>
	/// <exception cref="T:System.IO.DirectoryNotFoundException"></exception>
	public static void ThrowIfInvalidPath(string path) {
		if (!File.Exists(path) && !Directory.Exists(path))
			throw new DirectoryNotFoundException($"The specified path is not a valid file or directory.  ({path})");
	}

	/// <summary>条件不成立时抛出异常</summary>
	/// <param name="condition">判断条件</param>
	/// <exception cref="T:System.InvalidOperationException"></exception>
	public static void ThrowIfNot(bool condition) {
		if (!condition)
			throw new InvalidOperationException();
	}

	/// <summary>任意一个条件不成立时抛出异常</summary>
	/// <param name="conditions">条件集合</param>
	/// <exception cref="T:System.InvalidOperationException"></exception>
	public static void ThrowIfNot(params bool[] conditions) {
		if (((IEnumerable<bool>)conditions).Any<bool>((Func<bool, bool>)(s => !s)))
			throw new InvalidOperationException();
	}

	/// <summary>条件不成立时抛出异常</summary>
	/// <param name="condition">条件表达式</param>
	public static void ThrowIfNot(Func<bool> condition) {
		Guards.ThrowIfNull((object)condition);
		Guards.ThrowIfNot(condition());
	}

	/// <summary>条件不成立时抛出异常</summary>
	/// <param name="message"></param>
	/// <param name="condition"></param>
	public static void ThrowMessageIfNot(string message, Func<bool> condition) {
		Guards.ThrowMessageIfNull(message, (object)condition);
		Guards.ThrowMessageIfNot(message, condition());
	}

	/// <summary>如果条件不成立的时候，抛出异常消息</summary>
	/// <param name="message"></param>
	/// <param name="condition"></param>
	/// <exception cref="T:System.Exception"></exception>
	private static void ThrowMessageIfNot(string message, bool condition) {
		if (!condition)
			throw new Exception(message);
	}
}