using System.Windows;
using System.Windows.Documents;

namespace Wpf.Ui.Emoji;

public static class EmojiBehaviors
{
    /// <summary>
    /// Using a DependencyProperty as the backing store for EmojiRendering.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty EmojiRenderingProperty =
        DependencyProperty.RegisterAttached("EmojiRendering", typeof(bool), typeof(EmojiBehaviors),
                                            new UIPropertyMetadata(false, EmojiRenderingChanged));

    public static bool GetEmojiRendering(DependencyObject o)
        => (bool)o.GetValue(EmojiRenderingProperty);

    public static void SetEmojiRendering(DependencyObject o, bool value)
        => o.SetValue(EmojiRenderingProperty, value);

    private static void EmojiRenderingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FlowDocument doc && (bool)e.NewValue)
            doc.Loaded += FlowDocument_Loaded;
    }

    private static void FlowDocument_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FlowDocument doc)
            doc.SubstituteGlyphs();
    }
}
