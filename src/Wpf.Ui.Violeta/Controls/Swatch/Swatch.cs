using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Represents a selectable color or image swatch.
/// </summary>
public class Swatch : Button
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(Swatch), new PropertyMetadata(null));

    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(nameof(Color), typeof(Brush), typeof(Swatch), new PropertyMetadata(null));

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(Swatch), new PropertyMetadata(null));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Swatch), new PropertyMetadata(new CornerRadius(6)));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(Swatch), new PropertyMetadata(false));

    public static readonly DependencyProperty SelectionStrokeProperty =
        DependencyProperty.Register(nameof(SelectionStroke), typeof(Brush), typeof(Swatch), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectionGapProperty =
        DependencyProperty.Register(nameof(SelectionGap), typeof(Brush), typeof(Swatch), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectionStrokeThicknessProperty =
        DependencyProperty.Register(nameof(SelectionStrokeThickness), typeof(Thickness), typeof(Swatch), new PropertyMetadata(new Thickness(2)));

    public static readonly DependencyProperty SelectionGapThicknessProperty =
        DependencyProperty.Register(nameof(SelectionGapThickness), typeof(Thickness), typeof(Swatch), new PropertyMetadata(new Thickness(2)));

    static Swatch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Swatch), new FrameworkPropertyMetadata(typeof(Swatch)));
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Brush? Color
    {
        get => (Brush?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Brush of the outer ring drawn around the swatch when it is selected.
    /// </summary>
    public Brush? SelectionStroke
    {
        get => (Brush?)GetValue(SelectionStrokeProperty);
        set => SetValue(SelectionStrokeProperty, value);
    }

    /// <summary>
    /// Brush filling the gap between the selection ring and the swatch color.
    /// </summary>
    public Brush? SelectionGap
    {
        get => (Brush?)GetValue(SelectionGapProperty);
        set => SetValue(SelectionGapProperty, value);
    }

    /// <summary>
    /// Thickness of the accent stroke drawn inside the swatch when it is selected.
    /// </summary>
    public Thickness SelectionStrokeThickness
    {
        get => (Thickness)GetValue(SelectionStrokeThicknessProperty);
        set => SetValue(SelectionStrokeThicknessProperty, value);
    }

    /// <summary>
    /// Thickness of the gap drawn between the selection stroke and the swatch color.
    /// </summary>
    public Thickness SelectionGapThickness
    {
        get => (Thickness)GetValue(SelectionGapThicknessProperty);
        set => SetValue(SelectionGapThicknessProperty, value);
    }
}
