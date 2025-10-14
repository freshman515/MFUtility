using System.Windows;
using System.Windows.Controls;

namespace MFUtility.Helpers
{
    public static class GridHelper
    {
        #region Rows
        public static string GetRows(DependencyObject obj)
            => (string)obj.GetValue(RowsProperty);
        public static void SetRows(DependencyObject obj, string value)
            => obj.SetValue(RowsProperty, value);

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.RegisterAttached(
                "Rows",
                typeof(string),
                typeof(GridHelper),
                new PropertyMetadata(null, OnLayoutChanged));
        #endregion

        #region Columns
        public static string GetColumns(DependencyObject obj)
            => (string)obj.GetValue(ColumnsProperty);
        public static void SetColumns(DependencyObject obj, string value)
            => obj.SetValue(ColumnsProperty, value);

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached(
                "Columns",
                typeof(string),
                typeof(GridHelper),
                new PropertyMetadata(null, OnLayoutChanged));
        #endregion

        #region Spacing
        public static double GetSpacing(DependencyObject obj)
            => (double)obj.GetValue(SpacingProperty);
        public static void SetSpacing(DependencyObject obj, double value)
            => obj.SetValue(SpacingProperty, value);

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.RegisterAttached(
                "Spacing",
                typeof(double),
                typeof(GridHelper),
                new PropertyMetadata(0.0, OnLayoutChanged));
        #endregion

        #region RowSpacing
        public static double GetRowSpacing(DependencyObject obj)
            => (double)obj.GetValue(RowSpacingProperty);
        public static void SetRowSpacing(DependencyObject obj, double value)
            => obj.SetValue(RowSpacingProperty, value);

        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.RegisterAttached(
                "RowSpacing",
                typeof(double),
                typeof(GridHelper),
                new PropertyMetadata(0.0, OnLayoutChanged));
        #endregion

        #region ColumnSpacing
        public static double GetColumnSpacing(DependencyObject obj)
            => (double)obj.GetValue(ColumnSpacingProperty);
        public static void SetColumnSpacing(DependencyObject obj, double value)
            => obj.SetValue(ColumnSpacingProperty, value);

        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.RegisterAttached(
                "ColumnSpacing",
                typeof(double),
                typeof(GridHelper),
                new PropertyMetadata(0.0, OnLayoutChanged));
        #endregion

        #region Orientation
        public static Orientation GetOrientation(DependencyObject obj)
            => (Orientation)obj.GetValue(OrientationProperty);
        public static void SetOrientation(DependencyObject obj, Orientation value)
            => obj.SetValue(OrientationProperty, value);

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.RegisterAttached(
                "Orientation",
                typeof(Orientation),
                typeof(GridHelper),
                new PropertyMetadata(Orientation.Horizontal, OnLayoutChanged));
        #endregion

        #region Uniform
        public static bool GetUniform(DependencyObject obj)
            => (bool)obj.GetValue(UniformProperty);
        public static void SetUniform(DependencyObject obj, bool value)
            => obj.SetValue(UniformProperty, value);

        public static readonly DependencyProperty UniformProperty =
            DependencyProperty.RegisterAttached(
                "Uniform",
                typeof(bool),
                typeof(GridHelper),
                new PropertyMetadata(false, OnLayoutChanged));
        #endregion

        #region Reverse
        public static bool GetReverse(DependencyObject obj)
            => (bool)obj.GetValue(ReverseProperty);
        public static void SetReverse(DependencyObject obj, bool value)
            => obj.SetValue(ReverseProperty, value);

        public static readonly DependencyProperty ReverseProperty =
            DependencyProperty.RegisterAttached(
                "Reverse",
                typeof(bool),
                typeof(GridHelper),
                new PropertyMetadata(false, OnLayoutChanged));
        #endregion

        #region ItemHorizontalAlignment
        public static HorizontalAlignment GetItemHorizontalAlignment(DependencyObject obj)
            => (HorizontalAlignment)obj.GetValue(ItemHorizontalAlignmentProperty);
        public static void SetItemHorizontalAlignment(DependencyObject obj, HorizontalAlignment value)
            => obj.SetValue(ItemHorizontalAlignmentProperty, value);

        public static readonly DependencyProperty ItemHorizontalAlignmentProperty =
            DependencyProperty.RegisterAttached(
                "ItemHorizontalAlignment",
                typeof(HorizontalAlignment),
                typeof(GridHelper),
                new PropertyMetadata(HorizontalAlignment.Stretch, OnLayoutChanged));
        #endregion

        #region ItemVerticalAlignment
        public static VerticalAlignment GetItemVerticalAlignment(DependencyObject obj)
            => (VerticalAlignment)obj.GetValue(ItemVerticalAlignmentProperty);
        public static void SetItemVerticalAlignment(DependencyObject obj, VerticalAlignment value)
            => obj.SetValue(ItemVerticalAlignmentProperty, value);

