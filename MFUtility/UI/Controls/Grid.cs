using System.Diagnostics;

namespace MFUtility.UI.Controls {
	/// <summary>
	/// 🧩 智能增强版 Grid
	/// 支持：
	/// - Rows="auto auto"、Columns="100 *"（支持空格/逗号/中文逗号分隔）
	/// - 自动根据子元素数量补齐行或列（AutoPair）
	/// - 支持自动折行、Spacing、Uniform、Reverse、对齐等
	/// - FillMode 控制是否填满整个区域（Auto / Stretch）
	/// </summary>
	public class Grid : System.Windows.Controls.Grid {
		static Grid() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Grid),
				new FrameworkPropertyMetadata(typeof(Grid)));
		}

		public Grid() {
			Loaded += (_, _) => RebuildLayout();
		}

		#region === 依赖属性 ===

		public string Rows {
			get => (string)GetValue(RowsProperty);
			set => SetValue(RowsProperty, value);
		}

		public static readonly DependencyProperty RowsProperty =
			DependencyProperty.Register(nameof(Rows), typeof(string), typeof(Grid),
				new PropertyMetadata(null, OnLayoutChanged));

		public string Columns {
			get => (string)GetValue(ColumnsProperty);
			set => SetValue(ColumnsProperty, value);
		}

		public static readonly DependencyProperty ColumnsProperty =
			DependencyProperty.Register(nameof(Columns), typeof(string), typeof(Grid),
				new PropertyMetadata(null, OnLayoutChanged));

		public string Spacing {
			get => (string)GetValue(SpacingProperty);
			set => SetValue(SpacingProperty, value);
		}

		public static readonly DependencyProperty SpacingProperty =
			DependencyProperty.Register(nameof(Spacing), typeof(string), typeof(Grid),
				new PropertyMetadata("0", OnLayoutChanged));

		public double RowSpacing {
			get => (double)GetValue(RowSpacingProperty);
			set => SetValue(RowSpacingProperty, value);
		}

		public static readonly DependencyProperty RowSpacingProperty =
			DependencyProperty.Register(nameof(RowSpacing), typeof(double), typeof(Grid),
				new PropertyMetadata(0.0, OnLayoutChanged));

		public double ColumnSpacing {
			get => (double)GetValue(ColumnSpacingProperty);
			set => SetValue(ColumnSpacingProperty, value);
		}

		public static readonly DependencyProperty ColumnSpacingProperty =
			DependencyProperty.Register(nameof(ColumnSpacing), typeof(double), typeof(Grid),
				new PropertyMetadata(0.0, OnLayoutChanged));

		public Orientation Orientation {
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Grid),
				new PropertyMetadata(Orientation.Horizontal, OnLayoutChanged));

		public bool Uniform {
			get => (bool)GetValue(UniformProperty);
			set => SetValue(UniformProperty, value);
		}

		public static readonly DependencyProperty UniformProperty =
			DependencyProperty.Register(nameof(Uniform), typeof(bool), typeof(Grid),
				new PropertyMetadata(false, OnLayoutChanged));

		public bool Reverse {
			get => (bool)GetValue(ReverseProperty);
			set => SetValue(ReverseProperty, value);
		}

		public static readonly DependencyProperty ReverseProperty =
			DependencyProperty.Register(nameof(Reverse), typeof(bool), typeof(Grid),
				new PropertyMetadata(false, OnLayoutChanged));

		public HorizontalAlignment ItemHorizontalAlignment {
			get => (HorizontalAlignment)GetValue(ItemHorizontalAlignmentProperty);
			set => SetValue(ItemHorizontalAlignmentProperty, value);
		}

		public static readonly DependencyProperty ItemHorizontalAlignmentProperty =
			DependencyProperty.Register(nameof(ItemHorizontalAlignment), typeof(HorizontalAlignment), typeof(Grid),
				new PropertyMetadata(HorizontalAlignment.Stretch, OnLayoutChanged));

		public VerticalAlignment ItemVerticalAlignment {
			get => (VerticalAlignment)GetValue(ItemVerticalAlignmentProperty);
			set => SetValue(ItemVerticalAlignmentProperty, value);
		}

		public static readonly DependencyProperty ItemVerticalAlignmentProperty =
			DependencyProperty.Register(nameof(ItemVerticalAlignment), typeof(VerticalAlignment), typeof(Grid),
				new PropertyMetadata(VerticalAlignment.Stretch, OnLayoutChanged));

		/// <summary>
		/// 是否启用智能配对布局（只定义一维时自动补齐另一维）
		/// </summary>
		public bool AutoPair {
			get => (bool)GetValue(AutoPairProperty);
			set => SetValue(AutoPairProperty, value);
		}

		public static readonly DependencyProperty AutoPairProperty =
			DependencyProperty.Register(nameof(AutoPair), typeof(bool), typeof(Grid),
				new PropertyMetadata(true, OnLayoutChanged));

		/// <summary>
		/// 填充模式：Auto(内容自适应) 或 Stretch(拉伸填满)
		/// </summary>
		public GridFillMode FillMode {
			get => (GridFillMode)GetValue(FillModeProperty);
			set => SetValue(FillModeProperty, value);
		}

		public static readonly DependencyProperty FillModeProperty =
			DependencyProperty.Register(nameof(FillMode), typeof(GridFillMode), typeof(Grid),
				new PropertyMetadata(GridFillMode.Stretch, OnLayoutChanged));

		/// <summary>
		/// 调试模式：输出当前行列布局
		/// </summary>
		public bool DebugLayout {
			get => (bool)GetValue(DebugLayoutProperty);
			set => SetValue(DebugLayoutProperty, value);
		}

		public static readonly DependencyProperty DebugLayoutProperty =
			DependencyProperty.Register(nameof(DebugLayout), typeof(bool), typeof(Grid),
				new PropertyMetadata(false, OnLayoutChanged));

		#endregion

		private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Grid g && g.IsLoaded)
				g.RebuildLayout();
		}

		#region === 核心布局逻辑 ===

		private void RebuildLayout()
{
    if (!IsLoaded) return;

    var (spacingRow, spacingCol) = ParseSpacing(Spacing);
    double rowSpacing = RowSpacing == 0 ? spacingRow : RowSpacing;
    double colSpacing = ColumnSpacing == 0 ? spacingCol : ColumnSpacing;

    RowDefinitions.Clear();
    ColumnDefinitions.Clear();

    if (Uniform)
    {
        ApplyUniform();
        return;
    }

    bool hasRows = !string.IsNullOrWhiteSpace(Rows);
    bool hasCols = !string.IsNullOrWhiteSpace(Columns);
    int childCount = Children.Count;

    if (AutoPair)
    {
        if (hasCols && !hasRows)
        {
            var colDefs = SplitParts(Columns);
            int colCount = colDefs.Length;
            int rowCount = (int)Math.Ceiling((double)childCount / colCount);
            CreateCols(Columns);
            CreateAutoRows(rowCount);
        }
        else if (hasRows && !hasCols)
        {
            var rowDefs = SplitParts(Rows);
            int rowCount = rowDefs.Length;
            int colCount = (int)Math.Ceiling((double)childCount / rowCount);
            CreateRows(Rows);
            CreateAutoColumns(colCount);
        }
        else if (!hasRows && !hasCols)
        {
            CreateAutoRows(1);
            CreateAutoColumns(childCount);
        }
        else
        {
            CreateRows(Rows);
            CreateCols(Columns);
        }
    }
    else
    {
        if (hasRows) CreateRows(Rows);
        if (hasCols) CreateCols(Columns);
    }

    var list = Children.Cast<UIElement>().ToList();
    if (Reverse) list.Reverse();

    int totalRows = RowDefinitions.Count > 0 ? RowDefinitions.Count : 1;
    int totalCols = ColumnDefinitions.Count > 0 ? ColumnDefinitions.Count : 1;

    // 🧮 自动填充索引
    int currentRow = 0;
    int currentCol = 0;

    foreach (var child in list)
    {
        bool hasRow = child.ReadLocalValue(Grid.RowProperty) != DependencyProperty.UnsetValue;
        bool hasCol = child.ReadLocalValue(Grid.ColumnProperty) != DependencyProperty.UnsetValue;

        // ✅ 自动分配未定义位置的子项
        if (!hasRow && !hasCol)
        {
            SetRow(child, currentRow);
            SetColumn(child, currentCol);
        }
        else if (!hasRow && hasCol)
        {
            SetRow(child, currentRow);
        }
        else if (hasRow && !hasCol)
        {
            SetColumn(child, currentCol);
        }

        // 获取跨行跨列（默认1）
        int spanR = Math.Max(1, GetRowSpan(child));
        int spanC = Math.Max(1, GetColumnSpan(child));

        // ✅ 计算下一个位置（考虑跨列）
        currentCol += spanC;
        if (currentCol >= totalCols)
        {
            currentCol = 0;
            currentRow++;
        }

        if (child is FrameworkElement fe)
        {
            if (fe.ReadLocalValue(HorizontalAlignmentProperty) == DependencyProperty.UnsetValue)
                fe.HorizontalAlignment = ItemHorizontalAlignment;
            if (fe.ReadLocalValue(VerticalAlignmentProperty) == DependencyProperty.UnsetValue)
                fe.VerticalAlignment = ItemVerticalAlignment;

            fe.Margin = new Thickness(colSpacing / 2, rowSpacing / 2, colSpacing / 2, rowSpacing / 2);
        }
    }

    if (DebugLayout)
    {
        Debug.WriteLine($"[SmartGrid] {RowDefinitions.Count} rows × {ColumnDefinitions.Count} cols ({Children.Count} children)");
        foreach (UIElement child in Children)
        {
            Debug.WriteLine($"  → {child.GetType().Name} | Row={GetRow(child)}, Col={GetColumn(child)}, RowSpan={GetRowSpan(child)}, ColSpan={GetColumnSpan(child)}");
        }
    }
}
		private void ApplyUniform() {
			int count = Children.Count;
			if (count == 0) return;

			int cols = (int)Math.Ceiling(Math.Sqrt(count));
			int rows = (int)Math.Ceiling((double)count / cols);

			for (int i = 0; i < rows; i++)
				RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			for (int j = 0; j < cols; j++)
				ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		}

		#endregion

		#region === 工具方法 ===

		private void CreateAutoRows(int count) {
			var unit = FillMode == GridFillMode.Stretch ? GridUnitType.Star : GridUnitType.Auto;
			for (int i = 0; i < count; i++)
				RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, unit) });
		}

		private void CreateAutoColumns(int count) {
			var unit = FillMode == GridFillMode.Stretch ? GridUnitType.Star : GridUnitType.Auto;
			for (int i = 0; i < count; i++)
				ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, unit) });
		}

		private void CreateRows(string text) {
			var parts = SplitParts(text);
			foreach (var part in parts)
				RowDefinitions.Add(new RowDefinition { Height = ParseLength(part) });
		}

		private void CreateCols(string text) {
			var parts = SplitParts(text);
			foreach (var part in parts)
				ColumnDefinitions.Add(new ColumnDefinition { Width = ParseLength(part) });
		}

		private static string[] SplitParts(string text) {
			if (string.IsNullOrWhiteSpace(text))
				return Array.Empty<string>();

			var normalized = text
				.Replace('，', ' ')
				.Replace(',', ' ')
				.Trim();

			return normalized
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(p => p.Trim())
				.ToArray();
		}

		private static GridLength ParseLength(string val) {
			val = val?.Trim() ?? "*";
			if (val.Equals("auto", StringComparison.OrdinalIgnoreCase))
				return GridLength.Auto;

			if (val.EndsWith("*")) {
				if (double.TryParse(val.TrimEnd('*'), out double v))
					return new GridLength(v, GridUnitType.Star);
				return new GridLength(1, GridUnitType.Star);
			}

			if (double.TryParse(val, out double px))
				return new GridLength(px, GridUnitType.Pixel);

			return new GridLength(1, GridUnitType.Star);
		}

		private static (double row, double col) ParseSpacing(string? text) {
			if (string.IsNullOrWhiteSpace(text)) return (0, 0);
			text = text.Replace("，", ",").Replace(" ", ",");
			var p = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim()).ToArray();
			if (p.Length == 1 && double.TryParse(p[0], out double v))
				return (v, v);
			if (p.Length >= 2) {
				double.TryParse(p[0], out double r);
				double.TryParse(p[1], out double c);
				return (r, c);
			}

			return (0, 0);
		}

		#endregion
	}

	/// <summary>
	/// 控制自动生成行列是否拉伸填满
	/// </summary>
	public enum GridFillMode {
		Auto,
		Stretch
	}
}