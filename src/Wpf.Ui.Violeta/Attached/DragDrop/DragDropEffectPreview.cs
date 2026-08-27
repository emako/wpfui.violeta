using System.Windows;

namespace Wpf.Ui.Violeta.Attached.DragDrop;

internal class DragDropEffectPreview(UIElement rootElement, UIElement previewElement, Point translation, DragDropEffects effects, string effectText, string destinationText) : DragDropPreview(rootElement, previewElement, translation, default)
{
    public DragDropEffects Effects { get; set; } = effects;

    public string EffectText { get; set; } = effectText;

    public string DestinationText { get; set; } = destinationText;
}
