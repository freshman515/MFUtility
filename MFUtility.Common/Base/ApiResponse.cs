namespace MFUtility.Core.Base;

public class ApiResponse {
	/// <summary>
	/// 是否成功
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// 返回消息
	/// </summary>
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// 错误码（可选）
	/// </summary>
	public int Code { get; set; } = 0;

	public static ApiResponse Ok(string message = "OK")
		=> new ApiResponse { Success = true, Message = message };

	public static ApiResponse Fail(string message, int code = -1)
		=> new ApiResponse { Success = false, Message = message, Code = code };
}

public class ApiResponse<T> {
	/// <summary>
	/// 是否成功
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// 返回数据
	/// </summary>
	public T? Data { get; set; }

	/// <summary>
	/// 消息
	/// </summary>
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// 错误码
	/// </summary>
	public int Code { get; set; }

	public static ApiResponse<T> Ok(T data, string message = "OK")
		=> new ApiResponse<T> { Success = true, Data = data, Message = message };

	public static ApiResponse<T> Fail(string message, int code = -1)
		=> new ApiResponse<T> { Success = false, Message = message, Code = code };

}