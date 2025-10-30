namespace MFUtility.Dialogs;

/// <summary>
/// 通用对话框服务接口：
/// - 注册不同类型的对话框实现
/// - 统一管理调用与生命周期
/// </summary>
public interface IDialogService
{
    /// <summary>注册一个对话框类型（通常在启动时）</summary>
    void Register<TDialog>(string key) where TDialog : IDialog, new();

    /// <summary>显示一个对话框（通过 Key 调用）</summary>
    DialogResult Show(string key, object? parameter = null);

    /// <summary>异步显示</summary>
    Task<DialogResult> ShowAsync(string key, object? parameter = null);

    /// <summary>泛型快捷调用</summary>
    DialogResult Show<TDialog>(object? parameter = null) where TDialog : IDialog, new();
}