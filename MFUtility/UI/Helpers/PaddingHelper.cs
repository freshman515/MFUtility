namespace MFUtility.UI.Helpers
{
    /// <summary>
    /// 为单个控件或整个容器统一设置 Padding。
    /// 示例：
    /// <StackPanel local:PaddingHelper.UniformPadding="8,4">
    ///     <Button Content="按钮1" />
    ///     <Button Content="按钮2" />
    /// </StackPanel>
    /// </summary>
    public static class PaddingHelper
    {
        public static Thickness GetUniformPadding(DependencyObject obj)
            => (Thickness)obj.GetValue(UniformPaddingProperty);

        public static void SetUniformPadding(DependencyObject obj, Thickness value)
            => obj.SetValue(UniformPaddingProperty, value);

        public static readonly DependencyProperty UniformPaddingProperty =
            DependencyProperty.RegisterAttached(
                "UniformPadding",
                typeof(Thickness),
                typeof(PaddingHelper),
                new PropertyMetadata(default(Thickness), OnUniformPaddingChanged));

        private static void OnUniformPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newPadding = (Thickness)e.NewValue;

            // 1️⃣ 如果目标本身支持 Padding 属性，则直接应用
            if (d is Control control)
            {
                control.Padding = newPadding;
            }
            else if (d is System.Windows.Controls.Border border)
            {
                border.Padding = newPadding;
            }

            // 2️⃣ 如果作用在 Panel 上，则为其所有支持 Padding 的子元素设置
            if (d is Panel panel)
            {
                panel.Loaded += (_, _) =>
                {
                    foreach (UIElement child in panel.Children)
                    {
                        switch (child)
                        {
                            case Control c:
                                c.Padding = newPadding;
                                break;
                            case System.Windows.Controls.Border b:
                                b.Padding = newPadding;
                                break;
                        }
                    }
                };
            }
        }
    }
}
