using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MFUtility.Helpers
{
    /// <summary>
    /// 为 StackPanel、WrapPanel、UniformGrid 等容器添加统一子项间距（ItemSpacing）。
    /// 示例：
    /// <StackPanel Orientation="Horizontal" local:SpacingHelper.ItemSpacing="8">
    ///     <Button Content="确定" />
    ///     <Button Content="取消" />
    ///     <Button Content="关闭" />
    /// </StackPanel>
    /// </summary>
    public static class SpacingHelper
    {
        public static double GetSpacing(DependencyObject obj)
            => (double)obj.GetValue(SpacingProperty);

        public static void SetSpacing(DependencyObject obj, double value)
            => obj.SetValue(SpacingProperty, value);

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.RegisterAttached(
                "Spacing",
                typeof(double),
                typeof(SpacingHelper),
                new PropertyMetadata(0d, OnItemSpacingChanged));

        private static void OnItemSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Panel panel)
                return;

            panel.Loaded -= Panel_Loaded;
            panel.Loaded += Panel_Loaded;
        }

        private static void Panel_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            double spacing = GetSpacing(panel);
            if (spacing <= 0) return;

            // 根据容器类型和方向应用间距
            if (panel is StackPanel stack)
            {
                bool isHorizontal = stack.Orientation == Orientation.Horizontal;

                for (int i = 0; i < stack.Children.Count; i++)
                {
                    if (stack.Children[i] is FrameworkElement child)
                    {
                        var margin = child.Margin;

                        if (isHorizontal)
                        {
                            // 最后一个不加右边距
                            margin.Right = (i == stack.Children.Count - 1) ? 0 : spacing;
                            margin.Bottom = 0;
                        }
                        else
                        {
                            // 最后一个不加下边距
                            margin.Bottom = (i == stack.Children.Count - 1) ? 0 : spacing;
                            margin.Right = 0;
                        }

                        child.Margin = margin;
                    }
                }
            }
            else if (panel is WrapPanel wrap)
            {
                // WrapPanel 无方向限制，左右下均加一定间距
                foreach (UIElement element in wrap.Children)
                {
                    if (element is FrameworkElement fe)
                        fe.Margin = new Thickness(0, 0, spacing, spacing);
                }
            }
            else if (panel is UniformGrid grid)
            {
                // UniformGrid：均匀间距，所有方向一致
                foreach (UIElement element in grid.Children)
                {
                    if (element is FrameworkElement fe)
                        fe.Margin = new Thickness(spacing / 2);
                }
            }
        }
    }
}
