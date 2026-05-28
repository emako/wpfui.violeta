using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Wpf.Ui.Controls;

[ContentProperty(nameof(Children))]
public class Grid : System.Windows.Controls.Grid
{
    private readonly List<GridLength> _logicalColumns = [];
    private readonly List<GridLength> _logicalRows = [];

    // Private attached DPs that store the user's logical Grid.Column/Row before
    // we remap them to accommodate spacer columns/rows.
    private static readonly DependencyProperty LogicalColumnProperty =
        DependencyProperty.RegisterAttached("LogicalColumn", typeof(int), typeof(Grid), new PropertyMetadata(int.MinValue));

    private static readonly DependencyProperty LogicalRowProperty =
        DependencyProperty.RegisterAttached("LogicalRow", typeof(int), typeof(Grid), new PropertyMetadata(int.MinValue));

    private static readonly DependencyProperty LogicalColumnSpanProperty =
        DependencyProperty.RegisterAttached("LogicalColumnSpan", typeof(int), typeof(Grid), new PropertyMetadata(int.MinValue));

    private static readonly DependencyProperty LogicalRowSpanProperty =
        DependencyProperty.RegisterAttached("LogicalRowSpan", typeof(int), typeof(Grid), new PropertyMetadata(int.MinValue));

    public static readonly DependencyProperty ColumnDefinitionsProperty =
          DependencyProperty.Register(nameof(ColumnDefinitions), typeof(ColumnDefinitionCollection), typeof(Grid), new PropertyMetadata(null, OnColumnDefinitionsChanged));

    [TypeConverter(typeof(ColumnDefinitionsConverter))]
    public new ColumnDefinitionCollection ColumnDefinitions
    {
        get => base.ColumnDefinitions;
        set => SetValue(ColumnDefinitionsProperty, value);
    }

    private static void OnColumnDefinitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Grid grid && e.NewValue is ColumnDefinitionCollection columnDefinitions)
        {
            grid._logicalColumns.Clear();
            foreach (ColumnDefinition col in columnDefinitions)
                grid._logicalColumns.Add(col.Width);
            grid.RebuildColumnDefinitions();
        }
    }

    public static readonly DependencyProperty RowDefinitionsProperty =
          DependencyProperty.Register(nameof(RowDefinitions), typeof(RowDefinitionCollection), typeof(Grid), new PropertyMetadata(null, OnRowDefinitionsChanged));

    [TypeConverter(typeof(RowDefinitionsConverter))]
    public new RowDefinitionCollection RowDefinitions
    {
        get => base.RowDefinitions;
        set => SetValue(RowDefinitionsProperty, value);
    }

    private static void OnRowDefinitionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Grid grid && e.NewValue is RowDefinitionCollection rowDefinitions)
        {
            grid._logicalRows.Clear();
            foreach (RowDefinition row in rowDefinitions)
                grid._logicalRows.Add(row.Height);
            grid.RebuildRowDefinitions();
        }
    }

    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(Grid),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnHorizontalSpacingChanged));

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    private static void OnHorizontalSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Grid grid)
            grid.RebuildColumnDefinitions();
    }

    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(Grid),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnVerticalSpacingChanged));

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    private static void OnVerticalSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Grid grid)
            grid.RebuildRowDefinitions();
    }

    protected override Size MeasureOverride(Size constraint)
    {
        bool hasH = HorizontalSpacing > 0 && _logicalColumns.Count > 0;
        bool hasV = VerticalSpacing > 0 && _logicalRows.Count > 0;

        foreach (UIElement child in Children)
        {
            if (hasH || hasV)
            {
                // Capture user-set column/row on first encounter (before any remapping).
                if ((int)child.GetValue(LogicalColumnProperty) == int.MinValue)
                {
                    child.SetValue(LogicalColumnProperty, GetColumn(child));
                    child.SetValue(LogicalRowProperty, GetRow(child));
                    child.SetValue(LogicalColumnSpanProperty, GetColumnSpan(child));
                    child.SetValue(LogicalRowSpanProperty, GetRowSpan(child));
                }

                int logCol = (int)child.GetValue(LogicalColumnProperty);
                int logRow = (int)child.GetValue(LogicalRowProperty);
                int logColSpan = (int)child.GetValue(LogicalColumnSpanProperty);
                int logRowSpan = (int)child.GetValue(LogicalRowSpanProperty);

                // Map logical → actual (spacer-injected) index.
                // Logical column c → actual column c*2; span s → actual span s*2-1.
                SetColumn(child, hasH ? logCol * 2 : logCol);
                SetRow(child, hasV ? logRow * 2 : logRow);
                SetColumnSpan(child, hasH ? Math.Max(1, logColSpan * 2 - 1) : logColSpan);
                SetRowSpan(child, hasV ? Math.Max(1, logRowSpan * 2 - 1) : logRowSpan);
            }
            else
            {
                // Spacing was removed — restore the original logical values.
                int logCol = (int)child.GetValue(LogicalColumnProperty);
                if (logCol != int.MinValue)
                {
                    SetColumn(child, logCol);
                    SetRow(child, (int)child.GetValue(LogicalRowProperty));
                    SetColumnSpan(child, (int)child.GetValue(LogicalColumnSpanProperty));
                    SetRowSpan(child, (int)child.GetValue(LogicalRowSpanProperty));
                }
            }
        }

        return base.MeasureOverride(constraint);
    }

    private void RebuildColumnDefinitions()
    {
        base.ColumnDefinitions.Clear();
        bool hasSpacing = HorizontalSpacing > 0;
        bool first = true;
        foreach (GridLength width in _logicalColumns)
        {
            if (!first && hasSpacing)
                base.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(HorizontalSpacing) });
            base.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
            first = false;
        }
    }

    private void RebuildRowDefinitions()
    {
        base.RowDefinitions.Clear();
        bool hasSpacing = VerticalSpacing > 0;
        bool first = true;
        foreach (GridLength height in _logicalRows)
        {
            if (!first && hasSpacing)
                base.RowDefinitions.Add(new RowDefinition { Height = new GridLength(VerticalSpacing) });
            base.RowDefinitions.Add(new RowDefinition { Height = height });
            first = false;
        }
    }
}

