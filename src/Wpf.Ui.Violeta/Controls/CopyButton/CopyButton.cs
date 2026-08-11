using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button that plays a WinUI Gallery–style copy → checkmark success animation on click.
/// Optionally copies <see cref="TextToCopy"/> to the clipboard before animating.
/// </summary>
/// <remarks>
/// Icon typography uses <see cref="ControlHelper.IconFontFamilyProperty"/> /
/// <see cref="ControlHelper.IconFontSizeProperty"/>. Values applied to
/// <see cref="Wpf.Ui.Controls.Button.Icon"/> only when the icon has no local
/// <c>FontFamily</c> / <c>FontSize</c> — so an explicit size on
/// <c>SymbolIcon</c> / <c>FontIcon</c> wins over the attached properties.
/// </remarks>
/// <example>
/// <code lang="xml">
/// &lt;vio:CopyButton
///     Content="Copy"
///     ui:ControlHelper.IconFontSize="20"
///     TextToCopy="payload"&gt;
///     &lt;vio:CopyButton.Icon&gt;
///         &lt;ui:SymbolIcon Symbol="Copy24" FontSize="12" /&gt;
///     &lt;/vio:CopyButton.Icon&gt;
/// &lt;/vio:CopyButton&gt;
/// </code>
/// </example>
public class CopyButton : Wpf.Ui.Controls.Button
{
    private FrameworkElement? _rootGrid;
    private Storyboard? _successAnimation;
    private bool _isAnimating;
    private bool _iconAppearanceHooked;

    /// <summary>Identifies the <see cref="TextToCopy"/> dependency property.</summary>
    public static readonly DependencyProperty TextToCopyProperty =
        DependencyProperty.Register(
            nameof(TextToCopy),
            typeof(string),
            typeof(CopyButton),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the text copied to the clipboard when the button is clicked.
    /// When null or empty, clipboard work is left to <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/> / Click.
    /// </summary>
    public string? TextToCopy
    {
        get => (string?)GetValue(TextToCopyProperty);
        set => SetValue(TextToCopyProperty, value);
    }

    /// <summary>Identifies the <see cref="SuccessGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty SuccessGlyphProperty =
        DependencyProperty.Register(
            nameof(SuccessGlyph),
            typeof(string),
            typeof(CopyButton),
            new PropertyMetadata("\uE73E"));

    /// <summary>
    /// Gets or sets the Segoe Fluent Icons glyph shown during the success animation.
    /// Defaults to Accept (<c>E73E</c>), matching WinUI Gallery.
    /// </summary>
    public string SuccessGlyph
    {
        get => (string)GetValue(SuccessGlyphProperty);
        set => SetValue(SuccessGlyphProperty, value);
    }

    static CopyButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CopyButton),
            new FrameworkPropertyMetadata(typeof(CopyButton)));

        IconProperty.OverrideMetadata(
            typeof(CopyButton),
            new FrameworkPropertyMetadata(null, OnIconChanged));
    }

    public CopyButton()
    {
        Loaded += OnLoaded;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        _successAnimation?.Completed -= OnSuccessAnimationCompleted;

        base.OnApplyTemplate();

        _isAnimating = false;
        _rootGrid = GetTemplateChild("PART_RootGrid") as FrameworkElement;
        _successAnimation = null;

        if (_rootGrid?.Resources["CopyToClipboardSuccessAnimation"] is Storyboard storyboard)
        {
            _successAnimation = storyboard.IsFrozen ? storyboard.Clone() : storyboard;
            _successAnimation.Completed += OnSuccessAnimationCompleted;
        }

        HookIconAppearanceProperties();
        SyncIconAppearance();
    }

    /// <summary>
    /// Gates <see cref="System.Windows.Controls.Primitives.ButtonBase.Command"/> and Click together.
    /// While the success animation is running, the whole click path is suppressed
    /// so clipboard / command handlers are not spammed.
    /// </summary>
    protected override void OnClick()
    {
        if (_isAnimating)
        {
            return;
        }

        if (!string.IsNullOrEmpty(TextToCopy))
        {
            Win32.Clipboard.SetText(TextToCopy!);
        }

        PlaySuccessAnimation();
        base.OnClick();
    }

    /// <summary>
    /// Plays the copy-success animation (copy icon shrinks out, checkmark pops in, then restores).
    /// If the animation is already running, this is a no-op so rapid clicks do not restart it.
    /// </summary>
    public void PlaySuccessAnimation()
    {
        if (_rootGrid is null || _successAnimation is null || _isAnimating)
        {
            return;
        }

        _isAnimating = true;
        _successAnimation.Begin(_rootGrid, true);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        HookIconAppearanceProperties();
        SyncIconAppearance();
    }

    private void HookIconAppearanceProperties()
    {
        if (_iconAppearanceHooked)
        {
            return;
        }

        _iconAppearanceHooked = true;
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconFontFamilyProperty, typeof(CopyButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconFontSizeProperty, typeof(CopyButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CopyButton)d).SyncIconAppearance();
    }

    private void OnIconAppearanceAttachedPropertyChanged(object? sender, EventArgs e)
    {
        SyncIconAppearance();
    }

    private void OnSuccessAnimationCompleted(object? sender, EventArgs e)
    {
        _isAnimating = false;
    }

    /// <summary>
    /// Pushes <see cref="ControlHelper"/> icon typography onto <see cref="Icon"/> when the
    /// icon has no local FontFamily / FontSize (explicit Icon values always win).
    /// </summary>
    private void SyncIconAppearance()
    {
        if (Icon is not FontIcon fontIcon)
        {
            return;
        }

        var family = ControlHelper.GetIconFontFamily(this);
        if (family is not null
            && fontIcon.ReadLocalValue(FontIcon.FontFamilyProperty) == DependencyProperty.UnsetValue)
        {
            fontIcon.SetCurrentValue(FontIcon.FontFamilyProperty, family);
        }

        var size = ControlHelper.GetIconFontSize(this);
        if (!double.IsNaN(size)
            && fontIcon.ReadLocalValue(FontIcon.FontSizeProperty) == DependencyProperty.UnsetValue)
        {
            fontIcon.SetCurrentValue(FontIcon.FontSizeProperty, size);
        }
    }
}
