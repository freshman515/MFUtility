using System.Windows;
using System.Windows.Controls;

namespace MFUtility.Helpers
{
    public static class StackPanelHelper
    {
        #region Spacing
        public static double GetSpacing(DependencyObject obj) =>
            (double)obj.GetValue(SpacingProperty);

        public static void SetSpacing(DependencyObject obj, double value) =>
            obj.SetValue(SpacingProperty, value);

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.RegisterAttached(
                "Spacing",
                typeof(double),
                typeof(StackPanelHelper),
                new PropertyMetadata(0.0, OnLayoutChanged));
        #endregion

        #region Reverse
        public static bool GetReverse(DependencyObject obj) =>
            (bool)obj.GetValue(ReverseProperty);

        public static void SetReverse(DependencyObject obj, bool value) =>
            obj.SetValue(ReverseProperty, value);

        public static readonly DependencyProperty ReverseProperty =
            DependencyProperty.RegisterAttached(
                "Reverse",
                typeof(bool),
                typeof(StackPanelHelper),
                new PropertyMetadata(false, OnLayoutChanged));
        #endregion

        #region AutoSize
        public static bool GetAutoSize(DependencyObject obj) =>
            (bool)obj.GetValue(AutoSizeProperty);

        public static void SetAutoSize(DependencyObject obj, bool value) =>
            obj.SetValue(AutoSizeProperty, value);

        public static readonly DependencyProperty AutoSizeProperty =
            DependencyProperty.RegisterAttached(
                "AutoSize",
                typeof(bool),
                typeof(StackPanelHelper),
                new PropertyMetadata(false, OnLayoutChanged));
        #endregion

        #region WrapWhenOverflow
        public static bool GetWrapWhenOverflow(DependencyObject obj) =>
            (bool)obj.GetValue(WrapWhenOverflowProperty);

        public static void SetWrapWhenOverflow(DependencyObject obj, bool value) =>
            obj.SetValue(WrapWhenOverflowProperty, value);

        public static readonly DependencyProperty WrapWhenOverflowProperty =
            DependencyProperty.RegisterAttached(
                "WrapWhenOverflow",
                typeof(bool),
                typeof(StackPanelHelper),
                new PropertyMetadata(false, OnLayoutChanged));
        #endregion


        // ================= 核心逻辑 =================

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not StackPanel panel) return;

            panel.Loaded -= PanelOnLoaded;
            panel.Loaded += PanelOnLoaded;

            panel.LayoutUpdated -= PanelOnLayoutUpdated;
            panel.LayoutUpdated += PanelOnLayoutUpdated;
        }

        private static void PanelOnLayoutUpdated(object? sender, EventArgs e)
        {
            ApplyLayout(sender as StackPanel);
        }

        private static void PanelOnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyLayout(sender as StackPanel);
        }

        private static void ApplyLayout(StackPanel? panel)
        {
            if (panel == null) return;
            if (panel.Children.Count == 0) return;

            double spacing = GetSpacing(panel);
            bool reverse = GetReverse(panel);
            bool autoSize = GetAutoSize(panel);
            bool wrap = GetWrapWhenOverflow(panel);

            // ✅ 真正反转 Children 集合的顺序
            if (reverse)
            {
                var reversed = panel.Children.Cast<UIElement>().Reverse().ToList();
                panel.Children.Clear();
                foreach (var child in reversed)
                    panel.Children.Add(child);
            }

            double totalWidth = 0, totalHeight = 0;
            double maxWidth = 0, maxHeight = 0;

            // === 计算每个元素的布局与间距 ===
            int count = panel.Children.Count;
            for (int i = 0; i < count; i++)
            {
                if (panel.Children[i] is not FrameworkElement fe)
                    continue;

                // 统一应用间距
                if (panel.Orientation == Orientation.Horizontal)
                    fe.Margin = new Thickness(0, 0, (i == count - 1 ? 0 : spacing), 0);
                else
                    fe.Margin = new Thickness(0, 0, 0, (i == count - 1 ? 0 : spacing));

                fe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (panel.Orientation == Orientation.Horizontal)
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
            if (wrap && panel.Orientation == Orientation.Horizontal)
            {
                double availableWidth = panel.ActualWidth > 0 ? panel.ActualWidth : double.PositiveInfinity;
                double currentWidth = 0;
                double rowHeight = 0;
                double totalWrapHeight = 0;

                foreach (UIElement child in panel.Children)
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

            // === 自动调整 StackPanel 尺寸 ===
            if (autoSize)
            {
                if (panel.Orientation == Orientation.Horizontal)
                {
                    panel.Width = totalWidth;
                    panel.Height = maxHeight;
                }
                else
                {
                    panel.Width = maxWidth;
                    panel.Height = totalHeight;
                }
            }
        }
    }
}
