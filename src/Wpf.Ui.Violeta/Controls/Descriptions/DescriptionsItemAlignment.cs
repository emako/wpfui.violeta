namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Describes the alignment of label and content within a <see cref="DescriptionsItem"/>.
/// </summary>
public enum DescriptionsItemAlignment
{
    /// <summary>Label right-aligned, content left-aligned (table style).</summary>
    Center,

    /// <summary>Label left-aligned, content right-aligned.</summary>
    Justify,

    /// <summary>Both label and content left-aligned.</summary>
    Left,

    /// <summary>Label and content inline with a colon separator.</summary>
    Plain,
}
