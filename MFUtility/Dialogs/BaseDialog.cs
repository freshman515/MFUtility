using System.Windows.Media.Animation;

namespace MFUtility.Dialogs;

public abstract class BaseDialog : Window, IDialog
{
    protected BaseDialog()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        AllowsTransparency = true;
        ShowInTaskbar = false;
        ResizeMode = ResizeMode.NoResize;
        Background = System.Windows.Media.Brushes.Transparent;
    }

    public virtual DialogResult ShowDialog(Window? owner = null)
    {
        Owner = owner ?? Application.Current.MainWindow;
        BeginFadeIn();
        bool? result = base.ShowDialog();
        return new DialogResult { Confirmed = result == true };
    }

    public virtual async Task<DialogResult> ShowDialogAsync(Window? owner = null)
    {
        return await Task.Run(() => ShowDialog(owner));
    }

    public void Show(Window? owner = null)
    {
        Owner = owner ?? Application.Current.MainWindow;
        BeginFadeIn();
        base.Show();
    }

    public virtual object? DataContext { get; set; }

    protected void BeginFadeIn()
    {
        Opacity = 0;
        var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
        BeginAnimation(OpacityProperty, anim);
    }

    protected void BeginFadeOut()
    {
        var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(250));
        anim.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, anim);
    }
}