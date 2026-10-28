using System.Windows;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// One category occupying part of a <see cref="MeterBar"/>.
/// </summary>
/// <remarks>
/// Draws itself rather than using a template, so an <c>ItemContainerStyle</c> without <c>BasedOn</c> still renders.
/// </remarks>
public class MeterBarItem : FrameworkElement
{
    private Brush? _paletteBrush;

    #region Title

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(MeterBarItem),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    #endregion Title

    #region Value

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(MeterBarItem),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsParentArrange));

    /// <summary>
    /// Amount used to size this segment on the bar.
    /// </summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion Value

    #region ValueText

    public static readonly DependencyProperty ValueTextProperty =
        DependencyProperty.Register(
            nameof(ValueText),
            typeof(string),
            typeof(MeterBarItem),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Text shown beside <see cref="Title"/> in the legend.
    /// </summary>
    public string ValueText
    {
        get => (string)GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    #endregion ValueText

    #region Brush

    public static readonly DependencyProperty BrushProperty =
        DependencyProperty.Register(
            nameof(Brush),
            typeof(Brush),
            typeof(MeterBarItem),
            new PropertyMetadata(null, (d, _) => ((MeterBarItem)d).UpdateActualBrush()));

    /// <summary>
    /// Color of this segment. When not set, the owning <see cref="MeterBar"/> assigns a palette color.
    /// </summary>
    public Brush? Brush
    {
        get => (Brush?)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    #endregion Brush

    #region ActualBrush

    private static readonly DependencyPropertyKey ActualBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ActualBrush),
            typeof(Brush),
            typeof(MeterBarItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ActualBrushProperty = ActualBrushPropertyKey.DependencyProperty;

    /// <summary>
    /// <see cref="Brush"/> when set, otherwise the palette color assigned by the owning <see cref="MeterBar"/>.
    /// </summary>
    public Brush? ActualBrush => (Brush?)GetValue(ActualBrushProperty);

    #endregion ActualBrush

    internal void SetPaletteBrush(Brush brush)
    {
        _paletteBrush = brush;
        UpdateActualBrush();
    }

    private void UpdateActualBrush()
    {
        SetValue(ActualBrushPropertyKey, Brush ?? _paletteBrush);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        drawingContext.DrawRectangle(ActualBrush, null, new Rect(RenderSize));
    }
}
