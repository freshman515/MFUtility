using System.Windows.Input;

namespace MFUtility.UI;

public class KeyBindingHelper {
	
	#region Enter绑定命令

	public static ICommand GetEnterCommand(DependencyObject obj) =>
		(ICommand)obj.GetValue(EnterCommandProperty);

	public static void SetEnterCommand(DependencyObject obj, ICommand value) =>
		obj.SetValue(EnterCommandProperty, value);

	public static readonly DependencyProperty EnterCommandProperty =
		DependencyProperty.RegisterAttached(
			"EnterCommand",
			typeof(ICommand),
			typeof(KeyBindingHelper),
			new PropertyMetadata(null, OnEnterChanged));

	private static void OnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is UIElement element) {
			element.KeyDown += (sender, args) => {
				if (args.Key == Key.Enter) {
					var cmd = GetEnterCommand(element);
					if (cmd.CanExecute(null) == true) {
						cmd.Execute(null);
					}
				}
			};
		}
	}

	#endregion


	#region Escape关闭窗口

	public static bool GetCloseOnEscape(DependencyObject obj) =>
		(bool)obj.GetValue(CloseOnEscapeProperty);

	public static void SetCloseOnEscape(DependencyObject obj, bool value) =>
		obj.SetValue(CloseOnEscapeProperty, value);

	public static readonly DependencyProperty CloseOnEscapeProperty =
		DependencyProperty.RegisterAttached(
			"CloseOnEscape",
			typeof(bool),
			typeof(KeyBindingHelper),
			new PropertyMetadata(false, OnCloseOnEscapeChanged));

	private static void OnCloseOnEscapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is not Window window) {
			return;
		}

		if ((bool)e.NewValue) {
			window.PreviewKeyDown += WindowOnPreviewKeyDown;
		} else {
			window.PreviewKeyDown -= WindowOnPreviewKeyDown;
		}
	}

	private static void WindowOnPreviewKeyDown(object sender, KeyEventArgs e) {
		if (sender is Window window && e.Key == Key.Escape) {
			window.Close();
			e.Handled = true;
		}
	}

	#endregion


	#region Escape绑定命令

	public static ICommand GetEscapeCommand(DependencyObject obj)
		=> (ICommand)obj.GetValue(EscapeCommandProperty);

	public static void SetEscapeCommand(DependencyObject obj, ICommand value)
		=> obj.SetValue(EscapeCommandProperty, value);

	public static readonly DependencyProperty EscapeCommandProperty =
		DependencyProperty.RegisterAttached(
			"EscapeCommand",
			typeof(ICommand),
			typeof(KeyBindingHelper),
			new PropertyMetadata(null, OnEscapeCommandChanged));

	private static void OnEscapeCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is UIElement element) {
			element.KeyDown += (s, args) => {
				if (args.Key == Key.Escape) {
					var cmd = GetEscapeCommand(element);
					if (cmd?.CanExecute(null) == true)
						cmd.Execute(null);
				}
			};
		}
	}

	#endregion

	#region ShortcutKey + Command 绑定

	public static string GetShortcutKey(DependencyObject obj)
		=> (string)obj.GetValue(ShortcutKeyProperty);

	public static void SetShortcutKey(DependencyObject obj, string value)
		=> obj.SetValue(ShortcutKeyProperty, value);

	public static readonly DependencyProperty ShortcutKeyProperty =
		DependencyProperty.RegisterAttached(
			"ShortcutKey",
			typeof(string),
			typeof(KeyBindingHelper),
			new PropertyMetadata(null, OnShortcutKeyChanged));

	public static ICommand GetCommand(DependencyObject obj)
		=> (ICommand)obj.GetValue(CommandProperty);

	public static void SetCommand(DependencyObject obj, ICommand value)
		=> obj.SetValue(CommandProperty, value);

	public static readonly DependencyProperty CommandProperty =
		DependencyProperty.RegisterAttached(
			"Command",
			typeof(ICommand),
			typeof(KeyBindingHelper),
			new PropertyMetadata(null, OnCommandChanged));

	private static void OnShortcutKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		TryAttachKeyBinding(d);
	}

	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		TryAttachKeyBinding(d);
	}

	private static void TryAttachKeyBinding(DependencyObject d) {
		if (d is not UIElement element) return;

		var keyText = GetShortcutKey(element);
		var cmd = GetCommand(element);
		if (string.IsNullOrWhiteSpace(keyText) || cmd == null)
			return;

		// 解析组合键字符串
		var parts = keyText.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
			.Select(x => x.Trim().ToUpperInvariant()).ToArray();

		Key key = Key.None;
		ModifierKeys modifiers = ModifierKeys.None;

		foreach (var p in parts) {
			switch (p) {
				case "CTRL": modifiers |= ModifierKeys.Control; break;
				case "ALT": modifiers |= ModifierKeys.Alt; break;
				case "SHIFT": modifiers |= ModifierKeys.Shift; break;
				default:
					if (Enum.TryParse(p, true, out Key parsed))
						key = parsed;
					break;
			}
		}

		if (key == Key.None) return;

		// 检查是否已存在同样的 KeyBinding，避免重复绑定
		if (element.InputBindings.OfType<KeyBinding>()
		    .Any(kb => kb.Key == key && kb.Modifiers == modifiers))
			return;

		// 添加新的 KeyBinding
		element.InputBindings.Add(new KeyBinding(cmd, key, modifiers));
	}

	#endregion
}