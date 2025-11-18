using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MFUtility.Extensions;

namespace MFUtility.WPF.UI.Helpers;

public static class TextboxHelper
{
    #region === 附加属性 ===

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

    public static readonly DependencyProperty PlaceholderBrushProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderBrush",
            typeof(Brush),
            typeof(TextboxHelper),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)), OnPlaceholderChanged));

    public static Brush GetPlaceholderBrush(DependencyObject obj) =>
        (Brush)obj.GetValue(PlaceholderBrushProperty);

    public static void SetPlaceholderBrush(DependencyObject obj, Brush value) =>
        obj.SetValue(PlaceholderBrushProperty, value);

    public static readonly DependencyProperty PlaceholderOpacityProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderOpacity",
            typeof(double),
            typeof(TextboxHelper),
            new PropertyMetadata(0.5, OnPlaceholderChanged));

    public static double GetPlaceholderOpacity(DependencyObject obj) =>
        (double)obj.GetValue(PlaceholderOpacityProperty);

    public static void SetPlaceholderOpacity(DependencyObject obj, double value) =>
        obj.SetValue(PlaceholderOpacityProperty, value);

    public static readonly DependencyProperty PlaceholderFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderFontSize",
            typeof(double),
            typeof(TextboxHelper),
            new PropertyMetadata(double.NaN, OnPlaceholderChanged));

    public static double GetPlaceholderFontSize(DependencyObject obj) =>
        (double)obj.GetValue(PlaceholderFontSizeProperty);

    public static void SetPlaceholderFontSize(DependencyObject obj, double value) =>
        obj.SetValue(PlaceholderFontSizeProperty, value);

    #endregion

    #region === 事件逻辑 ===

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
            return;

        textBox.Loaded -= TextBox_Loaded;
        textBox.TextChanged -= TextBox_TextChanged;
        textBox.GotFocus -= TextBox_FocusChanged;
        textBox.LostFocus -= TextBox_FocusChanged;

        textBox.Loaded += TextBox_Loaded;
        textBox.TextChanged += TextBox_TextChanged;
        textBox.GotFocus += TextBox_FocusChanged;
        textBox.LostFocus += TextBox_FocusChanged;

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

    private static void TextBox_FocusChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
            ShowOrHidePlaceholder(tb);
    }

    #endregion

    #region === 绘制逻辑 ===

    private static void ShowOrHidePlaceholder(TextBox textBox)
    {
        var layer = AdornerLayer.GetAdornerLayer(textBox);
        if (layer == null) return;

        var adorners = layer.GetAdorners(textBox);
        var existing = adorners?.OfType<PlaceholderAdorner>().FirstOrDefault();

        // ✅ 仅当“未聚焦 + 内容为空”时显示占位符
        bool shouldShow = string.IsNullOrEmpty(textBox.Text)
                          && !string.IsNullOrEmpty(GetPlaceholder(textBox))
                          && !textBox.IsKeyboardFocused;

        if (shouldShow)
        {
            if (existing == null)
            {
                var adorner = new PlaceholderAdorner(
                    textBox,
                    GetPlaceholder(textBox),
                    GetPlaceholderBrush(textBox),
                    GetPlaceholderOpacity(textBox),
                    GetPlaceholderFontSize(textBox));
                layer.Add(adorner);
                adorner.StartFadeAnimation(1); // 初次淡入
            }
        }
        else
        {
            if (existing != null)
            {
                existing.StartFadeAnimation(0); // 淡出动画
                // 动画结束后再移除
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(320)
                };
                timer.Tick += (s, _) =>
                {
                    layer.Remove(existing);
                    timer.Stop();
                };
                timer.Start();
            }
        }
    }

    /// <summary>绘制提示文字的 Adorner</summary>
    private sealed class PlaceholderAdorner : Adorner
    {
        private readonly string _placeholder;
        private readonly Brush _brush;
        private readonly double _baseOpacity;
        private readonly double _fontSize;

        public PlaceholderAdorner(UIElement adornedElement, string placeholder, Brush brush, double opacity, double fontSize)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            _placeholder = placeholder;
            _brush = brush;
            _baseOpacity = opacity.Clamp(0, 1);
            _fontSize = double.IsNaN(fontSize) || fontSize <= 0 ? double.NaN : fontSize;
        }

        public void StartFadeAnimation(double target)
        {
            var animation = new DoubleAnimation(target, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase()
            };
            var clock = animation.CreateClock();
            ApplyAnimationClock(AnimationOpacityProperty, clock);
        }

        public static readonly DependencyProperty AnimationOpacityProperty =
            DependencyProperty.Register(nameof(AnimationOpacity), typeof(double), typeof(PlaceholderAdorner),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double AnimationOpacity
        {
            get => (double)GetValue(AnimationOpacityProperty);
            set => SetValue(AnimationOpacityProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement is not TextBox textBox)
                return;
            if (string.IsNullOrEmpty(_placeholder))
                return;

            var typeface = new Typeface(
                textBox.FontFamily,
                textBox.FontStyle,
                textBox.FontWeight,
                textBox.FontStretch);

            double actualFontSize = double.IsNaN(_fontSize) ? textBox.FontSize : _fontSize;

            var brush = _brush.Clone();
            brush.Opacity = _baseOpacity * AnimationOpacity;

            var formattedText = new FormattedText(
                _placeholder,
                System.Globalization.CultureInfo.CurrentCulture,
                textBox.FlowDirection,
                typeface,
                actualFontSize,
                brush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            double x = textBox.Padding.Left + 2;
            switch (textBox.HorizontalContentAlignment)
            {
                case HorizontalAlignment.Center:
                    x = (textBox.ActualWidth - formattedText.Width) / 2;
                    break;
                case HorizontalAlignment.Right:
                    x = textBox.ActualWidth - formattedText.Width - textBox.Padding.Right - 2;
                    break;
            }

            double y = textBox.Padding.Top + 1;
            switch (textBox.VerticalContentAlignment)
            {
                case VerticalAlignment.Center:
                    y = (textBox.ActualHeight - formattedText.Height) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    y = textBox.ActualHeight - formattedText.Height - textBox.Padding.Bottom - 1;
                    break;
            }

            drawingContext.DrawText(formattedText, new Point(x, y));
        }
    }

    #endregion
}
