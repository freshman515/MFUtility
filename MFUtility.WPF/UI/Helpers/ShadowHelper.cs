using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MFUtility.WPF.UI.Helpers
{
    public static class ShadowHelper
    {
        public static bool GetEnableShadow(DependencyObject obj)
            => (bool)obj.GetValue(EnableShadowProperty);

        public static void SetEnableShadow(DependencyObject obj, bool value)
            => obj.SetValue(EnableShadowProperty, value);

        public static readonly DependencyProperty EnableShadowProperty =
            DependencyProperty.RegisterAttached(
                "EnableShadow",
                typeof(bool),
                typeof(ShadowHelper),
                new PropertyMetadata(false, OnShadowChanged));

        public static Color GetColor(DependencyObject obj)
            => (Color)obj.GetValue(ColorProperty);

        public static void SetColor(DependencyObject obj, Color value)
            => obj.SetValue(ColorProperty, value);

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.RegisterAttached(
                "Color",
                typeof(Color),
                typeof(ShadowHelper),
                new PropertyMetadata(Colors.Black));

        public static double GetBlurRadius(DependencyObject obj)
            => (double)obj.GetValue(BlurRadiusProperty);

        public static void SetBlurRadius(DependencyObject obj, double value)
            => obj.SetValue(BlurRadiusProperty, value);

        public static readonly DependencyProperty BlurRadiusProperty =
            DependencyProperty.RegisterAttached(
                "BlurRadius",
                typeof(double),
                typeof(ShadowHelper),
                new PropertyMetadata(10.0));

        public static double GetDepth(DependencyObject obj)
            => (double)obj.GetValue(DepthProperty);

        public static void SetDepth(DependencyObject obj, double value)
            => obj.SetValue(DepthProperty, value);

        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.RegisterAttached(
                "Depth",
                typeof(double),
                typeof(ShadowHelper),
                new PropertyMetadata(3.0));

        private static void OnShadowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element) return;

            if ((bool)e.NewValue)
            {
                element.Effect = new DropShadowEffect
                {
                    BlurRadius = GetBlurRadius(d),
                    ShadowDepth = GetDepth(d),
                    Color = GetColor(d),
                    Opacity = 0.5
                };
            }
            else
            {
                element.Effect = null;
            }
        }
    }
}
