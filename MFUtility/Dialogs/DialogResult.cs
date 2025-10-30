namespace MFUtility.Dialogs;


/// <summary>
/// 通用对话框结果对象。
/// </summary>
public class DialogResult
{
    /// <summary>对话框是否被确认（OK / Yes）</summary>
    public bool Confirmed { get; set; }

    /// <summary>取消或关闭</summary>
    public bool Canceled => !Confirmed;

    /// <summary>可扩展返回值（输入框的值等）</summary>
    public object? Data { get; set; }

    public static DialogResult Ok(object? data = null) => new() { Confirmed = true, Data = data };
    public static DialogResult Cancel(object? data = null) => new() { Confirmed = false, Data = data };
}
        