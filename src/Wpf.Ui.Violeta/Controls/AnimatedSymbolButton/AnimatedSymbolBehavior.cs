using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Strategy that drives one <see cref="AnimatedSymbolKind"/> on an <see cref="AnimatedSymbolButton"/>.
/// </summary>
internal abstract class AnimatedSymbolBehavior
{
    protected AnimatedSymbolButton? Owner { get; private set; }

    protected FrameworkElement? RootGrid { get; private set; }

    protected FrameworkElement? ClipHost { get; private set; }

    protected FrameworkElement? AnimatedVisual { get; private set; }

    protected FrameworkElement? SuccessGlyph { get; private set; }

    protected ScaleTransform? ScaleTransform { get; private set; }

    protected TranslateTransform? TranslateTransform { get; private set; }

    protected RotateTransform? RotateTransform { get; private set; }

    /// <summary>Segoe Fluent Icons glyph used when <see cref="AnimatedSymbolButton.Icon"/> is unset.</summary>
    public abstract string DefaultGlyph { get; }

    /// <summary>RenderTransformOrigin applied to the animated visual for this kind.</summary>
    public virtual Point RenderTransformOrigin => new(0.5, 0.5);

    /// <summary>Whether <c>PART_ClipHost</c> should clip (ChevronDownSmall).</summary>
    public virtual bool ClipToBounds => false;

    /// <summary>Optional fixed clip viewport; <see cref="Size.Empty"/> means auto-size.</summary>
    public virtual Size ClipViewport => Size.Empty;

    public static AnimatedSymbolBehavior? Create(AnimatedSymbolKind kind) =>
        kind switch
        {
            AnimatedSymbolKind.Back => new AnimatedSymbolButton.BackBehavior(),
            AnimatedSymbolKind.Settings => new AnimatedSymbolButton.SettingsBehavior(),
            AnimatedSymbolKind.ChevronDownSmall => new AnimatedSymbolButton.ChevronDownSmallBehavior(),
            AnimatedSymbolKind.ChevronUpDownSmall => new AnimatedSymbolButton.ChevronUpDownSmallBehavior(),
            AnimatedSymbolKind.GlobalNavigationButton => new AnimatedSymbolButton.GlobalNavigationButtonBehavior(),
            AnimatedSymbolKind.CopyToClipboard => new AnimatedSymbolButton.CopyToClipboardBehavior(),
            _ => null,
        };

    public void Attach(AnimatedSymbolButton owner)
    {
        Detach();
        Owner = owner;
        ResolveParts();
        ApplyVisualDefaults();
        OnAttached();
    }

    public void Detach()
    {
        if (Owner is null)
        {
            return;
        }

        OnDetaching();
        Owner = null;
        RootGrid = null;
        ClipHost = null;
        AnimatedVisual = null;
        SuccessGlyph = null;
        ScaleTransform = null;
        TranslateTransform = null;
        RotateTransform = null;
    }

    public void OnTemplateApplied()
    {
        if (Owner is null)
        {
            return;
        }

        OnDetaching();
        ResolveParts();
        ApplyVisualDefaults();
        OnAttached();
    }

    protected virtual void OnAttached()
    {
    }

    protected virtual void OnDetaching()
    {
    }

    /// <summary>Invoked from <see cref="AnimatedSymbolButton.OnClick"/> after clipboard / expand handling.</summary>
    public virtual void OnClick()
    {
    }

    protected virtual void OnExpandedChanged(bool isExpanded)
    {
    }

    internal void NotifyExpandedChanged(bool isExpanded) => OnExpandedChanged(isExpanded);

    private void ResolveParts()
    {
        if (Owner is null)
        {
            return;
        }

        RootGrid = Owner.GetPart(AnimatedSymbolButton.RootGridPart) as FrameworkElement;
        ClipHost = Owner.GetPart(AnimatedSymbolButton.ClipHostPart) as FrameworkElement;
        AnimatedVisual = Owner.GetPart(AnimatedSymbolButton.AnimatedVisualPart) as FrameworkElement;
        SuccessGlyph = Owner.GetPart(AnimatedSymbolButton.SuccessGlyphPart) as FrameworkElement;

        if (AnimatedVisual?.RenderTransform is TransformGroup group)
        {
            ScaleTransform = group.Children.Count > 0 ? group.Children[0] as ScaleTransform : null;
            TranslateTransform = group.Children.Count > 1 ? group.Children[1] as TranslateTransform : null;
            RotateTransform = group.Children.Count > 2 ? group.Children[2] as RotateTransform : null;
        }
    }

    private void ApplyVisualDefaults()
    {
        if (AnimatedVisual is not null)
        {
            AnimatedVisual.RenderTransformOrigin = RenderTransformOrigin;
            AnimatedVisual.Opacity = 1;
            if (ScaleTransform is not null)
            {
                ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                ScaleTransform.ScaleX = 1;
                ScaleTransform.ScaleY = 1;
            }

            if (TranslateTransform is not null)
            {
                TranslateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                TranslateTransform.Y = 0;
            }

            if (RotateTransform is not null)
            {
                RotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
                RotateTransform.Angle = 0;
            }
        }

        if (ClipHost is not null)
        {
            ClipHost.ClipToBounds = ClipToBounds;
            if (ClipViewport != Size.Empty)
            {
                ClipHost.Width = ClipViewport.Width;
                ClipHost.Height = ClipViewport.Height;
            }
            else
            {
                ClipHost.ClearValue(FrameworkElement.WidthProperty);
                ClipHost.ClearValue(FrameworkElement.HeightProperty);
            }
        }

        if (SuccessGlyph is not null)
        {
            SuccessGlyph.Opacity = 0;
            SuccessGlyph.Visibility =
                this is AnimatedSymbolButton.CopyToClipboardBehavior
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }
    }

    protected static DoubleAnimation CreateDoubleAnimation(
        double to,
        TimeSpan duration,
        IEasingFunction? easing = null,
        FillBehavior fill = FillBehavior.HoldEnd)
    {
        return new DoubleAnimation
        {
            To = to,
            Duration = duration,
            EasingFunction = easing,
            FillBehavior = fill,
        };
    }
}
