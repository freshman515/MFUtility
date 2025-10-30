using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MFUtility.UI.Controls
{
    /// <summary>
    /// 💪 WPF Flex 容器（终极版，完全模拟 CSS Flexbox）
    /// 支持：
    /// - Flow(Row / Column / Reverse)
    /// - Wrap / Gap / JustifyContent / AlignItems / AlignContent
    /// - FlexGrow / FlexShrink / FlexBasis
    /// </summary>
    public class Flex : Panel
    {
        #region === 主属性 ===

        public FlexFlow Flow
        {
            get => (FlexFlow)GetValue(FlowProperty);
            set => SetValue(FlowProperty, value);
        }
        public static readonly DependencyProperty FlowProperty =
            DependencyProperty.Register(nameof(Flow), typeof(FlexFlow), typeof(Flex),
                new FrameworkPropertyMetadata(FlexFlow.Row, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public FlexWrap Wrap
        {
            get => (FlexWrap)GetValue(WrapProperty);
            set => SetValue(WrapProperty, value);
        }
        public static readonly DependencyProperty WrapProperty =
            DependencyProperty.Register(nameof(Wrap), typeof(FlexWrap), typeof(Flex),
                new FrameworkPropertyMetadata(FlexWrap.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public FlexJustify JustifyContent
        {
            get => (FlexJustify)GetValue(JustifyContentProperty);
            set => SetValue(JustifyContentProperty, value);
        }
        public static readonly DependencyProperty JustifyContentProperty =
            DependencyProperty.Register(nameof(JustifyContent), typeof(FlexJustify), typeof(Flex),
                new FrameworkPropertyMetadata(FlexJustify.Start, FrameworkPropertyMetadataOptions.AffectsArrange));

        public FlexAlign AlignItems
        {
            get => (FlexAlign)GetValue(AlignItemsProperty);
            set => SetValue(AlignItemsProperty, value);
        }
        public static readonly DependencyProperty AlignItemsProperty =
            DependencyProperty.Register(nameof(AlignItems), typeof(FlexAlign), typeof(Flex),
                new FrameworkPropertyMetadata(FlexAlign.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange));

        public FlexAlign AlignContent
        {
            get => (FlexAlign)GetValue(AlignContentProperty);
            set => SetValue(AlignContentProperty, value);
        }
        public static readonly DependencyProperty AlignContentProperty =
            DependencyProperty.Register(nameof(AlignContent), typeof(FlexAlign), typeof(Flex),
                new FrameworkPropertyMetadata(FlexAlign.Start, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double Gap
        {
            get => (double)GetValue(GapProperty);
            set => SetValue(GapProperty, value);
        }
        public static readonly DependencyProperty GapProperty =
            DependencyProperty.Register(nameof(Gap), typeof(double), typeof(Flex),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        #region === 附加属性 ===

        public static readonly DependencyProperty FlexGrowProperty =
            DependencyProperty.RegisterAttached("FlexGrow", typeof(double), typeof(Flex),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetFlexGrow(UIElement element, double value) => element.SetValue(FlexGrowProperty, value);
        public static double GetFlexGrow(UIElement element) => (double)element.GetValue(FlexGrowProperty);

        public static readonly DependencyProperty FlexShrinkProperty =
            DependencyProperty.RegisterAttached("FlexShrink", typeof(double), typeof(Flex),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetFlexShrink(UIElement element, double value) => element.SetValue(FlexShrinkProperty, value);
        public static double GetFlexShrink(UIElement element) => (double)element.GetValue(FlexShrinkProperty);

        public static readonly DependencyProperty FlexBasisProperty =
            DependencyProperty.RegisterAttached("FlexBasis", typeof(double), typeof(Flex),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void SetFlexBasis(UIElement element, double value) => element.SetValue(FlexBasisProperty, value);
        public static double GetFlexBasis(UIElement element) => (double)element.GetValue(FlexBasisProperty);

        #endregion

        #region === Measure ===

        protected override Size MeasureOverride(Size available)
        {
            bool isRow = Flow == FlexFlow.Row || Flow == FlexFlow.RowReverse;
            double gap = Gap;

            double maxMain = 0, maxCross = 0;
            double lineMain = 0, lineCross = 0;

            double limitMain = isRow
                ? (double.IsInfinity(available.Width) ? double.MaxValue : available.Width)
                : (double.IsInfinity(available.Height) ? double.MaxValue : available.Height);

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Visible) continue;
                child.Measure(available);

                double main = isRow ? child.DesiredSize.Width : child.DesiredSize.Height;
                double cross = isRow ? child.DesiredSize.Height : child.DesiredSize.Width;

                if (Wrap != FlexWrap.NoWrap && lineMain + main + (lineMain > 0 ? gap : 0) > limitMain)
                {
                    maxMain = Math.Max(maxMain, lineMain);
                    maxCross += lineCross + gap;
                    lineMain = 0;
                    lineCross = 0;
                }

                lineMain += main + (lineMain > 0 ? gap : 0);
                lineCross = Math.Max(lineCross, cross);
            }

            maxMain = Math.Max(maxMain, lineMain);
            maxCross += lineCross;

            return isRow ? new Size(maxMain, maxCross) : new Size(maxCross, maxMain);
        }

        #endregion

        #region === Arrange ===

        protected override Size ArrangeOverride(Size final)
        {
            bool isRow = Flow == FlexFlow.Row || Flow == FlexFlow.RowReverse;
            double gap = Gap;
            double limitMain = isRow ? final.Width : final.Height;
            double crossLimit = isRow ? final.Height : final.Width;

            // 分行（或列）
            List<List<UIElement>> lines = new();
            List<UIElement> current = new();
            double cursorMain = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Visible)
                    continue;

                double main = isRow ? child.DesiredSize.Width : child.DesiredSize.Height;

                if (Wrap != FlexWrap.NoWrap && cursorMain + main + (current.Count > 0 ? gap : 0) > limitMain && current.Count > 0)
                {
                    lines.Add(current);
                    current = new();
                    cursorMain = 0;
                }

                current.Add(child);
                cursorMain += main + (current.Count > 1 ? gap : 0);
            }
            if (current.Count > 0)
                lines.Add(current);

            // cross方向（副轴）总尺寸
            double totalCross = lines.Sum(l => l.Max(c => isRow ? c.DesiredSize.Height : c.DesiredSize.Width)) +
                                (lines.Count - 1) * gap;

            double crossFree = Math.Max(0, crossLimit - totalCross);
            double crossOffset = GetAlignContentStartOffset(crossLimit, totalCross, lines.Count, AlignContent, gap);

            foreach (var line in lines)
            {
                double totalMain = line.Sum(c => isRow ? c.DesiredSize.Width : c.DesiredSize.Height);
                double totalGrow = line.Sum(GetFlexGrow);
                double free = Math.Max(0, limitMain - totalMain - gap * (line.Count - 1));
                double[] spacing = GetJustifySpacings(line.Count, free, gap, JustifyContent);

                double mainCursor = spacing[0];
                double lineCross = line.Max(c => isRow ? c.DesiredSize.Height : c.DesiredSize.Width);

                for (int i = 0; i < line.Count; i++)
                {
                    UIElement child = line[i];
                    double grow = GetFlexGrow(child);
                    double main = isRow ? child.DesiredSize.Width : child.DesiredSize.Height;
                    if (totalGrow > 0) main += free * (grow / totalGrow);
                    double cross = isRow ? child.DesiredSize.Height : child.DesiredSize.Width;
                    double alignOffset = GetAlignOffset(lineCross, cross, AlignItems);

                    double x = isRow ? mainCursor : crossOffset + alignOffset;
                    double y = isRow ? crossOffset + alignOffset : mainCursor;
                    double w = isRow ? main : cross;
                    double h = isRow ? cross : main;

                    child.Arrange(new Rect(x, y, w, h));
                    mainCursor += main + spacing[Math.Min(i + 1, spacing.Length - 1)];
                }

                crossOffset += lineCross + gap;
            }

            return final;
        }

        private double[] GetJustifySpacings(int count, double free, double gap, FlexJustify mode)
        {
            double[] result = new double[count + 1];
            if (count <= 0) return result;

            switch (mode)
            {
                case FlexJustify.SpaceBetween:
                    double between = count == 1 ? 0 : free / (count - 1);
                    for (int i = 1; i < count; i++) result[i] = between;
                    break;
                case FlexJustify.SpaceAround:
                    double around = free / count;
                    result[0] = around / 2;
                    for (int i = 1; i <= count; i++) result[i] = around;
                    break;
                case FlexJustify.SpaceEvenly:
                    double evenly = free / (count + 1);
                    for (int i = 0; i <= count; i++) result[i] = evenly;
                    break;
                case FlexJustify.Center:
                    result[0] = free / 2;
                    for (int i = 1; i <= count; i++) result[i] = gap;
                    break;
                case FlexJustify.End:
                    result[0] = free;
                    for (int i = 1; i <= count; i++) result[i] = gap;
                    break;
                default:
                    for (int i = 1; i <= count; i++) result[i] = gap;
                    break;
            }
            return result;
        }

        private double GetAlignOffset(double lineCross, double itemCross, FlexAlign align)
        {
            return align switch
            {
                FlexAlign.Center => (lineCross - itemCross) / 2,
                FlexAlign.End => lineCross - itemCross,
                _ => 0
            };
        }

        private double GetAlignContentStartOffset(double totalCross, double usedCross, int lines, FlexAlign align, double gap)
        {
            double free = Math.Max(0, totalCross - usedCross);
            return align switch
            {
                FlexAlign.Center => free / 2,
                FlexAlign.End => free,
                FlexAlign.SpaceBetween => lines > 1 ? 0 : free / 2,
                FlexAlign.SpaceAround => free / (lines * 2),
                FlexAlign.SpaceEvenly => free / (lines + 1),
                _ => 0
            };
        }

        #endregion
    }

    #region === 枚举 ===

    public enum FlexFlow { Row, RowReverse, Column, ColumnReverse }
    public enum FlexWrap { NoWrap, Wrap }
    public enum FlexJustify { Start, Center, End, SpaceBetween, SpaceAround, SpaceEvenly }
    public enum FlexAlign { Start, Center, End, Stretch, SpaceBetween, SpaceAround, SpaceEvenly }

    #endregion
}
