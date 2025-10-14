using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MFUtility.Helpers;

/// <summary>
/// 为 TextBox 添加占位提示（Placeholder）
/// 示例：
/// <TextBox local:TextboxHelper.Placeholder="请输入内容..." />
/// </summary>
public static class TextboxHelper
{
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(string),
            typeof(TextboxHelper),
            new PropertyMetadata(null, OnPlaceholderChanged));

    public static string GetPlaceholder(DependencyObject obj) =>
        (string)obj.GetValue(PlaceholderProperty);

    public static void SetPlaceholder(DependencyObject obj, string value) =>
        obj.SetValue(PlaceholderProperty, value);

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
            return;

        textBox.Loaded -= TextBox_Loaded;
        textBox.TextChanged -= TextBox_TextChanged;
        textBox.Loaded += TextBox_Loaded;
        textBox.TextChanged += TextBox_TextChanged;

        ShowOrHidePlaceholder(textBox);
    }

    private static void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
            ShowOrHidePlaceholder(tb);
    }

    private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
            ShowOrHidePlaceholder(tb);
    }

    private static void ShowOrHidePlaceholder(TextBox textBox)
    {
        var layer = AdornerLayer.GetAdornerLayer(textBox);
        if (layer == null) return;

        var adorners = layer.GetAdorners(textBox);
        var existing = adorners?.OfType<PlaceholderAdorner>().FirstOrDefault();

        if (string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrEmpty(GetPlaceholder(textBox)))
        {
            if (existing == null)
            {
                layer.Add(new PlaceholderAdorner(textBox, GetPlaceholder(textBox)));
            }
        }
        else
        {
            if (existing != null)
                layer.Remove(existing);
        }
    }

    /// <summary>
    /// 内部类：绘制提示文字的 Adorner
    /// </summary>
    private sealed class PlaceholderAdorner : Adorner
    {
        private readonly string _placeholder;

        public PlaceholderAdorner(UIElement adornedElement, string placeholder)
            : base(adornedElement)
        {
            IsHitTestVisible = false; // 不阻止点击
            _placeholder = placeholder;
        }
protected override void OnRender(DrawingContext drawingContext)
{
    base.OnRender(drawingContext);

    if (AdornedElement is not TextBox textBox)
        return;

    string text = _placeholder;
    if (string.IsNullOrEmpty(text))
        return;

    // 使用 TextBox 的字体属性
    var typeface = new Typeface(
        textBox.FontFamily,
        textBox.FontStyle,
        textBox.FontWeight,
        textBox.FontStretch);

    var foreground = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128));
    foreground.Freeze();

    var formattedText = new FormattedText(
        text,
        System.Globalization.CultureInfo.CurrentCulture,
        textBox.FlowDirection,
        typeface,
        textBox.FontSize,
        foreground,
        VisualTreeHelper.GetDpi(this).PixelsPerDip);

    // === 计算 X 对齐（HorizontalContentAlignment） ===
    double x = textBox.Padding.Left;
    switch (textBox.HorizontalContentAlignment)
    {
        case HorizontalAlignment.Center:
            x = (textBox.ActualWidth - formattedText.Width) / 2;
            break;
        case HorizontalAlignment.Right:
            x = textBox.ActualWidth - formattedText.Width - textBox.Padding.Right - 2;
            break;
        case HorizontalAlignment.Stretch:
        case HorizontalAlignment.Left:
        default:
            x = textBox.Padding.Left + 2;
            break;
    }

    // === 计算 Y 对齐（VerticalContentAlignment） ===
    double y = textBox.Padding.Top;
    switch (textBox.VerticalContentAlignment)
    {
        case VerticalAlignment.Center:
            y = (textBox.ActualHeight - formattedText.Height) / 2;
            break;
        case VerticalAlignment.Bottom:
            y = textBox.ActualHeight - formattedText.Height - textBox.Padding.Bottom - 1;
            break;
        case VerticalAlignment.Stretch:
        case VerticalAlignment.Top:
        default:
            y = textBox.Padding.Top + 1;
            break;
    }

    drawingContext.DrawText(formattedText, new Point(x, y));
}

    }
}
