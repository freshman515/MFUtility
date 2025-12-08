using System.Windows;
using System.Windows.Controls;

namespace MFUtility.WPF.UI.Controls
{
    /// <summary>
    /// 💪 WPF Flex 容器（增强版：支持嵌套 Flex）
    /// 模拟 CSS Flexbox：
    /// - Flow(Row / Column / RowReverse / ColumnReverse)
    /// - Wrap / Gap / JustifyContent / AlignItems / AlignContent
    /// - 支持 FlexGrow / FlexShrink / FlexBasis
    /// - ✅ 支持嵌套 Flex（子 Flex 自动布局）
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

            double maxMain = 0;
            double totalCross = 0;
            double lineMain = 0;
            double lineCross = 0;

            double limitMain = isRow
                ? (double.IsInfinity(available.Width) ? double.MaxValue : available.Width)
                : (double.IsInfinity(available.Height) ? double.MaxValue : available.Height);

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Visible)
                    continue;

                // 🧩 支持嵌套 Flex：对子 Flex 调用其自身 Measure
                child.Measure(available);

                double main = isRow ? child.DesiredSize.Width : child.DesiredSize.Height;
                double cross = isRow ? child.DesiredSize.Height : child.DesiredSize.Width;

                if (Wrap != FlexWrap.NoWrap && lineMain > 0 && lineMain + main + gap > limitMain)
                {
                    maxMain = Math.Max(maxMain, lineMain);
                    totalCross += lineCross + gap;
                    lineMain = 0;
                    lineCross = 0;
                }

                lineMain += main + (lineMain > 0 ? gap : 0);
                lineCross = Math.Max(lineCross, cross);
            }

            maxMain = Math.Max(maxMain, lineMain);
            totalCross += lineCross;

            return isRow ? new Size(maxMain, totalCross) : new Size(totalCross, maxMain);
        }

        #endregion

        #region === Arrange ===

        protected override Size ArrangeOverride(Size final)
        {
            bool isRow = Flow == FlexFlow.Row || Flow == FlexFlow.RowReverse;
            bool reverse = Flow == FlexFlow.RowReverse || Flow == FlexFlow.ColumnReverse;

            double gap = Gap;
            double limitMain = isRow ? final.Width : final.Height;
            double crossLimit = isRow ? final.Height : final.Width;

            // === 1) 分行 ===
            List<List<UIElement>> lines = new();
            {
                List<UIElement> cur = new();
                double cursor = 0;
                foreach (UIElement child in InternalChildren)
                {
                    if (child.Visibility != Visibility.Visible) continue;
                    double main = isRow ? child.DesiredSize.Width : child.DesiredSize.Height;

                    if (Wrap != FlexWrap.NoWrap && cur.Count > 0 &&
                        cursor + main + gap > limitMain)
                    {
                        lines.Add(cur);
                        cur = new();
                        cursor = 0;
                    }
                    cur.Add(child);
                    cursor += (cur.Count > 1 ? gap : 0) + main;
                }
                if (cur.Count > 0) lines.Add(cur);
            }

            if (reverse) lines.Reverse();

            // === 2) Cross 尺寸 ===
            double[] lineCrossSizes = lines
                .Select(l => l.Max(c => isRow ? c.DesiredSize.Height : c.DesiredSize.Width))
                .ToArray();

            double totalCrossUsed = lineCrossSizes.Sum() + gap * Math.Max(0, lines.Count - 1);
            double crossLead = GetAlignContentLead(crossLimit, totalCrossUsed, lines.Count, AlignContent, gap);
            double crossCursor = crossLead;

            // === 3) 布局每一行 ===
            foreach (var line in lines)
            {
                int n = line.Count;
                double[] mainSizes = new double[n];
                double[] crossSizes = new double[n];
                double[] grows = new double[n];
                double[] shrinks = new double[n];

                for (int i = 0; i < n; i++)
                {
                    var c = line[i];
                    mainSizes[i] = isRow ? c.DesiredSize.Width : c.DesiredSize.Height;
                    crossSizes[i] = isRow ? c.DesiredSize.Height : c.DesiredSize.Width;
                    grows[i] = GetFlexGrow(c);
                    shrinks[i] = GetFlexShrink(c);
                }

                // === flex-grow / shrink ===
                double baseTotal = mainSizes.Sum() + gap * Math.Max(0, n - 1);
                double free0 = limitMain - baseTotal;

                if (free0 > 0)
                {
                    double totalGrow = grows.Sum();
                    if (totalGrow > 0)
                    {
                        for (int i = 0; i < n; i++)
                            mainSizes[i] += free0 * (grows[i] / totalGrow);
                    }
                }
                else if (free0 < 0)
                {
                    double overflow = -free0;
                    double shrinkWeight = 0;
                    for (int i = 0; i < n; i++)
                        shrinkWeight += shrinks[i] * Math.Max(0.0001, mainSizes[i]);
                    if (shrinkWeight > 0)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            double w = shrinks[i] * Math.Max(0.0001, mainSizes[i]) / shrinkWeight;
                            mainSizes[i] -= overflow * w;
                            if (mainSizes[i] < 0) mainSizes[i] = 0;
                        }
                    }
                }

                // === JustifyContent ===
                double baseTotal2 = mainSizes.Sum() + gap * Math.Max(0, n - 1);
                double free = Math.Max(0, limitMain - baseTotal2);
                (double lead, double betweenExtra) = GetJustifyLeadAndBetween(n, free, gap, JustifyContent);

                List<int> order = Enumerable.Range(0, n).ToList();
                if (reverse) order.Reverse();

                double lineCross = lineCrossSizes[lines.IndexOf(line)];
                double mainCursor = lead;

                // === 逐项布局 ===
                for (int k = 0; k < n; k++)
                {
                    int i = order[k];
                    var child = line[i];

                    double main = mainSizes[i];
                    double cross = crossSizes[i];
                    double alignOffset = GetAlignOffset(lineCross, cross, AlignItems);

                    double x = isRow ? mainCursor : crossCursor + alignOffset;
                    double y = isRow ? crossCursor + alignOffset : mainCursor;
                    double w = isRow ? main : cross;
                    double h = isRow ? cross : main;

                    if (AlignItems == FlexAlign.Stretch)
                    {
                        if (isRow)
                            h = lineCross;
                        else
                            w = lineCross;
                    }

                    // 🧩 嵌套 Flex：让子容器自由布局
                    child.Arrange(new Rect(x, y, w, h));

                    if (k < n - 1)
                        mainCursor += main + gap + betweenExtra;
                }

                crossCursor += lineCross + gap;
            }

            return final;
        }

        #endregion

        #region === Helper Methods ===

        private static (double lead, double betweenExtra) GetJustifyLeadAndBetween(int count, double free, double gap, FlexJustify mode)
        {
            if (count <= 0) return (0, 0);
            if (count == 1)
            {
                return mode switch
                {
                    FlexJustify.Center => (free / 2, 0),
                    FlexJustify.End => (free, 0),
                    FlexJustify.SpaceAround => (free / 2, 0),
                    FlexJustify.SpaceEvenly => (free / 2, 0),
                    _ => (0, 0),
                };
            }

            return mode switch
            {
                FlexJustify.Start => (0, 0),
                FlexJustify.Center => (free / 2, 0),
                FlexJustify.End => (free, 0),
                FlexJustify.SpaceBetween => (0, free / (count - 1)),
                FlexJustify.SpaceAround => (free / (2 * count), free / count),
                FlexJustify.SpaceEvenly => (free / (count + 1), free / (count + 1)),
                _ => (0, 0),
            };
        }

        private static double GetAlignOffset(double lineCross, double itemCross, FlexAlign align)
        {
            return align switch
            {
                FlexAlign.Center => (lineCross - itemCross) / 2,
                FlexAlign.End => lineCross - itemCross,
                _ => 0
            };
        }

        private static double GetAlignContentLead(double crossLimit, double used, int lines, FlexAlign align, double gap)
        {
            double free = Math.Max(0, crossLimit - used);
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
