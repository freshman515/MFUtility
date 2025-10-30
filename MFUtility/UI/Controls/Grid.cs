using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

namespace MFUtility.UI.Controls {
	/// <summary>
	/// 🧩 智能增强版 Grid（带外层边框）
	/// 支持：
	/// - Rows="auto auto"、Columns="100 *"（支持空格/逗号/中文逗号分隔）
	/// - 自动根据子元素数量补齐行或列（AutoPair）
	/// - 支持自动折行、Spacing、Uniform、Reverse、对齐等
	/// - FillMode 控制是否填满整个区域（Auto / Stretch）
	/// - ItemMargin / ItemPadding / Padding 支持“8 1”“12 34 4 5”
	/// - Item 背景、圆角、边框、尺寸限制
	/// - ✅ 新增外层边框 BorderBrush / BorderThickness / CornerRadius / Padding
	/// </summary>
	public class Grid : System.Windows.Controls.Grid {
		static Grid() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Grid),
				new FrameworkPropertyMetadata(typeof(Grid)));
		}

		private Border? _outerBorder; // 用于包裹自身的外层边框

		public Grid() {
			Loaded += OnLoaded;
		}

		private void OnLoaded(object? sender, RoutedEventArgs e) {
			if (_outerBorder == null)
				WrapWithBorder();

			// ✅ 加这一段：在外层边框创建后应用背景图或颜色
			if (BackgroundImage != null) {
				_outerBorder!.Background = new ImageBrush(BackgroundImage) {
					Stretch = Stretch.UniformToFill,
					AlignmentX = AlignmentX.Center,
					AlignmentY = AlignmentY.Center
				};
			} else {
				_outerBorder!.Background = Background;
			}

			RebuildLayout();
		}

		private void WrapWithBorder() {
			if (Parent is not Panel panel) return;

			int index = panel.Children.IndexOf(this);
			panel.Children.RemoveAt(index);

			_outerBorder = new Border {
				Background = Background,
				BorderBrush = BorderBrush,
				BorderThickness = BorderThickness,
				CornerRadius = CornerRadius,
				Padding = Padding,
				Margin = Margin,
				Child = this
			};
			base.Margin = new Thickness(0);
			base.Background = Brushes.Transparent;

			panel.Children.Insert(index, _outerBorder);
		}

		#region === 外层边框依赖属性 ===

		public ImageSource BackgroundImage {
			get => (ImageSource)GetValue(BackgroundImageProperty);
			set => SetValue(BackgroundImageProperty, value);
		}
		public static readonly DependencyProperty BackgroundImageProperty =
			DependencyProperty.Register(nameof(BackgroundImage), typeof(ImageSource), typeof(Grid),
				new PropertyMetadata(null, OnBackgroundImageChanged));

		private static void OnBackgroundImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Grid g) {
				if (g._outerBorder == null)
					return;

				if (g.BackgroundImage != null) {
					g._outerBorder.Background = new ImageBrush(g.BackgroundImage) {
						Stretch = Stretch.UniformToFill,
						AlignmentX = AlignmentX.Center,
						AlignmentY = AlignmentY.Center
					};
				} else {
					g._outerBorder.Background = g.Background;
				}
			}
		}

		public new Brush Background {
			get => (Brush)GetValue(BackgroundProperty);
			set => SetValue(BackgroundProperty, value);
		}
		public static readonly new DependencyProperty BackgroundProperty =
			DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(Grid),
				new PropertyMetadata(null, OnBackgroundChanged));

		private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Grid g && g._outerBorder != null) {
				g._outerBorder.Background = g.Background;
			}
		}
		public Brush BorderBrush {
			get => (Brush)GetValue(BorderBrushProperty);
			set => SetValue(BorderBrushProperty, value);
		}
		public static readonly DependencyProperty BorderBrushProperty =
			DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(Grid),
				new PropertyMetadata(null, OnBorderPropertyChanged));

		public Thickness BorderThickness {
			get => (Thickness)GetValue(BorderThicknessProperty);
			set => SetValue(BorderThicknessProperty, value);
		}
		public static readonly DependencyProperty BorderThicknessProperty =
			DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(Grid),
				new PropertyMetadata(new Thickness(0), OnBorderPropertyChanged));

		public CornerRadius CornerRadius {
			get => (CornerRadius)GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
		}
		public static readonly DependencyProperty CornerRadiusProperty =
			DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Grid),
				new PropertyMetadata(new CornerRadius(0), OnBorderPropertyChanged));

		public Thickness Padding {
			get => (Thickness)GetValue(PaddingProperty);
			set => SetValue(PaddingProperty, value);
		}
		public static readonly new DependencyProperty PaddingProperty =
			DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(Grid),
				new PropertyMetadata(new Thickness(0), OnBorderPropertyChanged));

		private static void OnBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Grid g && g._outerBorder != null) {
				g._outerBorder.BorderBrush = g.BorderBrush;
				g._outerBorder.BorderThickness = g.BorderThickness;
				g._outerBorder.CornerRadius = g.CornerRadius;
				g._outerBorder.Padding = g.Padding;

				if (e.Property == MarginProperty) {
					g._outerBorder.Margin = g.Margin; // ✅ 同步外层边框的 Margin
					g.Margin = new Thickness(0);
				}
			}
		}

		#endregion

		#region === 基础依赖属性 ===

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

		public bool AutoPair {
			get => (bool)GetValue(AutoPairProperty);
			set => SetValue(AutoPairProperty, value);
		}
		public static readonly DependencyProperty AutoPairProperty =
			DependencyProperty.Register(nameof(AutoPair), typeof(bool), typeof(Grid),
				new PropertyMetadata(true, OnLayoutChanged));

		public GridFillMode FillMode {
			get => (GridFillMode)GetValue(FillModeProperty);
			set => SetValue(FillModeProperty, value);
		}
		public static readonly DependencyProperty FillModeProperty =
			DependencyProperty.Register(nameof(FillMode), typeof(GridFillMode), typeof(Grid),
				new PropertyMetadata(GridFillMode.Stretch, OnLayoutChanged));

		public bool DebugLayout {
			get => (bool)GetValue(DebugLayoutProperty);
			set => SetValue(DebugLayoutProperty, value);
		}
		public static readonly DependencyProperty DebugLayoutProperty =
			DependencyProperty.Register(nameof(DebugLayout), typeof(bool), typeof(Grid),
				new PropertyMetadata(false, OnLayoutChanged));

		#endregion

		#region === 子项样式属性 ===

		public string ItemMargin {
			get => (string)GetValue(ItemMarginProperty);
			set => SetValue(ItemMarginProperty, value);
		}
		public static readonly DependencyProperty ItemMarginProperty =
			DependencyProperty.Register(nameof(ItemMargin), typeof(string), typeof(Grid),
				new PropertyMetadata("0", OnLayoutChanged));

		public string ItemPadding {
			get => (string)GetValue(ItemPaddingProperty);
			set => SetValue(ItemPaddingProperty, value);
		}
		public static readonly DependencyProperty ItemPaddingProperty =
			DependencyProperty.Register(nameof(ItemPadding), typeof(string), typeof(Grid),
				new PropertyMetadata("0", OnLayoutChanged));

		public double ItemMinWidth {
			get => (double)GetValue(ItemMinWidthProperty);
			set => SetValue(ItemMinWidthProperty, value);
		}
		public static readonly DependencyProperty ItemMinWidthProperty =
			DependencyProperty.Register(nameof(ItemMinWidth), typeof(double), typeof(Grid),
				new PropertyMetadata(0.0, OnLayoutChanged));

		public double ItemMinHeight {
			get => (double)GetValue(ItemMinHeightProperty);
			set => SetValue(ItemMinHeightProperty, value);
		}
		public static readonly DependencyProperty ItemMinHeightProperty =
			DependencyProperty.Register(nameof(ItemMinHeight), typeof(double), typeof(Grid),
				new PropertyMetadata(0.0, OnLayoutChanged));

		public double ItemMaxWidth {
			get => (double)GetValue(ItemMaxWidthProperty);
			set => SetValue(ItemMaxWidthProperty, value);
		}
		public static readonly DependencyProperty ItemMaxWidthProperty =
			DependencyProperty.Register(nameof(ItemMaxWidth), typeof(double), typeof(Grid),
				new PropertyMetadata(double.PositiveInfinity, OnLayoutChanged));

		public double ItemMaxHeight {
			get => (double)GetValue(ItemMaxHeightProperty);
			set => SetValue(ItemMaxHeightProperty, value);
		}
		public static readonly DependencyProperty ItemMaxHeightProperty =
			DependencyProperty.Register(nameof(ItemMaxHeight), typeof(double), typeof(Grid),
				new PropertyMetadata(double.PositiveInfinity, OnLayoutChanged));

		public Brush ItemBackground {
			get => (Brush)GetValue(ItemBackgroundProperty);
			set => SetValue(ItemBackgroundProperty, value);
		}
		public static readonly DependencyProperty ItemBackgroundProperty =
			DependencyProperty.Register(nameof(ItemBackground), typeof(Brush), typeof(Grid),
				new PropertyMetadata(null, OnLayoutChanged));

		public Brush ItemBorderBrush {
			get => (Brush)GetValue(ItemBorderBrushProperty);
			set => SetValue(ItemBorderBrushProperty, value);
		}
		public static readonly DependencyProperty ItemBorderBrushProperty =
			DependencyProperty.Register(nameof(ItemBorderBrush), typeof(Brush), typeof(Grid),
				new PropertyMetadata(null, OnLayoutChanged));

		public string ItemBorderThickness {
			get => (string)GetValue(ItemBorderThicknessProperty);
			set => SetValue(ItemBorderThicknessProperty, value);
		}
		public static readonly DependencyProperty ItemBorderThicknessProperty =
			DependencyProperty.Register(nameof(ItemBorderThickness), typeof(string), typeof(Grid),
				new PropertyMetadata("0", OnLayoutChanged));

		public string ItemCornerRadius {
			get => (string)GetValue(ItemCornerRadiusProperty);
			set => SetValue(ItemCornerRadiusProperty, value);
		}
		public static readonly DependencyProperty ItemCornerRadiusProperty =
			DependencyProperty.Register(nameof(ItemCornerRadius), typeof(string), typeof(Grid),
				new PropertyMetadata("0", OnLayoutChanged));

		#endregion

		private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Grid g && g.IsLoaded)
				g.RebuildLayout();
		}

		#region === 核心布局逻辑 ===

		private void RebuildLayout() {
			if (!IsLoaded) return;

			// 🧩 解析间距
			var (spacingRow, spacingCol) = ParseSpacing(Spacing);
			double rowSpacing = RowSpacing == 0 ? spacingRow : RowSpacing;
			double colSpacing = ColumnSpacing == 0 ? spacingCol : ColumnSpacing;

			// 🧩 清空现有行列定义
			RowDefinitions.Clear();
			ColumnDefinitions.Clear();

			if (Uniform) {
				ApplyUniform();
				return;
			}

			bool hasRows = !string.IsNullOrWhiteSpace(Rows);
			bool hasCols = !string.IsNullOrWhiteSpace(Columns);
			int childCount = Children.Count;

			// 🧮 自动行列配对逻辑
			if (AutoPair) {
				if (hasCols && !hasRows) {
					int colCount = SplitParts(Columns).Length;
					int rowCount = (int)Math.Ceiling((double)childCount / colCount);
					CreateCols(Columns);
					CreateAutoRows(rowCount);
				} else if (hasRows && !hasCols) {
					int rowCount = SplitParts(Rows).Length;
					int colCount = (int)Math.Ceiling((double)childCount / rowCount);
					CreateRows(Rows);
					CreateAutoColumns(colCount);
				} else if (!hasRows && !hasCols) {
					CreateAutoRows(1);
					CreateAutoColumns(childCount);
				} else {
					CreateRows(Rows);
					CreateCols(Columns);
				}
			} else {
				if (hasRows) CreateRows(Rows);
				if (hasCols) CreateCols(Columns);
			}

			// 🧩 子项布局
			var list = Children.Cast<UIElement>().ToList();
			if (Reverse) list.Reverse();

			int totalRows = RowDefinitions.Count > 0 ? RowDefinitions.Count : 1;
			int totalCols = ColumnDefinitions.Count > 0 ? ColumnDefinitions.Count : 1;

			// 🧩 解析全局样式
			var itemMargin = ParseThickness(ItemMargin);
			var itemPadding = ParseThickness(ItemPadding);
			var itemBorderThickness = ParseThickness(ItemBorderThickness);
			var itemCornerRadius = ParseCornerRadius(ItemCornerRadius);

			int currentRow = 0, currentCol = 0;

			foreach (var child in list) {
				bool hasRow = child.ReadLocalValue(RowProperty) != DependencyProperty.UnsetValue;
				bool hasCol = child.ReadLocalValue(ColumnProperty) != DependencyProperty.UnsetValue;

				// 自动分配未指定位置的元素
				if (!hasRow && !hasCol) {
					SetRow(child, currentRow);
					SetColumn(child, currentCol);
				} else if (!hasRow && hasCol)
					SetRow(child, currentRow);
				else if (hasRow && !hasCol)
					SetColumn(child, currentCol);

				int spanR = Math.Max(1, GetRowSpan(child));
				int spanC = Math.Max(1, GetColumnSpan(child));

				currentCol += spanC;
				if (currentCol >= totalCols) {
					currentCol = 0;
					currentRow++;
				}

				// ⚙️ 应用默认样式（仅在未手动设置时）
				if (child is FrameworkElement fe) {
					// 对齐属性
					if (fe.ReadLocalValue(HorizontalAlignmentProperty) == DependencyProperty.UnsetValue)
						fe.HorizontalAlignment = ItemHorizontalAlignment;
					if (fe.ReadLocalValue(VerticalAlignmentProperty) == DependencyProperty.UnsetValue)
						fe.VerticalAlignment = ItemVerticalAlignment;

					// ✅ Margin（内部间隙版本，保留跨行/跨列支持）
					if (fe.ReadLocalValue(MarginProperty) == DependencyProperty.UnsetValue) {
						int row = GetRow(fe);
						int col = GetColumn(fe);
						int rowSpan = Math.Max(1, GetRowSpan(fe));
						int colSpan = Math.Max(1, GetColumnSpan(fe));

						double left = itemMargin.Left;
						double top = itemMargin.Top;
						double right = itemMargin.Right;
						double bottom = itemMargin.Bottom;

						// 🧩 仅在“内部”添加 Spacing，不影响外边缘
						// 左列以外 → 左加半个 spacing
						if (col > 0)
							left += colSpacing / 2;

						// 非最后一列（考虑跨列） → 右加半个 spacing
						if (col + colSpan < totalCols)
							right += colSpacing / 2;

						// 非首行 → 上加半个 spacing
						if (row > 0)
							top += rowSpacing / 2;

						// 非最后一行（考虑跨行） → 下加半个 spacing
						if (row + rowSpan < totalRows)
							bottom += rowSpacing / 2;

						fe.Margin = new Thickness(left, top, right, bottom);
					}

					// 尺寸限制
					if (fe.ReadLocalValue(MinWidthProperty) == DependencyProperty.UnsetValue)
						fe.MinWidth = ItemMinWidth;
					if (fe.ReadLocalValue(MinHeightProperty) == DependencyProperty.UnsetValue)
						fe.MinHeight = ItemMinHeight;
					if (fe.ReadLocalValue(MaxWidthProperty) == DependencyProperty.UnsetValue)
						fe.MaxWidth = ItemMaxWidth;
					if (fe.ReadLocalValue(MaxHeightProperty) == DependencyProperty.UnsetValue)
						fe.MaxHeight = ItemMaxHeight;

					// ✅ 针对 Control 类控件（Button、TextBox、Label 等）
					if (fe is Control ctrl) {
						if (ctrl.ReadLocalValue(Control.PaddingProperty) == DependencyProperty.UnsetValue)
							ctrl.Padding = itemPadding;

						if (ctrl.ReadLocalValue(Control.BackgroundProperty) == DependencyProperty.UnsetValue)
							ctrl.Background = ItemBackground;

						if (ctrl.ReadLocalValue(Control.BorderBrushProperty) == DependencyProperty.UnsetValue)
							ctrl.BorderBrush = ItemBorderBrush;

						if (ctrl.ReadLocalValue(Control.BorderThicknessProperty) == DependencyProperty.UnsetValue)
							ctrl.BorderThickness = itemBorderThickness;
					}
					// ✅ 针对 Border 控件
					else if (fe is System.Windows.Controls.Border border) {
						if (border.ReadLocalValue(System.Windows.Controls.Border.BackgroundProperty) == DependencyProperty.UnsetValue)
							border.Background = ItemBackground;

						if (border.ReadLocalValue(System.Windows.Controls.Border.BorderBrushProperty) == DependencyProperty.UnsetValue)
							border.BorderBrush = ItemBorderBrush;

						if (border.ReadLocalValue(System.Windows.Controls.Border.BorderThicknessProperty) == DependencyProperty.UnsetValue)
							border.BorderThickness = itemBorderThickness;

						if (border.ReadLocalValue(System.Windows.Controls.Border.CornerRadiusProperty) == DependencyProperty.UnsetValue)
							border.CornerRadius = itemCornerRadius;

						if (border.ReadLocalValue(System.Windows.Controls.Border.PaddingProperty) == DependencyProperty.UnsetValue)
							border.Padding = itemPadding;
					}
				}
			}

			// 🧾 调试输出
			// if (DebugLayout) {
			// 	Debug.WriteLine($"[SmartGrid] {RowDefinitions.Count} rows × {ColumnDefinitions.Count} cols ({Children.Count} children)");
			// 	foreach (UIElement child in Children) {
			// 		Debug.WriteLine($"  → {child.GetType().Name} | Row={GetRow(child)}, Col={GetColumn(child)}, RowSpan={GetRowSpan(child)}, ColSpan={GetColumnSpan(child)}");
			// 	}
			// }
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
			foreach (var part in SplitParts(text))
				RowDefinitions.Add(new RowDefinition { Height = ParseLength(part) });
		}

		private void CreateCols(string text) {
			foreach (var part in SplitParts(text))
				ColumnDefinitions.Add(new ColumnDefinition { Width = ParseLength(part) });
		}

		private static string[] SplitParts(string text) {
			if (string.IsNullOrWhiteSpace(text)) return Array.Empty<string>();
			var normalized = text.Replace('，', ' ').Replace(',', ' ').Trim();
			return normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static GridLength ParseLength(string val) {
			val = val?.Trim() ?? "*";
			if (val.Equals("auto", StringComparison.OrdinalIgnoreCase)) return GridLength.Auto;
			if (val.EndsWith("*") && double.TryParse(val.TrimEnd('*'), out double v))
				return new GridLength(v, GridUnitType.Star);
			if (double.TryParse(val, out double px))
				return new GridLength(px, GridUnitType.Pixel);
			return new GridLength(1, GridUnitType.Star);
		}

		private static (double row, double col) ParseSpacing(string? text) {
			if (string.IsNullOrWhiteSpace(text))
				return (0, 0);

			// 统一分隔符（支持空格 / 英文逗号 / 中文逗号）
			var normalized = text
				.Replace('，', ' ')
				.Replace(',', ' ')
				.Trim();

			// 分割
			var parts = normalized
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.ToArray();

			// 一个数：行列相同
			if (parts.Length == 1 && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
				return (v, v);

			// 两个数：第一个是行间距，第二个是列间距
			if (parts.Length >= 2) {
				double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var row);
				double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var col);
				return (row, col);
			}

			return (0, 0);
		}

		private static Thickness ParseThickness(string? text) {
			if (string.IsNullOrWhiteSpace(text)) return new Thickness(0);
			text = text.Replace("，", " ").Replace(",", " ");
			var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var vals = parts.Select(p => double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0).ToArray();

			return vals.Length switch {
				1 => new Thickness(vals[0]),
				2 => new Thickness(vals[0], vals[1], vals[0], vals[1]),
				4 => new Thickness(vals[0], vals[1], vals[2], vals[3]),
				_ => new Thickness(0)
			};
		}

		private static CornerRadius ParseCornerRadius(string? text) {
			if (string.IsNullOrWhiteSpace(text)) return new CornerRadius(0);
			text = text.Replace("，", " ").Replace(",", " ");
			var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var vals = parts.Select(p => double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0).ToArray();

			return vals.Length switch {
				1 => new CornerRadius(vals[0]),
				2 => new CornerRadius(vals[0], vals[1], vals[0], vals[1]),
				4 => new CornerRadius(vals[0], vals[1], vals[2], vals[3]),
				_ => new CornerRadius(0)
			};
		}

		#endregion
	}

	public enum GridFillMode { Auto, Stretch }
}