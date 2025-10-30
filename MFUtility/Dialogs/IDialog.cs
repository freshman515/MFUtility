namespace MFUtility.Dialogs;

public interface IDialog
{
    /// <summary>显示并返回结果</summary>
    DialogResult ShowDialog(Window? owner = null);

    /// <summary>异步显示</summary>
    Task<DialogResult> ShowDialogAsync(Window? owner = null);

    /// <summary>非模态显示</summary>
    void Show(Window? owner = null);

    /// <summary>绑定的数据上下文</summary>
    object? DataContext { get; set; }
}