        public static readonly DependencyProperty ItemVerticalAlignmentProperty =
            DependencyProperty.RegisterAttached(
                "ItemVerticalAlignment",
                typeof(VerticalAlignment),
                typeof(GridHelper),
                new PropertyMetadata(VerticalAlignment.Stretch, OnLayoutChanged));
        #endregion


        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid) return;
            grid.Loaded -= GridOnLoaded;
            grid.Loaded += GridOnLoaded;
        }

        private static void GridOnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Grid grid) return;

            bool uniform = GetUniform(grid);
            double spacing = GetSpacing(grid);
            bool reverse = GetReverse(grid);
            var hAlign = GetItemHorizontalAlignment(grid);
            var vAlign = GetItemVerticalAlignment(grid);

            if (uniform)
            {
                ApplyUniformLayout(grid, spacing, reverse, hAlign, vAlign);
                return;
            }

            string rowsText = GetRows(grid);
            string colsText = GetColumns(grid);
            double rowSpacing = GetRowSpacing(grid);
            double colSpacing = GetColumnSpacing(grid);
            Orientation orientation = GetOrientation(grid);

            // Spacing 自动适配 Orientation
            if (spacing > 0)
            {
                if (orientation == Orientation.Vertical && rowSpacing == 0)
                    rowSpacing = spacing;
                else if (orientation == Orientation.Horizontal && colSpacing == 0)
                    colSpacing = spacing;
            }

            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            int childCount = grid.Children.Count;
            bool isManual = !string.IsNullOrWhiteSpace(rowsText) || !string.IsNullOrWhiteSpace(colsText);

            if (!isManual)
            {
                if (orientation == Orientation.Horizontal)
                {
                    CreateAutoColumns(grid, childCount, colSpacing);
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                else
                {
                    CreateAutoRows(grid, childCount, rowSpacing);
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(rowsText))
                    CreateRows(grid, rowsText, rowSpacing);
                if (!string.IsNullOrWhiteSpace(colsText))
                    CreateColumns(grid, colsText, colSpacing);
            }

            int baseRowCount = grid.RowDefinitions.Count > 0 ? grid.RowDefinitions.Count : 1;
            int baseColCount = grid.ColumnDefinitions.Count > 0 ? grid.ColumnDefinitions.Count : 1;

            if (rowSpacing > 0) baseRowCount = (baseRowCount + 1) / 2;
            if (colSpacing > 0) baseColCount = (baseColCount + 1) / 2;

            var children = grid.Children.Cast<UIElement>().ToList();
            if (reverse)
                children.Reverse();

            int index = 0;
            foreach (UIElement child in children)
            {
                int logicalRow, logicalCol;
                if (orientation == Orientation.Horizontal)
                {
                    logicalRow = index / baseColCount;
                    logicalCol = index % baseColCount;
                }
                else
                {
                    logicalRow = index % baseRowCount;
                    logicalCol = index / baseRowCount;
                }

                int actualRow = rowSpacing > 0 ? logicalRow * 2 : logicalRow;
                int actualCol = colSpacing > 0 ? logicalCol * 2 : logicalCol;

                Grid.SetRow(child, actualRow);
                Grid.SetColumn(child, actualCol);

                // 应用对齐属性
                if (child is FrameworkElement fe)
                {
                    fe.HorizontalAlignment = hAlign;
                    fe.VerticalAlignment = vAlign;
                }

                index++;
            }
        }

        private static void ApplyUniformLayout(Grid grid, double spacing, bool reverse,
            HorizontalAlignment hAlign, VerticalAlignment vAlign)
        {
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            int count = grid.Children.Count;
            if (count == 0) return;

            int cols = (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (int)Math.Ceiling((double)count / cols);

            for (int i = 0; i < rows; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            for (int j = 0; j < cols; j++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var children = grid.Children.Cast<UIElement>().ToList();
            if (reverse)
                children.Reverse();

            for (int index = 0; index < count; index++)
            {
                int row = index / cols;
                int col = index % cols;
                var child = children[index];
                Grid.SetRow(child, row);
                Grid.SetColumn(child, col);

                if (child is FrameworkElement fe)
                {
                    fe.HorizontalAlignment = hAlign;
                    fe.VerticalAlignment = vAlign;
                    if (spacing > 0)
                        fe.Margin = new Thickness(spacing / 2);
                }
            }

            grid.Margin = new Thickness(spacing / 2);
        }

        private static void CreateAutoRows(Grid grid, int count, double spacing)
        {
            for (int i = 0; i < count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                if (i < count - 1 && spacing > 0)
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(spacing, GridUnitType.Pixel) });
            }
        }

        private static void CreateAutoColumns(Grid grid, int count, double spacing)
        {
            for (int i = 0; i < count; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                if (i < count - 1 && spacing > 0)
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(spacing, GridUnitType.Pixel) });
            }
        }

        private static void CreateRows(Grid grid, string text, double spacing)
        {
var parts = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);            for (int i = 0; i < parts.Length; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = ParseGridLength(parts[i].Trim()) });
                if (i < parts.Length - 1 && spacing > 0)
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(spacing, GridUnitType.Pixel) });
            }
        }

        private static void CreateColumns(Grid grid, string text, double spacing)
        {
var parts = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
for (int i = 0; i < parts.Length; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = ParseGridLength(parts[i].Trim()) });
                if (i < parts.Length - 1 && spacing > 0)
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(spacing, GridUnitType.Pixel) });
            }
        }

        private static GridLength ParseGridLength(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new GridLength(1, GridUnitType.Star);

            value = value.Trim();

            if (value.Equals("auto", StringComparison.OrdinalIgnoreCase))
                return GridLength.Auto;

            if (value.EndsWith("*"))
            {
                var numText = value.TrimEnd('*');
                double weight = 1;
                if (!string.IsNullOrWhiteSpace(numText))
                    double.TryParse(numText, out weight);
                return new GridLength(weight, GridUnitType.Star);
            }

            if (double.TryParse(value, out var pixels))
                return new GridLength(pixels, GridUnitType.Pixel);

            return new GridLength(1, GridUnitType.Star);
        }
    }
}
