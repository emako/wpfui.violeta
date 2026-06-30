using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A layout control that switches its displayed content based on the current aspect ratio of its
/// bounds, mirroring the behaviour of Ursa.Avalonia's <c>AspectRatioLayout</c> control.
/// </summary>
/// <remarks>
/// Add <see cref="AspectRatioLayoutItem"/> children (via <see cref="Items"/>) and set either
/// <see cref="AspectRatioLayoutItem.AcceptAspectRatioMode"/> or
/// <see cref="AspectRatioLayoutItem.StartAspectRatioValue"/>/<see cref="AspectRatioLayoutItem.EndAspectRatioValue"/>
/// on each item.  The first matching item's <see cref="ContentControl.Content"/> is shown.
/// </remarks>
[ContentProperty(nameof(Items))]
public class AspectRatioLayout : ContentControl
{
    // Rolling window of 3 direction flags used to debounce rapid oscillation at the boundary.
    private readonly Queue<bool> _history = new();

    static AspectRatioLayout()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AspectRatioLayout),
            new FrameworkPropertyMetadata(typeof(AspectRatioLayout)));

        IsTabStopProperty.OverrideMetadata(
            typeof(AspectRatioLayout),
            new FrameworkPropertyMetadata(false));

        FocusableProperty.OverrideMetadata(
            typeof(AspectRatioLayout),
            new FrameworkPropertyMetadata(false));
    }

    public AspectRatioLayout()
    {
        Items = [];
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        UpdateContent(forceUpdate: true);
    }

    #region Items

    /// <summary>The collection of content candidates, one per aspect-ratio range/mode.</summary>
    public List<AspectRatioLayoutItem> Items { get; set; }

    #endregion Items

    #region AspectRatioTolerance

    public static readonly DependencyProperty AspectRatioToleranceProperty =
        DependencyProperty.Register(
            nameof(AspectRatioTolerance),
            typeof(double),
            typeof(AspectRatioLayout),
            new PropertyMetadata(0.2, OnToleranceChanged));

    /// <summary>
    /// The ±tolerance around 1.0 that is still considered "square" (default 0.2).
    /// </summary>
    public double AspectRatioTolerance
    {
        get => (double)GetValue(AspectRatioToleranceProperty);
        set => SetValue(AspectRatioToleranceProperty, value);
    }

    private static void OnToleranceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((AspectRatioLayout)d).UpdateContent(forceUpdate: true);

    #endregion AspectRatioTolerance

    #region CurrentAspectRatioMode (read-only)

    private static readonly DependencyPropertyKey CurrentAspectRatioModePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(CurrentAspectRatioMode),
            typeof(AspectRatioMode),
            typeof(AspectRatioLayout),
            new PropertyMetadata(AspectRatioMode.None));

    public static readonly DependencyProperty CurrentAspectRatioModeProperty =
        CurrentAspectRatioModePropertyKey.DependencyProperty;

    public AspectRatioMode CurrentAspectRatioMode =>
        (AspectRatioMode)GetValue(CurrentAspectRatioModeProperty);

    #endregion CurrentAspectRatioMode (read-only)

    #region AspectRatioValue (read-only)

    private static readonly DependencyPropertyKey AspectRatioValuePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(AspectRatioValue),
            typeof(double),
            typeof(AspectRatioLayout),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty AspectRatioValueProperty =
        AspectRatioValuePropertyKey.DependencyProperty;

    /// <summary>The computed width/height ratio (rounded to 3 decimal places).</summary>
    public double AspectRatioValue =>
        (double)GetValue(AspectRatioValueProperty);

    #endregion AspectRatioValue (read-only)

    // -------------------------------------------------------------------------

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        var oldRatio = GetRatio(sizeInfo.PreviousSize.Width, sizeInfo.PreviousSize.Height);
        var newRatio = GetRatio(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);

        UpdateHistory(oldRatio <= newRatio);

        if (!IsHistoryConsistent()) return;

        UpdateContent(forceUpdate: false);
    }

    private void UpdateContent(bool forceUpdate)
    {
        if (!IsLoaded && !forceUpdate) return;

        var ratio = GetRatio(ActualWidth, ActualHeight);
        SetValue(AspectRatioValuePropertyKey, ratio);

        var mode = GetMode(ratio);
        SetValue(CurrentAspectRatioModePropertyKey, mode);

        var candidate =
            Items.FirstOrDefault(x =>
                x.IsUseAspectRatioRange
                && x.StartAspectRatioValue <= ratio
                && ratio <= x.EndAspectRatioValue)
            ?? Items.FirstOrDefault(x => x.AcceptAspectRatioMode == mode)
            ?? Items.FirstOrDefault();

        Content = candidate;
    }

    private void UpdateHistory(bool isIncreasing)
    {
        _history.Enqueue(isIncreasing);
        while (_history.Count > 3)
            _history.Dequeue();
    }

    private bool IsHistoryConsistent()
        => _history.Count == 0 || _history.All(x => x) || _history.All(x => !x);

    private static double GetRatio(double width, double height)
    {
        if (height == 0) return 0;
        return Math.Round(Math.Truncate(Math.Abs(width)) / Math.Truncate(Math.Abs(height)), 3);
    }

    private AspectRatioMode GetMode(double ratio)
    {
        var abs = Math.Abs(AspectRatioTolerance);
        var high = 1d + abs;
        var low = 1d - abs;
        if (ratio >= high) return AspectRatioMode.HorizontalRectangle;
        if (low < ratio && ratio < high) return AspectRatioMode.Square;
        if (ratio <= low) return AspectRatioMode.VerticalRectangle;
        return AspectRatioMode.None;
    }
}
