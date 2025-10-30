using System.Collections.Concurrent;

namespace MFUtility.Dialogs;
public class DialogService : IDialogService
{
    private readonly ConcurrentDictionary<string, Func<IDialog>> _registry = new();

    public void Register<TDialog>(string key) where TDialog : IDialog, new()
    {
        _registry[key] = () => new TDialog();
    }

    public DialogResult Show(string key, object? parameter = null)
    {
        if (!_registry.TryGetValue(key, out var factory))
            throw new InvalidOperationException($"未注册对话框：{key}");

        var dialog = factory();
        dialog.DataContext = parameter;
        return dialog.ShowDialog(Application.Current.MainWindow);
    }

    public async Task<DialogResult> ShowAsync(string key, object? parameter = null)
    {
        if (!_registry.TryGetValue(key, out var factory))
            throw new InvalidOperationException($"未注册对话框：{key}");

        var dialog = factory();
        dialog.DataContext = parameter;
        return await dialog.ShowDialogAsync(Application.Current.MainWindow);
    }

    public DialogResult Show<TDialog>(object? parameter = null) where TDialog : IDialog, new()
    {
        var key = typeof(TDialog).FullName!;
        Register<TDialog>(key);
        return Show(key, parameter);
    }
}