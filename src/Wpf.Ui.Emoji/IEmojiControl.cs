using System.Collections.Generic;

namespace Wpf.Ui.Emoji;

internal interface IEmojiControl
{
    /// <summary>
    /// Specify whether emoji are blended with the foreground colour.
    /// </summary>
    public bool ColorBlend { get; set; }

    /// <summary>
    /// Enumerate all EmojiInline instances managed by this object.
    /// </summary>
    public IEnumerable<EmojiInline> EmojiInlines { get; }
}
