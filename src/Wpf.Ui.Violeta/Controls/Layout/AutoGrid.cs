using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

/// <summary>
/// Defines a flexible grid area that consists of columns and rows.
/// Depending on the orientation, either the rows or the columns are auto-generated,
/// and the children's position is set according to their index.
///
/// Partially based on work at http://rachel53461.wordpress.com/2011/09/17/wpf-grids-rowcolumn-count-properties/
/// </summary>
public class AutoGrid : Grid
{
    public static readonly DependencyProperty ChildHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(ChildHorizontalAlignment), typeof(HorizontalAlignment?), typeof(AutoGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnChildHorizontalAlignmentChanged));

    public static readonly DependencyProperty ChildMarginProperty =
        DependencyProperty.Register(nameof(ChildMargin), typeof(Thickness?), typeof(AutoGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnChildMarginChanged));

    public static readonly DependencyProperty ChildVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(ChildVerticalAlignment), propertyType: typeof(VerticalAlignment?), ownerType: typeof(AutoGrid), typeMetadata: new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnChildVerticalAlignmentChanged));

    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.RegisterAttached(nameof(ColumnCount), typeof(int), typeof(AutoGrid), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ColumnCountChanged)));

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.RegisterAttached(nameof(Columns), typeof(string), typeof(AutoGrid), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ColumnsChanged)));

    public static readonly DependencyProperty ColumnWidthProperty =
        DependencyProperty.RegisterAttached(nameof(ColumnWidth), typeof(GridLength), typeof(AutoGrid), new FrameworkPropertyMetadata(GridLength.Auto, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FixedColumnWidthChanged)));

    public static readonly DependencyProperty IsAutoIndexingProperty =
        DependencyProperty.Register(nameof(IsAutoIndexing), typeof(bool), typeof(AutoGrid), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(AutoGrid), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty RowCountProperty =
        DependencyProperty.RegisterAttached(nameof(RowCount), typeof(int), typeof(AutoGrid), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(RowCountChanged)));

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.RegisterAttached(nameof(RowHeight), typeof(GridLength), typeof(AutoGrid), new FrameworkPropertyMetadata(GridLength.Auto, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FixedRowHeightChanged)));

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.RegisterAttached(nameof(Rows), typeof(string), typeof(AutoGrid), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(RowsChanged)));

    // Base Grid stores sticky logical Column/Row/Span on private attached DPs before remapping
    // for HorizontalSpacing / VerticalSpacing. AutoGrid reassigns indices every measure, so those
    // caches must be restored + cleared without modifying Grid.cs.
    private static readonly DependencyProperty? LogicalColumnProperty =
        typeof(Grid).GetField("LogicalColumnProperty", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as DependencyProperty;

    private static readonly DependencyProperty? LogicalRowProperty =
        typeof(Grid).GetField("LogicalRowProperty", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as DependencyProperty;

    private static readonly DependencyProperty? LogicalColumnSpanProperty =
        typeof(Grid).GetField("LogicalColumnSpanProperty", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as DependencyProperty;

    private static readonly DependencyProperty? LogicalRowSpanProperty =
        typeof(Grid).GetField("LogicalRowSpanProperty", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as DependencyProperty;

    /// <summary>
    /// Gets or sets the child horizontal alignment.
    /// </summary>
    /// <value>The child horizontal alignment.</value>
    [Category("Layout"), Description("Presets the horizontal alignment of all child controls")]
    public HorizontalAlignment? ChildHorizontalAlignment
    {
        get => (HorizontalAlignment?)GetValue(ChildHorizontalAlignmentProperty);
        set => SetValue(ChildHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the child margin.
    /// </summary>
    /// <value>The child margin.</value>
    [Category("Layout"), Description("Presets the margin of all child controls")]
    public Thickness? ChildMargin
    {
        get => (Thickness?)GetValue(ChildMarginProperty);
        set => SetValue(ChildMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets the child vertical alignment.
    /// </summary>
    /// <value>The child vertical alignment.</value>
    [Category("Layout"), Description("Presets the vertical alignment of all child controls")]
    public VerticalAlignment? ChildVerticalAlignment
    {
        get => (VerticalAlignment?)GetValue(ChildVerticalAlignmentProperty);
        set => SetValue(ChildVerticalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the column count
    /// </summary>
    [Category("Layout"), Description("Defines a set number of columns")]
    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the columns
    /// </summary>
    [Category("Layout"), Description("Defines all columns using comma separated grid length notation")]
    public string Columns
    {
        get => (string)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the fixed column width
    /// </summary>
    [Category("Layout"), Description("Presets the width of all columns set using the ColumnCount property")]
    public GridLength ColumnWidth
    {
        get => (GridLength)GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the children are automatically indexed.
    /// <remarks>
    /// The default is <c>true</c>.
    /// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
    /// </remarks>
    /// </summary>
    [Category("Layout"), Description("Set to false to disable the auto layout functionality")]
    public bool IsAutoIndexing
    {
        get => (bool)GetValue(IsAutoIndexingProperty);
        set => SetValue(IsAutoIndexingProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation.
    /// <remarks>The default is Vertical.</remarks>
    /// </summary>
    /// <value>The orientation.</value>
    [Category("Layout"), Description("Defines the directionality of the autolayout. Use vertical for a column first layout, horizontal for a row first layout.")]
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of rows
    /// </summary>
    [Category("Layout"), Description("Defines a set number of rows")]
    public int RowCount
    {
        get => (int)GetValue(RowCountProperty);
        set => SetValue(RowCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the fixed row height
    /// </summary>
    [Category("Layout"), Description("Presets the height of all rows set using the RowCount property")]
    public GridLength RowHeight
    {
        get => (GridLength)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the rows
    /// </summary>
    [Category("Layout"), Description("Defines all rows using comma separated grid length notation")]
    public string Rows
    {
        get => (string)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>
    /// Handles the column count changed event
    /// </summary>
    public static void ColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((int)e.NewValue < 0)
            return;

        var grid = d as AutoGrid;

        // look for an existing column definition for the height
        var width = GridLength.Auto;
        if (grid.GetLogicalColumnCount() > 0)
            width = grid.GetLogicalColumnWidth(0);

        var widths = new GridLength[(int)e.NewValue];
        for (int i = 0; i < widths.Length; i++)
            widths[i] = width;
        grid.ApplyLogicalColumnDefinitions(widths);
    }

    /// <summary>
    /// Handle the columns changed event
    /// </summary>
    public static void ColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty((string)e.NewValue))
            return;

        var grid = d as AutoGrid;
        grid.ApplyLogicalColumnDefinitions(Parse((string)e.NewValue));
    }

    /// <summary>
    /// Handle the fixed column width changed event
    /// </summary>
    public static void FixedColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = d as AutoGrid;
        int count = Math.Max(1, grid.GetLogicalColumnCount());
        var widths = new GridLength[count];
        for (int i = 0; i < count; i++)
            widths[i] = (GridLength)e.NewValue;
        grid.ApplyLogicalColumnDefinitions(widths);
    }

    /// <summary>
    /// Handle the fixed row height changed event
    /// </summary>
    public static void FixedRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = d as AutoGrid;
        int count = Math.Max(1, grid.GetLogicalRowCount());
        var heights = new GridLength[count];
        for (int i = 0; i < count; i++)
            heights[i] = (GridLength)e.NewValue;
        grid.ApplyLogicalRowDefinitions(heights);
    }

    /// <summary>
    /// Parse an array of grid lengths from comma delim text
    /// </summary>
    public static GridLength[] Parse(string text)
    {
        var tokens = text.Split(',');
        var definitions = new GridLength[tokens.Length];
        for (var i = 0; i < tokens.Length; i++)
        {
            var str = tokens[i];
            double value;

            // ratio
            if (str.Contains("*"))
            {
                if (!double.TryParse(str.Replace("*", string.Empty), out value))
                    value = 1.0;

                definitions[i] = new GridLength(value, GridUnitType.Star);
                continue;
            }

            // pixels
            if (double.TryParse(str, out value))
            {
                definitions[i] = new GridLength(value);
                continue;
            }

            // auto
            definitions[i] = GridLength.Auto;
        }
        return definitions;
    }

    /// <summary>
    /// Handles the row count changed event
    /// </summary>
    public static void RowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((int)e.NewValue < 0)
            return;

        var grid = d as AutoGrid;

        // look for an existing row to get the height
        var height = GridLength.Auto;
        if (grid.GetLogicalRowCount() > 0)
            height = grid.GetLogicalRowHeight(0);

        var heights = new GridLength[(int)e.NewValue];
        for (int i = 0; i < heights.Length; i++)
            heights[i] = height;
        grid.ApplyLogicalRowDefinitions(heights);
    }

    /// <summary>
    /// Handle the rows changed event
    /// </summary>
    public static void RowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty((string)e.NewValue))
            return;

        var grid = d as AutoGrid;
        grid.ApplyLogicalRowDefinitions(Parse((string)e.NewValue));
    }

    /// <summary>
    /// Measures the children of a <see cref="T:System.Windows.Controls.Grid"/> in anticipation of arranging them during the <see cref="M:ArrangeOverride"/> pass.
    /// </summary>
    /// <param name="constraint">Indicates an upper limit size that should not be exceeded.</param>
    /// <returns>
    /// <see cref="Size"/> that represents the required size to arrange child content.
    /// </returns>
    protected override Size MeasureOverride(Size constraint)
    {
        // If ColumnDefinitions were mutated via Clear/Add before this type started
        // routing through ApplyLogical*, re-push through the Grid DP so spacers exist.
        EnsureSpacingDefinitionsSynced();
        // Restore logical Column/Row/Span from base Grid's sticky cache (if any) so
        // PerformLayout reads logical values, then clear the cache so base re-captures
        // after we reassign indices for this pass.
        RestoreAndClearBaseLogicalPlacement();
        this.PerformLayout();
        return base.MeasureOverride(constraint);
    }

    /// <summary>
    /// Called when [child horizontal alignment changed].
    /// </summary>
    private static void OnChildHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = d as AutoGrid;
        foreach (UIElement child in grid.Children)
        {
            if (grid.ChildHorizontalAlignment.HasValue)
                child.SetValue(FrameworkElement.HorizontalAlignmentProperty, grid.ChildHorizontalAlignment);
            else
                child.SetValue(FrameworkElement.HorizontalAlignmentProperty, DependencyProperty.UnsetValue);
        }
    }

    /// <summary>
    /// Called when [child layout changed].
    /// </summary>
    private static void OnChildMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = d as AutoGrid;
        foreach (UIElement child in grid.Children)
        {
            if (grid.ChildMargin.HasValue)
                child.SetValue(FrameworkElement.MarginProperty, grid.ChildMargin);
            else
                child.SetValue(FrameworkElement.MarginProperty, DependencyProperty.UnsetValue);
        }
    }

    /// <summary>
    /// Called when [child vertical alignment changed].
    /// </summary>
    private static void OnChildVerticalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = d as AutoGrid;
        foreach (UIElement child in grid.Children)
        {
            if (grid.ChildVerticalAlignment.HasValue)
                child.SetValue(FrameworkElement.VerticalAlignmentProperty, grid.ChildVerticalAlignment);
            else
                child.SetValue(FrameworkElement.VerticalAlignmentProperty, DependencyProperty.UnsetValue);
        }
    }

    ///// <summary>
    ///// Handled the redraw properties changed event
    ///// </summary>
    //private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{ }

    /// <summary>
    /// Apply child margins and layout effects such as alignment
    /// </summary>
    private void ApplyChildLayout(UIElement child)
    {
        if (ChildMargin != null)
        {
            child.SetIfDefault(FrameworkElement.MarginProperty, ChildMargin.Value);
        }
        if (ChildHorizontalAlignment != null)
        {
            child.SetIfDefault(FrameworkElement.HorizontalAlignmentProperty, ChildHorizontalAlignment.Value);
        }
        if (ChildVerticalAlignment != null)
        {
            child.SetIfDefault(FrameworkElement.VerticalAlignmentProperty, ChildVerticalAlignment.Value);
        }
    }

    /// <summary>
    /// Pushes logical column widths through <see cref="Grid.ColumnDefinitions"/> so the base
    /// <see cref="Grid"/> populates its logical cache and injects <see cref="Grid.HorizontalSpacing"/> spacers.
    /// </summary>
    private void ApplyLogicalColumnDefinitions(IReadOnlyList<GridLength> widths)
    {
        var temp = new System.Windows.Controls.Grid();
        foreach (GridLength width in widths)
            temp.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
        ColumnDefinitions = temp.ColumnDefinitions;
    }

    /// <summary>
    /// Pushes logical row heights through <see cref="Grid.RowDefinitions"/> so the base
    /// <see cref="Grid"/> populates its logical cache and injects <see cref="Grid.VerticalSpacing"/> spacers.
    /// </summary>
    private void ApplyLogicalRowDefinitions(IReadOnlyList<GridLength> heights)
    {
        var temp = new System.Windows.Controls.Grid();
        foreach (GridLength height in heights)
            temp.RowDefinitions.Add(new RowDefinition { Height = height });
        RowDefinitions = temp.RowDefinitions;
    }

    /// <summary>
    /// Clamp a value to its maximum.
    /// </summary>
    private int Clamp(int value, int max)
    {
        return (value > max) ? max : value;
    }

    /// <summary>
    /// Re-sync definitions when spacing is on but spacer columns/rows are missing
    /// (e.g. definitions were Clear/Add'd without going through <see cref="ApplyLogicalColumnDefinitions"/>).
    /// </summary>
    private void EnsureSpacingDefinitionsSynced()
    {
        if (HorizontalSpacing > 0 && ColumnDefinitions.Count > 0 && !HasInjectedColumnSpacers())
        {
            var widths = new List<GridLength>(ColumnDefinitions.Count);
            foreach (ColumnDefinition col in ColumnDefinitions)
                widths.Add(col.Width);
            ApplyLogicalColumnDefinitions(widths);
        }

        if (VerticalSpacing > 0 && RowDefinitions.Count > 0 && !HasInjectedRowSpacers())
        {
            var heights = new List<GridLength>(RowDefinitions.Count);
            foreach (RowDefinition row in RowDefinitions)
                heights.Add(row.Height);
            ApplyLogicalRowDefinitions(heights);
        }
    }

    private GridLength GetLogicalColumnWidth(int logicalIndex)
    {
        int actualIndex = HorizontalSpacing > 0 && HasInjectedColumnSpacers()
            ? logicalIndex * 2
            : logicalIndex;
        return ColumnDefinitions[actualIndex].Width;
    }

    private int GetLogicalColumnCount()
    {
        int count = ColumnDefinitions.Count;
        if (count == 0)
            return 0;
        if (HorizontalSpacing > 0 && HasInjectedColumnSpacers())
            return (count + 1) / 2;
        return count;
    }

    private GridLength GetLogicalRowHeight(int logicalIndex)
    {
        int actualIndex = VerticalSpacing > 0 && HasInjectedRowSpacers()
            ? logicalIndex * 2
            : logicalIndex;
        return RowDefinitions[actualIndex].Height;
    }

    private int GetLogicalRowCount()
    {
        int count = RowDefinitions.Count;
        if (count == 0)
            return 0;
        if (VerticalSpacing > 0 && HasInjectedRowSpacers())
            return (count + 1) / 2;
        return count;
    }

    private bool HasInjectedColumnSpacers()
    {
        int n = ColumnDefinitions.Count;
        if (HorizontalSpacing <= 0 || n < 3 || n % 2 == 0)
            return false;

        var spacer = new GridLength(HorizontalSpacing);
        for (int i = 1; i < n; i += 2)
        {
            if (ColumnDefinitions[i].Width != spacer)
                return false;
        }
        return true;
    }

    private bool HasInjectedRowSpacers()
    {
        int n = RowDefinitions.Count;
        if (VerticalSpacing <= 0 || n < 3 || n % 2 == 0)
            return false;

        var spacer = new GridLength(VerticalSpacing);
        for (int i = 1; i < n; i += 2)
        {
            if (RowDefinitions[i].Height != spacer)
                return false;
        }
        return true;
    }

    /// <summary>
    /// If base <see cref="Grid"/> already remapped children for spacing, restore the cached
    /// logical placement onto the public attached properties and clear the sticky cache so
    /// the next base measure pass re-captures whatever <see cref="PerformLayout"/> assigns.
    /// </summary>
    private void RestoreAndClearBaseLogicalPlacement()
    {
        if (LogicalColumnProperty == null || LogicalRowProperty == null
            || LogicalColumnSpanProperty == null || LogicalRowSpanProperty == null)
            return;

        foreach (UIElement child in Children)
        {
            int logCol = (int)child.GetValue(LogicalColumnProperty);
            if (logCol == int.MinValue)
                continue;

            SetColumn(child, logCol);
            SetRow(child, (int)child.GetValue(LogicalRowProperty));
            SetColumnSpan(child, (int)child.GetValue(LogicalColumnSpanProperty));
            SetRowSpan(child, (int)child.GetValue(LogicalRowSpanProperty));

            child.SetValue(LogicalColumnProperty, int.MinValue);
            child.SetValue(LogicalRowProperty, int.MinValue);
            child.SetValue(LogicalColumnSpanProperty, int.MinValue);
            child.SetValue(LogicalRowSpanProperty, int.MinValue);
        }
    }

    /// <summary>
    /// Perform the grid layout of row and column indexes
    /// </summary>
    private void PerformLayout()
    {
        var fillRowFirst = Orientation == Orientation.Horizontal;
        // Use logical counts so injected HorizontalSpacing / VerticalSpacing spacers
        // are not treated as layout cells.
        var rowCount = GetLogicalRowCount();
        var colCount = GetLogicalColumnCount();

        if (rowCount == 0 || colCount == 0)
            return;

        var position = 0;
        var skip = new bool[rowCount, colCount];
        foreach (UIElement child in Children)
        {
            var childIsCollapsed = child.Visibility == Visibility.Collapsed;
            if (IsAutoIndexing && !childIsCollapsed)
            {
                if (fillRowFirst)
                {
                    var row = Clamp(position / colCount, rowCount - 1);
                    var col = Clamp(position % colCount, colCount - 1);
                    if (skip[row, col])
                    {
                        position++;
                        row = (position / colCount);
                        col = (position % colCount);
                    }

                    SetRow(child, row);
                    SetColumn(child, col);
                    position += GetColumnSpan(child);

                    var offset = GetRowSpan(child) - 1;
                    while (offset > 0)
                    {
                        skip[row + offset--, col] = true;
                    }
                }
                else
                {
                    var row = Clamp(position % rowCount, rowCount - 1);
                    var col = Clamp(position / rowCount, colCount - 1);
                    if (skip[row, col])
                    {
                        position++;
                        row = position % rowCount;
                        col = position / rowCount;
                    }

                    SetRow(child, row);
                    SetColumn(child, col);
                    position += GetRowSpan(child);

                    var offset = GetColumnSpan(child) - 1;
                    while (offset > 0)
                    {
                        skip[row, col + offset--] = true;
                    }
                }
            }

            ApplyChildLayout(child);
        }
    }
}

file static class DependencyExtensions
{
    /// <summary>
    /// Sets the value of the <paramref name="property"/> only if it hasn't been explicitly set.
    /// </summary>
    public static bool SetIfDefault<T>(this DependencyObject o, DependencyProperty property, T value)
    {
        if (DependencyPropertyHelper.GetValueSource(o, property).BaseValueSource == BaseValueSource.Default)
        {
            o.SetValue(property, value);

            return true;
        }

        return false;
    }
}
