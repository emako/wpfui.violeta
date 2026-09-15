namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Identifies a caption (title-bar) button and the Win32 <c>WM_NCHITTEST</c>
/// hit-test code returned for it via <see cref="CaptionButtonHandler"/>.
/// Numeric values match the corresponding <c>HT*</c> constants where applicable.
/// </summary>
public enum CaptionButtonKind
{
    /// <summary>No caption button / <c>HTNOWHERE</c> (0).</summary>
    None = 0,

    /// <summary>Minimize button / <c>HTMINBUTTON</c> (8).</summary>
    Minimize = 8,

    /// <summary>Maximize or restore button / <c>HTMAXBUTTON</c> (9).</summary>
    Maximize = 9,

    /// <summary>Close button / <c>HTCLOSE</c> (20).</summary>
    Close = 20,

    /// <summary>Help (?) button / <c>HTHELP</c> (21).</summary>
    Help = 21,

    /// <summary>
    /// Overflow / more (...) button. Custom code after <c>HTHELP</c>; not a system HT constant.
    /// </summary>
    More = 22,
}
