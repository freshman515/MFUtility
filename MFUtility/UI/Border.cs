using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace MFUtility.UI
{
    /// <summary>
    /// 🧩 超强版 Border
    /// ✅ 支持 Corner="15 5 30 10"（空格/逗号皆可）
    /// ✅ Hover / Pressed 仅改变背景，不再动画阴影
    /// ✅ 支持四角独立圆角、Padding、阴影、动画、裁剪
    /// </summary>
    public class Border : Decorator
    {
        private DropShadowEffect? _shadowEffect;

        static Border()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Border),
                new FrameworkPropertyMetadata(typeof(Border)));
        }

        public Border()
        {
            SizeChanged += (_, _) => UpdateClip();
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            MouseLeftButtonDown += OnMouseDown;
            MouseLeftButtonUp += OnMouseUp;
        }

        #region === 依赖属性 ===

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(Border),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(Border),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public double BorderThickness
        {
            get => (double)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }
        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register(nameof(BorderThickness), typeof(double), typeof(Border),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>支持空格或逗号的圆角文本</summary>
        public string Corner
        {
            get => (string)GetValue(CornerProperty);
            set => SetValue(CornerProperty, value);
        }
        public static readonly DependencyProperty CornerProperty =
            DependencyProperty.Register(nameof(Corner), typeof(string), typeof(Border),
                new PropertyMetadata("0", OnCornerTextChanged));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Border),
                new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender, OnCornerChanged));

        public bool ClipContent
        {
            get => (bool)GetValue(ClipContentProperty);
            set => SetValue(ClipContentProperty, value);
        }
        public static readonly DependencyProperty ClipContentProperty =
            DependencyProperty.Register(nameof(ClipContent), typeof(bool), typeof(Border),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnCornerChanged));

        public bool OuterStroke
        {
            get => (bool)GetValue(OuterStrokeProperty);
            set => SetValue(OuterStrokeProperty, value);
        }
        public static readonly DependencyProperty OuterStrokeProperty =
            DependencyProperty.Register(nameof(OuterStroke), typeof(bool), typeof(Border),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(Border),
                new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Brush HoverBrush
        {
            get => (Brush)GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }
        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register(nameof(HoverBrush), typeof(Brush), typeof(Border),
                new PropertyMetadata(null));

        public Brush PressedBrush
        {
            get => (Brush)GetValue(PressedBrushProperty);
            set => SetValue(PressedBrushProperty, value);
        }
        public static readonly DependencyProperty PressedBrushProperty =
            DependencyProperty.Register(nameof(PressedBrush), typeof(Brush), typeof(Border),
                new PropertyMetadata(null));

        public bool EnableShadow
        {
            get => (bool)GetValue(EnableShadowProperty);
            set => SetValue(EnableShadowProperty, value);
        }
        public static readonly DependencyProperty EnableShadowProperty =
            DependencyProperty.Register(nameof(EnableShadow), typeof(bool), typeof(Border),
                new FrameworkPropertyMetadata(false, OnShadowChanged));

        public double ShadowDepth
        {
            get => (double)GetValue(ShadowDepthProperty);
            set => SetValue(ShadowDepthProperty, value);
        }
        public static readonly DependencyProperty ShadowDepthProperty =
            DependencyProperty.Register(nameof(ShadowDepth), typeof(double), typeof(Border),
                new PropertyMetadata(8.0));

        #endregion

        #region === Corner 文本解析 ===

        private static void OnCornerTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border b && e.NewValue is string s)
                b.CornerRadius = ParseCornerRadius(s);
        }

        private static CornerRadius ParseCornerRadius(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new CornerRadius(0);

            text = text.Replace("，", " ").Replace(",", " ");
            var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0)
                            .ToArray();

            return parts.Length switch
            {
                1 => new CornerRadius(parts[0]),
                2 => new CornerRadius(parts[0], parts[1], parts[0], parts[1]),
                3 => new CornerRadius(parts[0], parts[1], parts[2], parts[1]),
                >= 4 => new CornerRadius(parts[0], parts[1], parts[2], parts[3]),
                _ => new CornerRadius(0)
            };
        }

        #endregion

        #region === 阴影、圆角更新 ===

        private static void OnCornerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border b) b.UpdateClip();
        }

        private static void OnShadowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Border b) return;

            if (b.EnableShadow)
            {
                b._shadowEffect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 15,
                    ShadowDepth = b.ShadowDepth,
                    Opacity = 0.3
                };
                b.Effect = b._shadowEffect;
            }
            else b.Effect = null;
        }

        #endregion

        #region === 布局 ===

        protected override Size MeasureOverride(Size constraint)
        {
            if (Child != null)
            {
                var p = Padding;
                var inner = new Size(
                    Math.Max(0, constraint.Width - p.Left - p.Right),
                    Math.Max(0, constraint.Height - p.Top - p.Bottom));
                Child.Measure(inner);
                var desired = Child.DesiredSize;
                return new Size(desired.Width + p.Left + p.Right,
                                desired.Height + p.Top + p.Bottom);
            }
            return new Size(Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child != null)
            {
                var p = Padding;
                var rect = new Rect(p.Left, p.Top,
                    Math.Max(0, arrangeSize.Width - p.Left - p.Right),
                    Math.Max(0, arrangeSize.Height - p.Top - p.Bottom));
                Child.Arrange(rect);
            }
            UpdateClip();
            return arrangeSize;
        }

        #endregion

        #region === 绘制 ===

        protected override void OnRender(DrawingContext dc)
        {
            double w = ActualWidth, h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            var rect = new Rect(0, 0, w, h);
            var geo = CreateRoundRectGeometry(rect, CornerRadius);

            if (Background != null)
                dc.DrawGeometry(Background, null, geo);

            if (BorderBrush != null && BorderThickness > 0)
            {
                double half = BorderThickness / 2.0;
                var borderRect = OuterStroke
                    ? new Rect(-half, -half, w + BorderThickness, h + BorderThickness)
                    : new Rect(half, half, w - BorderThickness, h - BorderThickness);
                var borderGeo = CreateRoundRectGeometry(borderRect, CornerRadius);
                dc.DrawGeometry(null, new Pen(BorderBrush, BorderThickness), borderGeo);
            }
        }

        private static Geometry CreateRoundRectGeometry(Rect rect, CornerRadius r)
        {
            var g = new StreamGeometry();
            double l = rect.Left, t = rect.Top, rgt = rect.Right, btm = rect.Bottom;
            double tl = r.TopLeft, tr = r.TopRight, br = r.BottomRight, bl = r.BottomLeft;
            double k = 0.55228475;

            using (var c = g.Open())
            {
                c.BeginFigure(new Point(l, t + tl), true, true);

                if (tl > 0)
                    c.BezierTo(new Point(l, t + tl - tl * k), new Point(l + tl - tl * k, t), new Point(l + tl, t), true, false);
                else c.LineTo(new Point(l, t), true, false);

                c.LineTo(new Point(rgt - tr, t), true, false);
                if (tr > 0)
                    c.BezierTo(new Point(rgt - tr + tr * k, t), new Point(rgt, t + tr - tr * k), new Point(rgt, t + tr), true, false);

                c.LineTo(new Point(rgt, btm - br), true, false);
                if (br > 0)
                    c.BezierTo(new Point(rgt, btm - br + br * k), new Point(rgt - br + br * k, btm), new Point(rgt - br, btm), true, false);

                c.LineTo(new Point(l + bl, btm), true, false);
                if (bl > 0)
                    c.BezierTo(new Point(l + bl - bl * k, btm), new Point(l, btm - bl + bl * k), new Point(l, btm - bl), true, false);

                c.Close();
            }

            g.Freeze();
            return g;
        }

        private void UpdateClip()
        {
            if (!ClipContent || Child == null) return;
            var rect = new Rect(0, 0, ActualWidth, ActualHeight);
            Child.Clip = CreateRoundRectGeometry(rect, CornerRadius);
        }

        #endregion

        #region === 动画 ===

        private void OnMouseEnter(object s, MouseEventArgs e)
        {
            if (HoverBrush != null)
                AnimateBackgroundTo(HoverBrush);
        }

        private void OnMouseLeave(object s, MouseEventArgs e)
        {
            AnimateBackgroundTo(Background);
        }

        private void OnMouseDown(object s, MouseButtonEventArgs e)
        {
            if (PressedBrush != null)
                AnimateBackgroundTo(PressedBrush);
        }

        private void OnMouseUp(object s, MouseButtonEventArgs e)
        {
            AnimateBackgroundTo(HoverBrush ?? Background);
        }

        private void AnimateBackgroundTo(Brush? target)
        {
            if (target == null) return;
            var anim = new BrushAnimation
            {
                From = Background,
                To = target,
                Duration = TimeSpan.FromSeconds(0.25)
            };
            BeginAnimation(BackgroundProperty, anim);
        }

        #endregion
    }

    public class BrushAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(Brush);
        public Brush? From { get; set; }
        public Brush? To { get; set; }

        public override object GetCurrentValue(object origin, object dest, AnimationClock clock)
        {
            double p = clock.CurrentProgress ?? 0.0;
            if (From is SolidColorBrush a && To is SolidColorBrush b)
            {
                Color c = Color.FromArgb(
                    (byte)(a.Color.A + (b.Color.A - a.Color.A) * p),
                    (byte)(a.Color.R + (b.Color.R - a.Color.R) * p),
                    (byte)(a.Color.G + (b.Color.G - a.Color.G) * p),
                    (byte)(a.Color.B + (b.Color.B - a.Color.B) * p));
                return new SolidColorBrush(c);
            }
            return To ?? dest;
        }

        protected override Freezable CreateInstanceCore() => new BrushAnimation();
    }
}
