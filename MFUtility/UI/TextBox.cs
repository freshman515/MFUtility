using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MFUtility.UI;

/// <summary>
/// 增强版 TextBox，支持 Placeholder、圆角、占位颜色等。
/// 使用：<mf:TextBox Placeholder="请输入内容" CornerRadius="6" />
/// </summary>
public class TextBox : global::System.Windows.Controls.TextBox
{
    static TextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox),
            new FrameworkPropertyMetadata(typeof(TextBox)));
    }

    public TextBox()
    {
        Loaded += (_, _) => UpdatePlaceholder();
        TextChanged += (_, _) => UpdatePlaceholder();
    }

    #region === Dependency Properties ===

    /// <summary>占位文字</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(TextBox),
            new PropertyMetadata(null, OnPlaceholderChanged));

    /// <summary>占位文字颜色</summary>
    public Brush PlaceholderBrush
    {
        get => (Brush)GetValue(PlaceholderBrushProperty);
        set => SetValue(PlaceholderBrushProperty, value);
    }

    public static readonly DependencyProperty PlaceholderBrushProperty =
        DependencyProperty.Register(nameof(PlaceholderBrush), typeof(Brush), typeof(TextBox),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 128, 128, 128))));

    /// <summary>圆角</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TextBox),
            new PropertyMetadata(new CornerRadius(3)));

    #endregion

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox tb)
            tb.UpdatePlaceholder();
    }

    private void UpdatePlaceholder()
    {
        var layer = AdornerLayer.GetAdornerLayer(this);
        if (layer == null) return;

        var adorners = layer.GetAdorners(this);
        var existing = adorners?.OfType<PlaceholderAdorner>().FirstOrDefault();

        if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder))
        {
            if (existing == null)
                layer.Add(new PlaceholderAdorner(this, Placeholder, PlaceholderBrush));
        }
        else
        {
            if (existing != null)
                layer.Remove(existing);
        }
    }

    /// <summary>
    /// 内部类：用于绘制占位提示
    /// </summary>
    private sealed class PlaceholderAdorner : Adorner
    {
        private readonly string _placeholder;
        private readonly Brush _brush;

        public PlaceholderAdorner(UIElement adornedElement, string text, Brush brush)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            _placeholder = text;
            _brush = brush ?? Brushes.Gray;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement is not global::System.Windows.Controls.TextBox textBox)
                return;

            if (string.IsNullOrEmpty(_placeholder))
                return;

            var typeface = new Typeface(
                textBox.FontFamily,
                textBox.FontStyle,
                textBox.FontWeight,
                textBox.FontStretch);

            var ft = new FormattedText(
                _placeholder,
                System.Globalization.CultureInfo.CurrentCulture,
                textBox.FlowDirection,
                typeface,
                textBox.FontSize,
                _brush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            double x = textBox.Padding.Left + 2;
            double y = textBox.Padding.Top + 1;

            if (textBox.HorizontalContentAlignment == HorizontalAlignment.Center)
                x = (textBox.ActualWidth - ft.Width) / 2;
            else if (textBox.HorizontalContentAlignment == HorizontalAlignment.Right)
                x = textBox.ActualWidth - ft.Width - textBox.Padding.Right - 2;

            if (textBox.VerticalContentAlignment == VerticalAlignment.Center)
                y = (textBox.ActualHeight - ft.Height) / 2;
            else if (textBox.VerticalContentAlignment == VerticalAlignment.Bottom)
                y = textBox.ActualHeight - ft.Height - textBox.Padding.Bottom - 1;

            drawingContext.DrawText(ft, new Point(x, y));
        }
    }
}
