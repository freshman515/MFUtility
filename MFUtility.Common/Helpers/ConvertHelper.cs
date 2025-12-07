#nullable enable

using System.Net;
using MFUtility.Extensions;

namespace MFUtility.Core.Helpers;

/// <summary>把文本转换为指定类型的数值</summary>
public static class ConvertHelper {
	public static byte? TryToByte(string text) {
		byte result;
		return byte.TryParse(text, out result) ? new byte?(result) : new byte?();
	}

	public static short? TryToInt16(string text) {
		short result;
		return short.TryParse(text, out result) ? new short?(result) : new short?();
	}

	public static int? TryToInt32(string text) {
		int result;
		return int.TryParse(text, out result) ? new int?(result) : new int?();
	}

	public static long? TryToInt64(string text) {
		long result;
		return long.TryParse(text, out result) ? new long?(result) : new long?();
	}

	public static ushort? TryToUInt16(string text) {
		ushort result;
		return ushort.TryParse(text, out result) ? new ushort?(result) : new ushort?();
	}

	public static uint? TryToUInt32(string text) {
		uint result;
		return uint.TryParse(text, out result) ? new uint?(result) : new uint?();
	}

	public static ulong? TryToUInt64(string text) {
		ulong result;
		return ulong.TryParse(text, out result) ? new ulong?(result) : new ulong?();
	}

	public static byte ToByte(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		byte result;
		if (byte.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static byte? ToByteNullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new byte?();
		byte result;
		if (byte.TryParse(text, out result))
			return new byte?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static short ToInt16(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		short result;
		if (short.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static short? ToInt16Nullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new short?();
		short result;
		if (short.TryParse(text, out result))
			return new short?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static int ToInt32(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		int result;
		if (int.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static int? ToInt32Nullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new int?();
		int result;
		if (int.TryParse(text, out result))
			return new int?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static long ToInt64(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		long result;
		if (long.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static long? ToInt64Nullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new long?();
		long result;
		if (long.TryParse(text, out result))
			return new long?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static ushort ToUInt16(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		ushort result;
		if (ushort.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static ushort? ToUInt16Nullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new ushort?();
		ushort result;
		if (ushort.TryParse(text, out result))
			return new ushort?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static uint ToUInt32(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		uint result;
		if (uint.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static uint? ToUInt32Nullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new uint?();
		uint result;
		if (uint.TryParse(text, out result))
			return new uint?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static ulong ToUInt64(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		ulong result;
		if (ulong.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static ulong? ToUInt64Nullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new ulong?();
		ulong result;
		if (ulong.TryParse(text, out result))
			return new ulong?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static float ToSingle(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		float result;
		if (float.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static float? ToSingleNullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new float?();
		float result;
		if (float.TryParse(text, out result))
			return new float?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static double ToDouble(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		double result;
		if (double.TryParse(text, out result))
			return result;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static double? ToDoubleNullable(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace())
			return new double?();
		double result;
		if (double.TryParse(text, out result))
			return new double?(result);
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static IPAddress ToIPAddress(string text, string name, Action action = null) {
		if (text.IsNullOrWhiteSpace()) {
			if (action != null)
				action();
			throw new Exception(name + "的值为空");
		}
		IPAddress address;
		if (IPAddress.TryParse(text, out address))
			return address;
		if (action != null)
			action();
		throw new Exception($"{name}的值不正确：{text}");
	}

	public static bool IsByteNullable(string text) {
		return text.IsNullOrWhiteSpace() || byte.TryParse(text, out byte _);
	}

	public static bool IsByte(string text) {
		return !text.IsNullOrWhiteSpace() && byte.TryParse(text, out byte _);
	}

	public static bool IsInt16Nullable(string text) {
		return text.IsNullOrWhiteSpace() || short.TryParse(text, out short _);
	}

	public static bool IsInt16(string text) {
		return !text.IsNullOrWhiteSpace() && short.TryParse(text, out short _);
	}

	public static bool IsInt32Nullable(string text) {
		return text.IsNullOrWhiteSpace() || int.TryParse(text, out int _);
	}

	public static bool IsInt32(string text) {
		return !text.IsNullOrWhiteSpace() && int.TryParse(text, out int _);
	}

	public static bool IsInt64Nullable(string text) {
		return text.IsNullOrWhiteSpace() || long.TryParse(text, out long _);
	}

	public static bool IsInt64(string text) {
		return !text.IsNullOrWhiteSpace() && long.TryParse(text, out long _);
	}

	public static bool IsUInt16Nullable(string text) {
		return text.IsNullOrWhiteSpace() || ushort.TryParse(text, out ushort _);
	}

	public static bool IsUInt16(string text) {
		return !text.IsNullOrWhiteSpace() && ushort.TryParse(text, out ushort _);
	}

	public static bool IsUInt32Nullable(string text) {
		return text.IsNullOrWhiteSpace() || uint.TryParse(text, out uint _);
	}

	public static bool IsUInt32(string text) {
		return !text.IsNullOrWhiteSpace() && uint.TryParse(text, out uint _);
	}

	public static bool IsUInt64Nullable(string text) {
		return text.IsNullOrWhiteSpace() || ulong.TryParse(text, out ulong _);
	}

	public static bool IsUInt64(string text) {
		return !text.IsNullOrWhiteSpace() && ulong.TryParse(text, out ulong _);
	}

	public static bool IsSingleNullable(string text) {
		return text.IsNullOrWhiteSpace() || float.TryParse(text, out float _);
	}

	public static bool IsSingle(string text) {
		return !text.IsNullOrWhiteSpace() && float.TryParse(text, out float _);
	}

	public static bool IsDoubleNullable(string text) {
		return text.IsNullOrWhiteSpace() || double.TryParse(text, out double _);
	}

	public static bool IsDouble(string text) {
		return !text.IsNullOrWhiteSpace() && double.TryParse(text, out double _);
	}
}