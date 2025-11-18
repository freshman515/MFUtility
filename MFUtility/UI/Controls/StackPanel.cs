using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MFUtility.UI.Controls
{
    /// <summary>
    /// 🧩 增强版 StackPanel（支持间距序列、统一尺寸、自动大小、自适应换行）
    /// 支持：
    /// - Spacing="10" → 所有间距=10
    /// - Spacing="10 20" 或 "10,20" → 第1个间距=10，第2个间距=20，后续以20为准
    /// - AutoSize、ItemWidth、ItemHeight、WrapWhenOverflow
    /// </summary>
    public class StackPanel : System.Windows.Controls.StackPanel
    {
        static StackPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StackPanel),
                new FrameworkPropertyMetadata(typeof(StackPanel)));
        }

        public StackPanel()
        {
            Loaded += (_, _) => ApplyLayout();
            LayoutUpdated += (_, _) => ApplyLayout();
        }

        #region === 依赖属性 ===

        /// <summary>
        /// 子元素间距（支持多值："10 20 30" 或 "10,20"）
        /// </summary>
        public string Spacing
        {
            get => (string)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }
        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(nameof(Spacing), typeof(string), typeof(StackPanel),
                new PropertyMetadata("0", OnLayoutChanged));

        /// <summary>
        /// 是否根据内容自动调整宽高
        /// </summary>
        public bool AutoSize
        {
            get => (bool)GetValue(AutoSizeProperty);
            set => SetValue(AutoSizeProperty, value);
        }
        public static readonly DependencyProperty AutoSizeProperty =
            DependencyProperty.Register(nameof(AutoSize), typeof(bool), typeof(StackPanel),
                new PropertyMetadata(false, OnLayoutChanged));

        /// <summary>
        /// 当溢出时是否自动换行（仅支持横向）
        /// </summary>
        public bool WrapWhenOverflow
        {
            get => (bool)GetValue(WrapWhenOverflowProperty);
            set => SetValue(WrapWhenOverflowProperty, value);
        }
        public static readonly DependencyProperty WrapWhenOverflowProperty =
            DependencyProperty.Register(nameof(WrapWhenOverflow), typeof(bool), typeof(StackPanel),
                new PropertyMetadata(false, OnLayoutChanged));

        /// <summary>
        /// 子项统一宽度（可选）
        /// </summary>
        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(StackPanel),
                new PropertyMetadata(double.NaN, OnLayoutChanged));

        /// <summary>
        /// 子项统一高度（可选）
        /// </summary>
        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(StackPanel),
                new PropertyMetadata(double.NaN, OnLayoutChanged));

        // 新增四个 Margin 相关的依赖属性
        public double MarginTop
        {
            get => (double)GetValue(MarginTopProperty);
            set => SetValue(MarginTopProperty, value);
        }
        public static readonly DependencyProperty MarginTopProperty =
            DependencyProperty.Register(nameof(MarginTop), typeof(double), typeof(StackPanel),
                new PropertyMetadata(0.0, OnLayoutChanged));

        public double MarginLeft
        {
            get => (double)GetValue(MarginLeftProperty);
            set => SetValue(MarginLeftProperty, value);
        }
        public static readonly DependencyProperty MarginLeftProperty =
            DependencyProperty.Register(nameof(MarginLeft), typeof(double), typeof(StackPanel),
                new PropertyMetadata(0.0, OnLayoutChanged));

        public double MarginRight
        {
            get => (double)GetValue(MarginRightProperty);
            set => SetValue(MarginRightProperty, value);
        }
        public static readonly DependencyProperty MarginRightProperty =
            DependencyProperty.Register(nameof(MarginRight), typeof(double), typeof(StackPanel),
                new PropertyMetadata(0.0, OnLayoutChanged));

        public double MarginBottom
        {
            get => (double)GetValue(MarginBottomProperty);
            set => SetValue(MarginBottomProperty, value);
        }
        public static readonly DependencyProperty MarginBottomProperty =
            DependencyProperty.Register(nameof(MarginBottom), typeof(double), typeof(StackPanel),
                new PropertyMetadata(0.0, OnLayoutChanged));

        #endregion

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StackPanel sp)
                sp.ApplyLayout();
        }

        #region === 核心布局逻辑 ===

        private void ApplyLayout()
        {
            if (Children.Count == 0) return;

            var spacingValues = ParseSpacing(Spacing, Math.Max(Children.Count - 1, 1));
            bool autoSize = AutoSize;
            bool wrap = WrapWhenOverflow;
            double itemW = ItemWidth;
            double itemH = ItemHeight;

            double totalWidth = 0, totalHeight = 0;
            double maxWidth = 0, maxHeight = 0;

            int count = Children.Count;
            for (int i = 0; i < count; i++)
            {
                if (Children[i] is not FrameworkElement fe)
                    continue;

                // ✅ 应用间距序列
                double gap = (i < spacingValues.Length ? spacingValues[i] : spacingValues.LastOrDefault());
                if (Orientation == Orientation.Horizontal)
                    fe.Margin = new Thickness(MarginLeft, MarginTop, (i == count - 1 ? MarginRight : gap), MarginBottom);
                else
                    fe.Margin = new Thickness(MarginLeft, MarginTop, MarginRight, (i == count - 1 ? MarginBottom : gap));

                // ✅ 统一宽高（若设置）
                if (!double.IsNaN(itemW) && itemW > 0)
                    fe.Width = itemW;
                if (!double.IsNaN(itemH) && itemH > 0)
                    fe.Height = itemH;

                fe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (Orientation == Orientation.Horizontal)
                {
                    totalWidth += fe.DesiredSize.Width + (i == count - 1 ? 0 : gap);
                    maxHeight = Math.Max(maxHeight, fe.DesiredSize.Height);
                }
                else
                {
                    totalHeight += fe.DesiredSize.Height + (i == count - 1 ? 0 : gap);
                    maxWidth = Math.Max(maxWidth, fe.DesiredSize.Width);
                }
            }

            // === WrapWhenOverflow 模拟换行 ===
            if (wrap && Orientation == Orientation.Horizontal)
            {
                double availableWidth = ActualWidth > 0 ? ActualWidth : double.PositiveInfinity;
                double currentWidth = 0;
                double rowHeight = 0;
                double totalWrapHeight = 0;

                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i] is not FrameworkElement fe) continue;
                    double w = fe.DesiredSize.Width;
                    double h = fe.DesiredSize.Height;
                    double gap = (i < spacingValues.Length ? spacingValues[i] : spacingValues.LastOrDefault());

                    if (currentWidth + w > availableWidth && currentWidth > 0)
                    {
                        totalWrapHeight += rowHeight + gap;
                        currentWidth = 0;
                        rowHeight = 0;
                    }

                    currentWidth += w + gap;
                    rowHeight = Math.Max(rowHeight, h);
                }

                totalWrapHeight += rowHeight;
                totalHeight = totalWrapHeight;
            }

            // === AutoSize 自动调整容器尺寸 ===
            if (autoSize)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    Width = totalWidth;
                    Height = maxHeight;
                }
                else
                {
                    Width = maxWidth;
                    Height = totalHeight;
                }
            }
        }

        #endregion

        #region === Spacing 解析 ===

        private static double[] ParseSpacing(string text, int count)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Repeat(0.0, count).ToArray();

            text = text.Replace('，', ' ').Replace(',', ' ');
            var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var values = parts
                .Select(s => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0)
                .ToList();

            // 不足则填充，超出则截断
            while (values.Count < count)
                values.Add(values.LastOrDefault());
            if (values.Count > count)
                values = values.Take(count).ToList();

            return values.ToArray();
        }

        #endregion
    }
}
