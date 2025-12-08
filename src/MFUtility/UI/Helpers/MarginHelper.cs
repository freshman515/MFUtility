namespace MFUtility.UI.Helpers
{
    public static class MarginHelper
    {
        public static Thickness GetUniformMargin(DependencyObject obj)
            => (Thickness)obj.GetValue(UniformMarginProperty);

        public static void SetUniformMargin(DependencyObject obj, Thickness value)
            => obj.SetValue(UniformMarginProperty, value);

        public static readonly DependencyProperty UniformMarginProperty =
            DependencyProperty.RegisterAttached(
                "UniformMargin",
                typeof(Thickness),
                typeof(MarginHelper),
                new PropertyMetadata(default(Thickness), OnUniformMarginChanged));

        private static void OnUniformMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                fe.Margin = (Thickness)e.NewValue;
            }

            // 如果设置在 Panel 上则应用到所有子元素
            if (d is Panel panel)
            {
                panel.Loaded += (_, _) =>
                {
                    foreach (UIElement child in panel.Children)
                    {
                        if (child is FrameworkElement childFe)
                            childFe.Margin = (Thickness)e.NewValue;
                    }
                };
            }
        }
    }
}
