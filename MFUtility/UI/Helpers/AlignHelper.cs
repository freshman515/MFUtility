namespace MFUtility.UI.Helpers
{
    public static class AlignHelper
    {
        public static HorizontalAlignment GetHorizontal(DependencyObject obj)
            => (HorizontalAlignment)obj.GetValue(HorizontalProperty);

        public static void SetHorizontal(DependencyObject obj, HorizontalAlignment value)
            => obj.SetValue(HorizontalProperty, value);

        public static readonly DependencyProperty HorizontalProperty =
            DependencyProperty.RegisterAttached(
                "Horizontal",
                typeof(HorizontalAlignment),
                typeof(AlignHelper),
                new PropertyMetadata(HorizontalAlignment.Stretch, OnAlignChanged));

        public static VerticalAlignment GetVertical(DependencyObject obj)
            => (VerticalAlignment)obj.GetValue(VerticalProperty);

        public static void SetVertical(DependencyObject obj, VerticalAlignment value)
            => obj.SetValue(VerticalProperty, value);

        public static readonly DependencyProperty VerticalProperty =
            DependencyProperty.RegisterAttached(
                "Vertical",
                typeof(VerticalAlignment),
                typeof(AlignHelper),
                new PropertyMetadata(VerticalAlignment.Stretch, OnAlignChanged));

        private static void OnAlignChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Panel panel) return;

            panel.Loaded += (_, _) =>
            {
                foreach (UIElement child in panel.Children)
                {
                    if (child is FrameworkElement fe)
                    {
                        fe.HorizontalAlignment = GetHorizontal(panel);
                        fe.VerticalAlignment = GetVertical(panel);
                    }
                }
            };
        }
    }
}
