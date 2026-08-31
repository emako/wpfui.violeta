using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// WinUI-style Slider / RangeSlider AutoToolTip placement and Fluent style attachment.
/// </summary>
public static class SliderHelper
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
            typeof(SliderHelper),
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
            typeof(SliderHelper),
            new PropertyMetadata(OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var toolTip = (ToolTip)d;
        if ((bool)e.NewValue)
        {
            WirePlacement(toolTip);

            AddContentListener(toolTip);

            toolTip.IsVisibleChanged -= OnToolTipIsVisibleChanged;
            toolTip.IsVisibleChanged += OnToolTipIsVisibleChanged;

            if (toolTip.IsVisible && toolTip.PlacementTarget is Thumb visibleThumb)
            {
                UpdatePlacementRectangle(toolTip, visibleThumb.RenderSize);
            }

            // WPF sets Content before this attached property applies, so reformat once now.
            OnAutoToolTipContentChanged(toolTip, EventArgs.Empty);
        }
        else
        {
            if (toolTip.ReadLocalValue(OriginalCustomPopupPlacementCallbackProperty) != DependencyProperty.UnsetValue)
            {
                toolTip.CustomPopupPlacementCallback = GetOriginalCustomPopupPlacementCallback(toolTip);
                toolTip.ClearValue(OriginalCustomPopupPlacementCallbackProperty);
            }

            RemoveContentListener(toolTip);
            toolTip.IsVisibleChanged -= OnToolTipIsVisibleChanged;
        }
    }

    private static void WirePlacement(ToolTip toolTip)
    {
        if (toolTip.PlacementTarget is Thumb thumb &&
            TryGetAutoToolTipHost(thumb, out _, out var orientation, out var placement))
        {
            if (toolTip.ReadLocalValue(OriginalCustomPopupPlacementCallbackProperty) == DependencyProperty.UnsetValue)
            {
                SetOriginalCustomPopupPlacementCallback(toolTip, toolTip.CustomPopupPlacementCallback);
            }

            toolTip.Placement = PlacementMode.Custom;
            toolTip.CustomPopupPlacementCallback = (popupSize, targetSize, offset) =>
                PositionAutoToolTip(orientation, placement, toolTip, popupSize, targetSize);
        }
    }

    #endregion IsEnabled

    #region Content formatting

    private static readonly DependencyPropertyDescriptor ContentPropertyDescriptor =
        DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, typeof(ToolTip));

    private static void AddContentListener(ToolTip toolTip)
    {
        ContentPropertyDescriptor.AddValueChanged(toolTip, OnAutoToolTipContentChanged);
    }

    private static void RemoveContentListener(ToolTip toolTip)
    {
        ContentPropertyDescriptor.RemoveValueChanged(toolTip, OnAutoToolTipContentChanged);
    }

    private static void OnAutoToolTipContentChanged(object? sender, EventArgs e)
    {
        if (sender is not ToolTip toolTip ||
            toolTip.PlacementTarget is not Thumb thumb ||
            !TryGetAutoToolTipHost(thumb, out var host, out _, out _) ||
            host is null ||
            toolTip.Content is not string content)
        {
            return;
        }

        // When attached to the ToolTip itself, honor the local value over the host's.
        string? prefix = ControlHelper.GetPrefix(toolTip) ?? ControlHelper.GetPrefix(host);
        string? suffix = ControlHelper.GetSuffix(toolTip) ?? ControlHelper.GetSuffix(host);

        if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
        {
            return;
        }

        // Do not re-wrap the value we just wrote.
        if (GetIsFormattingContent(toolTip))
        {
            return;
        }

        SetIsFormattingContent(toolTip, true);
        try
        {
            toolTip.Content = prefix + content + suffix;
        }
        finally
        {
            SetIsFormattingContent(toolTip, false);
        }
    }

    private static readonly DependencyProperty IsFormattingContentProperty =
        DependencyProperty.RegisterAttached(
            "IsFormattingContent",
            typeof(bool),
            typeof(SliderHelper));

    private static bool GetIsFormattingContent(ToolTip toolTip)
    {
        return (bool)toolTip.GetValue(IsFormattingContentProperty);
    }

    private static void SetIsFormattingContent(ToolTip toolTip, bool value)
    {
        toolTip.SetValue(IsFormattingContentProperty, value);
    }

    #endregion Content formatting

    #region OriginalCustomPopupPlacementCallback

    private static readonly DependencyProperty OriginalCustomPopupPlacementCallbackProperty =
        DependencyProperty.RegisterAttached(
            "OriginalCustomPopupPlacementCallback",
            typeof(CustomPopupPlacementCallback),
            typeof(SliderHelper));

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

    private static bool TryGetAutoToolTipHost(
        Thumb thumb,
        out FrameworkElement? host,
        out Orientation orientation,
        out AutoToolTipPlacement placement)
    {
        switch (thumb.TemplatedParent)
        {
            case Slider slider:
                host = slider;
                orientation = slider.Orientation;
                placement = slider.AutoToolTipPlacement;
                return true;

            case RangeSlider rangeSlider:
                host = rangeSlider;
                orientation = rangeSlider.Orientation;
                placement = rangeSlider.AutoToolTipPlacement;
                return true;
        }

        // RangeSlider thumbs are assigned as RangeTrack properties; walk the tree if needed.
        for (DependencyObject? parent = thumb; parent is not null; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent is RangeSlider visualRangeSlider)
            {
                host = visualRangeSlider;
                orientation = visualRangeSlider.Orientation;
                placement = visualRangeSlider.AutoToolTipPlacement;
                return true;
            }
        }

        host = null;
        orientation = default;
        placement = default;
        return false;
    }

    private static CustomPopupPlacement[] PositionAutoToolTip(
        Orientation orientation,
        AutoToolTipPlacement placement,
        ToolTip autoToolTip,
        Size popupSize,
        Size targetSize)
    {
        Point point;
        PopupPrimaryAxis primaryAxis;

        switch (placement)
        {
            case AutoToolTipPlacement.TopLeft:
                if (orientation == Orientation.Horizontal)
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
                if (orientation == Orientation.Horizontal)
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
