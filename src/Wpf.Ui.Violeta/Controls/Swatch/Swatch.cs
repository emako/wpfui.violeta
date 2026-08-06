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
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Swatch), new PropertyMetadata(new CornerRadius(4)));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(Swatch), new PropertyMetadata(false));

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
}
