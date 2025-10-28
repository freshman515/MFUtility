namespace MFUtility.UI.Controls
{
    /// <summary>
    /// 🧩 增强版 StackPanel（支持间距、统一尺寸、自动大小、自适应换行）
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
        /// 子元素间距
        /// </summary>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }
        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(StackPanel),
                new PropertyMetadata(0.0, OnLayoutChanged));

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

            double spacing = Spacing;
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

                // 应用间距
                if (Orientation == Orientation.Horizontal)
                    fe.Margin = new Thickness(0, 0, (i == count - 1 ? 0 : spacing), 0);
                else
                    fe.Margin = new Thickness(0, 0, 0, (i == count - 1 ? 0 : spacing));

                // ✅ 统一宽高（若设置）
                if (!double.IsNaN(itemW) && itemW > 0)
                    fe.Width = itemW;
                if (!double.IsNaN(itemH) && itemH > 0)
                    fe.Height = itemH;

                fe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (Orientation == Orientation.Horizontal)
                {
                    totalWidth += fe.DesiredSize.Width + (i == count - 1 ? 0 : spacing);
                    maxHeight = Math.Max(maxHeight, fe.DesiredSize.Height);
                }
                else
                {
                    totalHeight += fe.DesiredSize.Height + (i == count - 1 ? 0 : spacing);
                    maxWidth = Math.Max(maxWidth, fe.DesiredSize.Width);
                }
            }

            // === WrapWhenOverflow 模拟轻量换行 ===
            if (wrap && Orientation == Orientation.Horizontal)
            {
                double availableWidth = ActualWidth > 0 ? ActualWidth : double.PositiveInfinity;
                double currentWidth = 0;
                double rowHeight = 0;
                double totalWrapHeight = 0;

                foreach (UIElement child in Children)
                {
                    if (child is not FrameworkElement fe) continue;
                    double w = fe.DesiredSize.Width;
                    double h = fe.DesiredSize.Height;

                    if (currentWidth + w > availableWidth && currentWidth > 0)
                    {
                        totalWrapHeight += rowHeight + spacing;
                        currentWidth = 0;
                        rowHeight = 0;
                    }

                    currentWidth += w + spacing;
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
    }
}
