using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MFUtility.Helpers;

/// <summary>
/// ClickHelper v3 - 为任意控件添加完整交互命令绑定：
/// ✅ ClickCommand（单击）
/// ✅ DoubleClickCommand（双击）
/// ✅ RightClickCommand（右键）
/// ✅ LongPressCommand（长按）
/// ✅ SuppressSystemContextMenu（屏蔽系统右键菜单）
///
/// 示例：
/// <Border mf:ClickHelper.ClickCommand="{Binding ClickCmd}"
///         mf:ClickHelper.DoubleClickCommand="{Binding DoubleClickCmd}"
///         mf:ClickHelper.RightClickCommand="{Binding RightClickCmd}"
///         mf:ClickHelper.LongPressCommand="{Binding LongPressCmd}"
///         mf:ClickHelper.CommandParameter="{Binding .}"
///         mf:ClickHelper.ClickDelay="250"
///         mf:ClickHelper.LongPressDelay="600"
///         mf:ClickHelper.SuppressSystemContextMenu="True" />
/// </summary>
public static class ClickHelper
{
    #region === CommandParameter ===
    public static object GetCommandParameter(DependencyObject obj)
        => obj.GetValue(CommandParameterProperty);
    public static void SetCommandParameter(DependencyObject obj, object value)
        => obj.SetValue(CommandParameterProperty, value);

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(ClickHelper),
            new PropertyMetadata(null));
    #endregion

    #region === ClickCommand ===
    public static ICommand GetClickCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(ClickCommandProperty);
    public static void SetClickCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(ClickCommandProperty, value);

    public static readonly DependencyProperty ClickCommandProperty =
        DependencyProperty.RegisterAttached(
            "ClickCommand",
            typeof(ICommand),
            typeof(ClickHelper),
            new PropertyMetadata(null, OnCommandChanged));
    #endregion

    #region === DoubleClickCommand ===
    public static ICommand GetDoubleClickCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(DoubleClickCommandProperty);
    public static void SetDoubleClickCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(DoubleClickCommandProperty, value);

    public static readonly DependencyProperty DoubleClickCommandProperty =
        DependencyProperty.RegisterAttached(
            "DoubleClickCommand",
            typeof(ICommand),
            typeof(ClickHelper),
            new PropertyMetadata(null, OnCommandChanged));
    #endregion

    #region === RightClickCommand ===
    public static ICommand GetRightClickCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(RightClickCommandProperty);
    public static void SetRightClickCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(RightClickCommandProperty, value);

    public static readonly DependencyProperty RightClickCommandProperty =
        DependencyProperty.RegisterAttached(
            "RightClickCommand",
            typeof(ICommand),
            typeof(ClickHelper),
            new PropertyMetadata(null, OnCommandChanged));
    #endregion

    #region === LongPressCommand ===
    public static ICommand GetLongPressCommand(DependencyObject obj)
        => (ICommand)obj.GetValue(LongPressCommandProperty);
    public static void SetLongPressCommand(DependencyObject obj, ICommand value)
        => obj.SetValue(LongPressCommandProperty, value);

    public static readonly DependencyProperty LongPressCommandProperty =
        DependencyProperty.RegisterAttached(
            "LongPressCommand",
            typeof(ICommand),
            typeof(ClickHelper),
            new PropertyMetadata(null, OnCommandChanged));
    #endregion

    #region === ClickDelay ===
    public static int GetClickDelay(DependencyObject obj)
        => (int)obj.GetValue(ClickDelayProperty);
    public static void SetClickDelay(DependencyObject obj, int value)
        => obj.SetValue(ClickDelayProperty, value);

    public static readonly DependencyProperty ClickDelayProperty =
        DependencyProperty.RegisterAttached(
            "ClickDelay",
            typeof(int),
            typeof(ClickHelper),
            new PropertyMetadata(200));
    #endregion

    #region === LongPressDelay ===
    public static int GetLongPressDelay(DependencyObject obj)
        => (int)obj.GetValue(LongPressDelayProperty);
    public static void SetLongPressDelay(DependencyObject obj, int value)
        => obj.SetValue(LongPressDelayProperty, value);

    public static readonly DependencyProperty LongPressDelayProperty =
        DependencyProperty.RegisterAttached(
            "LongPressDelay",
            typeof(int),
            typeof(ClickHelper),
            new PropertyMetadata(600));
    #endregion

    #region === SuppressSystemContextMenu ===
    public static bool GetSuppressSystemContextMenu(DependencyObject obj)
        => (bool)obj.GetValue(SuppressSystemContextMenuProperty);
    public static void SetSuppressSystemContextMenu(DependencyObject obj, bool value)
        => obj.SetValue(SuppressSystemContextMenuProperty, value);

    public static readonly DependencyProperty SuppressSystemContextMenuProperty =
        DependencyProperty.RegisterAttached(
            "SuppressSystemContextMenu",
            typeof(bool),
            typeof(ClickHelper),
            new PropertyMetadata(false, OnSuppressChanged));
    #endregion

    // ==== 内部状态 ====
    private static DateTime _lastClickTime = DateTime.MinValue;
    private static UIElement? _lastClickedElement;
    private static bool _doubleClickDetected;
    private static bool _isPressing;

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        element.MouseRightButtonDown -= OnMouseRightButtonDown;

        bool needsLeft = GetClickCommand(element) != null ||
                         GetDoubleClickCommand(element) != null ||
                         GetLongPressCommand(element) != null;

        if (needsLeft)
        {
            element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        if (GetRightClickCommand(element) != null)
            element.MouseRightButtonDown += OnMouseRightButtonDown;
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element) return;

        _isPressing = true;
        var param = GetCommandParameter(element);
        int longPressDelay = GetLongPressDelay(element);

        var longCmd = GetLongPressCommand(element);
        if (longCmd != null)
        {
            Task.Run(async () =>
            {
                await Task.Delay(longPressDelay);
                if (_isPressing && longCmd.CanExecute(param))
                {
                    element.Dispatcher.Invoke(() => longCmd.Execute(param));
                    _isPressing = false;
                }
            });
        }
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element) return;

        _isPressing = false;

        var clickCmd = GetClickCommand(element);
        var dblCmd = GetDoubleClickCommand(element);
        var param = GetCommandParameter(element);
        int delay = GetClickDelay(element);

        DateTime now = DateTime.Now;
        bool isDoubleClick = _lastClickedElement == element && (now - _lastClickTime).TotalMilliseconds < delay;
        _lastClickTime = now;
        _lastClickedElement = element;

        if (isDoubleClick && dblCmd != null)
        {
            _doubleClickDetected = true;
            if (dblCmd.CanExecute(param))
                dblCmd.Execute(param);
            e.Handled = true;
        }
        else
        {
            _doubleClickDetected = false;
            element.Dispatcher.BeginInvoke(new Action(async () =>
            {
                await Task.Delay(delay);
                if (!_doubleClickDetected && clickCmd != null && clickCmd.CanExecute(param))
                    clickCmd.Execute(param);
            }), DispatcherPriority.Input);
        }
    }

    private static void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element) return;

        var rightCmd = GetRightClickCommand(element);
        var param = GetCommandParameter(element);
        int delay = GetClickDelay(element);

        if (rightCmd != null)
        {
            element.Dispatcher.BeginInvoke(new Action(async () =>
            {
                await Task.Delay(delay);
                if (rightCmd.CanExecute(param))
                    rightCmd.Execute(param);
            }), DispatcherPriority.Input);
            e.Handled = true;
        }
    }

    private static void OnSuppressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement fe) return;

        if ((bool)e.NewValue)
        {
            fe.PreviewMouseRightButtonDown += (_, args) => args.Handled = true;
            fe.ContextMenu = null; // 彻底禁用默认右键菜单
        }
    }
}
