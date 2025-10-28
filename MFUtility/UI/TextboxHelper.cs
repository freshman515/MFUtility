using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MFUtility.UI;

/// <summary>
/// 为 System.Windows.Controls.TextBox 添加占位提示（Placeholder）
/// 使用示例：
/// <TextBox local:TextboxHelper.Placeholder="请输入内容..." />
/// </summary>
public static class TextboxHelper
{
    #region === 附加属性 ===

    /// <summary>占位提示文字</summary>
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

    #endregion

    #region === 事件逻辑 ===

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not global::System.Windows.Controls.TextBox textBox)
            return;

        textBox.Loaded -= TextBox_Loaded;
        textBox.TextChanged -= TextBox_TextChanged;
        textBox.Loaded += TextBox_Loaded;
        textBox.TextChanged += TextBox_TextChanged;

        ShowOrHidePlaceholder(textBox);
    }

    private static void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is global::System.Windows.Controls.TextBox tb)
            ShowOrHidePlaceholder(tb);
    }

    private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is global::System.Windows.Controls.TextBox tb)
            ShowOrHidePlaceholder(tb);
    }

    #endregion

    #region === 绘制逻辑 ===

    private static void ShowOrHidePlaceholder(global::System.Windows.Controls.TextBox textBox)
    {
        var layer = AdornerLayer.GetAdornerLayer(textBox);
        if (layer == null) return;

        var adorners = layer.GetAdorners(textBox);
        var existing = adorners?.OfType<PlaceholderAdorner>().FirstOrDefault();

        if (string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrEmpty(GetPlaceholder(textBox)))
        {
            if (existing == null)
                layer.Add(new PlaceholderAdorner(textBox, GetPlaceholder(textBox)));
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
            IsHitTestVisible = false; // 不阻止鼠标事件
            _placeholder = placeholder;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement is not global::System.Windows.Controls.TextBox textBox)
                return;

            if (string.IsNullOrEmpty(_placeholder))
                return;

            // 使用 TextBox 的字体样式属性
            var typeface = new Typeface(
                textBox.FontFamily,
                textBox.FontStyle,
                textBox.FontWeight,
                textBox.FontStretch);

            // 半透明灰色提示文字
            var foreground = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128));
            foreground.Freeze();

            var formattedText = new FormattedText(
                _placeholder,
                System.Globalization.CultureInfo.CurrentCulture,
                textBox.FlowDirection,
                typeface,
                textBox.FontSize,
                foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            // === X 对齐 ===
            double x = textBox.Padding.Left + 2;
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
                    break;
            }

            // === Y 对齐 ===
            double y = textBox.Padding.Top + 1;
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
                    break;
            }

            drawingContext.DrawText(formattedText, new Point(x, y));
        }
    }

    #endregion
}
