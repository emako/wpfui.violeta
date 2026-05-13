using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// An item in an <see cref="AspectRatioLayout"/> that declares which aspect-ratio range or mode it handles.
/// </summary>
public class AspectRatioLayoutItem : ContentControl
{
    static AspectRatioLayoutItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AspectRatioLayoutItem),
            new FrameworkPropertyMetadata(typeof(AspectRatioLayoutItem)));

        IsTabStopProperty.OverrideMetadata(
            typeof(AspectRatioLayoutItem),
            new FrameworkPropertyMetadata(false));

        FocusableProperty.OverrideMetadata(
            typeof(AspectRatioLayoutItem),
            new FrameworkPropertyMetadata(false));
    }

    #region AcceptAspectRatioMode

    public static readonly DependencyProperty AcceptAspectRatioModeProperty =
        DependencyProperty.Register(
            nameof(AcceptAspectRatioMode),
            typeof(AspectRatioMode),
            typeof(AspectRatioLayoutItem),
            new PropertyMetadata(AspectRatioMode.None));

    public AspectRatioMode AcceptAspectRatioMode
    {
        get => (AspectRatioMode)GetValue(AcceptAspectRatioModeProperty);
        set => SetValue(AcceptAspectRatioModeProperty, value);
    }

    #endregion

    #region StartAspectRatioValue / EndAspectRatioValue

    public static readonly DependencyProperty StartAspectRatioValueProperty =
        DependencyProperty.Register(
            nameof(StartAspectRatioValue),
            typeof(double),
            typeof(AspectRatioLayoutItem),
            new PropertyMetadata(double.NaN));

    public double StartAspectRatioValue
    {
        get => (double)GetValue(StartAspectRatioValueProperty);
        set => SetValue(StartAspectRatioValueProperty, value);
    }

    public static readonly DependencyProperty EndAspectRatioValueProperty =
        DependencyProperty.Register(
            nameof(EndAspectRatioValue),
            typeof(double),
            typeof(AspectRatioLayoutItem),
            new PropertyMetadata(double.NaN));

    public double EndAspectRatioValue
    {
        get => (double)GetValue(EndAspectRatioValueProperty);
        set => SetValue(EndAspectRatioValueProperty, value);
    }

    /// <summary>
    /// <see langword="true"/> when both <see cref="StartAspectRatioValue"/> and
    /// <see cref="EndAspectRatioValue"/> form a valid range.
    /// </summary>
    public bool IsUseAspectRatioRange =>
        !double.IsNaN(StartAspectRatioValue)
        && !double.IsNaN(EndAspectRatioValue)
        && !(StartAspectRatioValue > EndAspectRatioValue);

    #endregion
}
