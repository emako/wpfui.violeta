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
/// &lt;vio:CopyButton Content="&#xE8C8;" Click="OnCopyClick" /&gt;
/// </code>
/// </example>
public class CopyButton : Wpf.Ui.Controls.Button
{
    private FrameworkElement? _rootGrid;
    private Storyboard? _successAnimation;

    /// <summary>Identifies the <see cref="TextToCopy"/> dependency property.</summary>
    public static readonly DependencyProperty TextToCopyProperty =
        DependencyProperty.Register(
            nameof(TextToCopy),
            typeof(string),
            typeof(CopyButton),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the text copied to the clipboard when the button is clicked.
    /// When null or empty, only the success animation runs (clipboard is left to the Click handler).
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
        Click -= OnCopyButtonClick;
        base.OnApplyTemplate();

        _rootGrid = GetTemplateChild("PART_RootGrid") as FrameworkElement;
        _successAnimation = null;

        if (_rootGrid?.Resources["CopyToClipboardSuccessAnimation"] is Storyboard storyboard)
        {
            _successAnimation = storyboard.IsFrozen ? storyboard.Clone() : storyboard;
        }

        Click += OnCopyButtonClick;
    }

    /// <summary>
    /// Plays the copy-success animation (copy icon shrinks out, checkmark pops in, then restores).
    /// </summary>
    public void PlaySuccessAnimation()
    {
        if (_rootGrid is null || _successAnimation is null)
        {
            return;
        }

        _successAnimation.Stop(_rootGrid);
        _successAnimation.Begin(_rootGrid, true);
    }

    private void OnCopyButtonClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TextToCopy))
        {
            Win32.Clipboard.SetText(TextToCopy);
        }

        PlaySuccessAnimation();
    }
}
