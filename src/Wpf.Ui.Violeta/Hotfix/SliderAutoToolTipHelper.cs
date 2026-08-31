using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Wpf.Ui.Violeta.Hotfix;

/// <summary>
/// WinUI-style Slider AutoToolTip placement and Fluent style attachment for Hotfix Slider.
/// </summary>
public static class SliderAutoToolTipHelper
{
    private const string AutoToolTipStyleKey = "SliderAutoToolTipStyle";

    #region Attach

    /// <summary>
    /// When set on a <see cref="Thumb"/>, applies Fluent AutoToolTip styling after WPF creates the tooltip on drag.
    /// </summary>
    public static bool GetAttach(Thumb thumb)
    {
        return (bool)thumb.GetValue(AttachProperty);
    }

    public static void SetAttach(Thumb thumb, bool value)
    {
        thumb.SetValue(AttachProperty, value);
    }

    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached(
            "Attach",
            typeof(bool),
            typeof(SliderAutoToolTipHelper),
            new PropertyMetadata(false, OnAttachChanged));

    private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Thumb thumb)
        {
            return;
        }

        thumb.DragStarted -= OnThumbDragStarted;
        if ((bool)e.NewValue)
        {
            thumb.DragStarted += OnThumbDragStarted;
        }
    }

    private static void OnThumbDragStarted(object sender, DragStartedEventArgs e)
    {
        var thumb = (Thumb)sender;

        // Slider.OnThumbDragStarted creates _autoToolTip after this handler; defer configuration.
        _ = thumb.Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            () => ConfigureAutoToolTip(thumb));
    }

    private static void ConfigureAutoToolTip(Thumb thumb)
    {
        if (thumb.ToolTip is not ToolTip toolTip)
        {
            return;
        }

        ApplyFluentStyle(toolTip, thumb);
        SetIsEnabled(toolTip, true);
    }

    private static void ApplyFluentStyle(ToolTip toolTip, FrameworkElement resourceHost)
    {
        object? style =
            resourceHost.TryFindResource(AutoToolTipStyleKey)
            ?? Application.Current?.TryFindResource(AutoToolTipStyleKey)
            ?? resourceHost.TryFindResource(typeof(ToolTip))
            ?? Application.Current?.TryFindResource(typeof(ToolTip));

        if (style is Style toolTipStyle)
        {
            toolTip.Style = toolTipStyle;
        }
    }

    #endregion Attach

    #region IsEnabled

    public static bool GetIsEnabled(ToolTip toolTip)
    {
        return (bool)toolTip.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(ToolTip toolTip, bool value)
    {
        toolTip.SetValue(IsEnabledProperty, value);
    }

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SliderAutoToolTipHelper),
            new PropertyMetadata(OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var toolTip = (ToolTip)d;
        if ((bool)e.NewValue)
        {
            WirePlacement(toolTip);

            toolTip.IsVisibleChanged -= OnToolTipIsVisibleChanged;
            toolTip.IsVisibleChanged += OnToolTipIsVisibleChanged;

            if (toolTip.IsVisible && toolTip.PlacementTarget is Thumb visibleThumb)
            {
                UpdatePlacementRectangle(toolTip, visibleThumb.RenderSize);
            }
        }
        else
        {
            if (toolTip.ReadLocalValue(OriginalCustomPopupPlacementCallbackProperty) != DependencyProperty.UnsetValue)
            {
                toolTip.CustomPopupPlacementCallback = GetOriginalCustomPopupPlacementCallback(toolTip);
                toolTip.ClearValue(OriginalCustomPopupPlacementCallbackProperty);
            }

            toolTip.IsVisibleChanged -= OnToolTipIsVisibleChanged;
        }
    }

    private static void WirePlacement(ToolTip toolTip)
    {
        if (toolTip.PlacementTarget is Thumb thumb &&
            thumb.TemplatedParent is Slider slider)
        {
            if (toolTip.ReadLocalValue(OriginalCustomPopupPlacementCallbackProperty) == DependencyProperty.UnsetValue)
            {
                SetOriginalCustomPopupPlacementCallback(toolTip, toolTip.CustomPopupPlacementCallback);
            }

            toolTip.Placement = PlacementMode.Custom;
            toolTip.CustomPopupPlacementCallback = (popupSize, targetSize, offset) =>
                PositionAutoToolTip(slider, toolTip, popupSize, targetSize);
        }
    }

    #endregion IsEnabled

    #region OriginalCustomPopupPlacementCallback

    private static readonly DependencyProperty OriginalCustomPopupPlacementCallbackProperty =
        DependencyProperty.RegisterAttached(
            "OriginalCustomPopupPlacementCallback",
            typeof(CustomPopupPlacementCallback),
            typeof(SliderAutoToolTipHelper));

    private static CustomPopupPlacementCallback GetOriginalCustomPopupPlacementCallback(ToolTip toolTip)
    {
        return (CustomPopupPlacementCallback)toolTip.GetValue(OriginalCustomPopupPlacementCallbackProperty);
    }

    private static void SetOriginalCustomPopupPlacementCallback(ToolTip toolTip, CustomPopupPlacementCallback value)
    {
        toolTip.SetValue(OriginalCustomPopupPlacementCallbackProperty, value);
    }

    #endregion OriginalCustomPopupPlacementCallback

    private static void OnToolTipIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        var toolTip = (ToolTip)sender!;
        Debug.Assert(toolTip.PlacementTarget is Thumb);
        if (toolTip.PlacementTarget is Thumb thumb)
        {
            if ((bool)e.NewValue)
            {
                ApplyFluentStyle(toolTip, thumb);
                WirePlacement(toolTip);

                thumb.SizeChanged += OnThumbSizeChanged;
                UpdatePlacementRectangle(toolTip, thumb.RenderSize);
            }
            else
            {
                thumb.SizeChanged -= OnThumbSizeChanged;
                toolTip.ClearValue(ToolTip.PlacementRectangleProperty);
            }
        }
    }

    private static void OnThumbSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var thumb = (Thumb)sender!;
        if (thumb.ToolTip is ToolTip toolTip)
        {
            UpdatePlacementRectangle(toolTip, e.NewSize);
        }
    }

    private static void UpdatePlacementRectangle(ToolTip toolTip, Size targetSize)
    {
        toolTip.PlacementRectangle = new Rect(
            new Point(-20, -20),
            new Point(targetSize.Width + 20, targetSize.Height + 20));
    }

    private static CustomPopupPlacement[] PositionAutoToolTip(
        Slider slider,
        ToolTip autoToolTip,
        Size popupSize,
        Size targetSize)
    {
        Point point;
        PopupPrimaryAxis primaryAxis;

        switch (slider.AutoToolTipPlacement)
        {
            case AutoToolTipPlacement.TopLeft:
                if (slider.Orientation == Orientation.Horizontal)
                {
                    point = new Point((targetSize.Width - popupSize.Width) * 0.5, -popupSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                }
                else
                {
                    point = new Point(-popupSize.Width, (targetSize.Height - popupSize.Height) * 0.5);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                }
                break;

            case AutoToolTipPlacement.BottomRight:
                if (slider.Orientation == Orientation.Horizontal)
                {
                    point = new Point((targetSize.Width - popupSize.Width) * 0.5, targetSize.Height);
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                }
                else
                {
                    point = new Point(targetSize.Width, (targetSize.Height - popupSize.Height) * 0.5);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                }
                break;

            default:
                return [];
        }

        if (TryGetTransformToDevice(autoToolTip, out Matrix transformToDevice))
        {
            Vector offset = VisualTreeHelper.GetOffset(autoToolTip);
            point -= transformToDevice.Transform(offset);
        }

        return [new CustomPopupPlacement(point, primaryAxis)];
    }

    private static bool TryGetTransformToDevice(Visual visual, out Matrix value)
    {
        var presentationSource = PresentationSource.FromVisual(visual);
        if (presentationSource?.CompositionTarget is { } target)
        {
            value = target.TransformToDevice;
            return true;
        }

        value = default;
        return false;
    }
}