internal sealed class ColumnDefinitionsConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(ColumnDefinitionCollection) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        bool hotReload = false;

        try
        {
            if (value is string strValue)
            {
                System.Windows.Controls.Grid _grid = null!;

                if (context is IProvideValueTarget target)
                {
                    _grid = (System.Windows.Controls.Grid)target.TargetObject;
                }
                else
                {
                    // Support for Hot Reload (WpfVisualTreeService.LiveMarkup.TapTypeDescriptorContext).
                    _grid = new System.Windows.Controls.Grid();
                    hotReload = true;
                }

                ColumnDefinitionCollection columnDefinitions = (ColumnDefinitionCollection)Activator.CreateInstance(
                    typeof(ColumnDefinitionCollection),
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                    null!,
                    [_grid],
                    null!)!;
                GridLengthConverter converter = new();
                string[] definitions = strValue.Split(',');

                foreach (string definition in definitions)
                {
                    GridLength gridLength = (GridLength)converter.ConvertFromString(definition.Trim())!;
                    columnDefinitions.Add(new ColumnDefinition { Width = gridLength });
                }

                return columnDefinitions;
            }
            else if (value is ColumnDefinitionCollection columnDefinitions)
            {
                return columnDefinitions;
            }
        }
        catch (Exception e)
        {
            if (!hotReload)
            {
                throw new InvalidOperationException($"Invalid ColumnDefinitions value \"{value}\".{Environment.NewLine}" + e);
            }
            return DependencyProperty.UnsetValue;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

internal sealed class RowDefinitionsConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(RowDefinitionCollection) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        bool hotReload = false;

        try
        {
            if (value is string strValue)
            {
                System.Windows.Controls.Grid _grid = null!;

                if (context is IProvideValueTarget target)
                {
                    _grid = (System.Windows.Controls.Grid)target.TargetObject;
                }
                else
                {
                    // Support for Hot Reload (WpfVisualTreeService.LiveMarkup.TapTypeDescriptorContext).
                    _grid = new System.Windows.Controls.Grid();
                    hotReload = true;
                }

                RowDefinitionCollection rowDefinitions = (RowDefinitionCollection)Activator.CreateInstance(
                    typeof(RowDefinitionCollection),
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                    null!,
                    [_grid],
                    null!)!;
                GridLengthConverter converter = new();
                string[] definitions = strValue.Split(',');

                foreach (string definition in definitions)
                {
                    GridLength gridLength = (GridLength)converter.ConvertFromString(definition.Trim())!;
                    rowDefinitions.Add(new RowDefinition { Height = gridLength });
                }

                return rowDefinitions;
            }
            else if (value is RowDefinitionCollection rowDefinitions)
            {
                return rowDefinitions;
            }
        }
        catch (Exception e)
        {
            if (!hotReload)
            {
                throw new InvalidOperationException($"Invalid RowDefinitions value \"{value}\".{Environment.NewLine}" + e);
            }
            return DependencyProperty.UnsetValue;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
