using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button that plays a WinUI Gallery–style copy → checkmark success animation on click.
/// Optionally copies <see cref="TextToCopy"/> to the clipboard before animating.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;vio:CopyButton TextToCopy="Hello" /&gt;
/// &lt;vio:CopyButton Command="{Binding CopyCommand}" Content="{x:Null}"&gt;
///     &lt;vio:CopyButton.Icon&gt;
///         &lt;ui:SymbolIcon Symbol="Copy24" /&gt;
///     &lt;/vio:CopyButton.Icon&gt;
/// &lt;/vio:CopyButton&gt;
/// </code>
/// </example>
public class CopyButton : Wpf.Ui.Controls.Button
{
    private FrameworkElement? _rootGrid;
    private Storyboard? _successAnimation;
    private bool _isAnimating;

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

    private void OnSuccessAnimationCompleted(object? sender, EventArgs e)
    {
        _isAnimating = false;
    }
}
