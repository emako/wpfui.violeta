namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Selects which symbol animation an <see cref="AnimatedSymbolButton"/> plays.
/// Member names mirror <c>Microsoft.UI.Xaml.Controls.AnimatedVisuals</c> sources
/// (the <c>Animated</c> / <c>VisualSource</c> affixes are omitted), plus
/// <see cref="CopyToClipboard"/> from the WinUI Gallery copy-success pattern.
/// </summary>
public enum AnimatedSymbolKind
{
    /// <summary>No animation.</summary>
    None = 0,

    /// <summary>
    /// <c>AnimatedBackVisualSource</c> — RightToLeft <c>ScaleX</c> press
    /// (TitleBar back / <see cref="GoBackButton"/>).
    /// </summary>
    Back,

    /// <summary>
    /// <c>AnimatedSettingsVisualSource</c> — gear winds back on press, full turn on release
    /// (NavigationView settings item).
    /// </summary>
    Settings,

    /// <summary>
    /// <c>AnimatedChevronDownSmallVisualSource</c> — clipped translate bounce
    /// (<see cref="DropDownButton"/> chevron).
    /// </summary>
    ChevronDownSmall,

    /// <summary>
    /// <c>AnimatedChevronUpDownSmallVisualSource</c> — 0°↔180° rotate
    /// (ComboBox / ToggleComboBox chevron).
    /// </summary>
    ChevronUpDownSmall,

    /// <summary>
    /// <c>AnimatedGlobalNavigationButtonVisualSource</c> — horizontal squash press
    /// (NavigationView pane toggle / hamburger).
    /// </summary>
    GlobalNavigationButton,

    /// <summary>
    /// WinUI Gallery <c>CopyToClipboardSuccessAnimation</c> — copy glyph morphs to Accept
    /// (<see cref="CopyButton"/>).
    /// </summary>
    CopyToClipboard,

    /// <summary>
    /// Spinning arc indicator in the symbol slot when <see cref="AnimatedSymbolButton.IsLoading"/>
    /// is <c>true</c> (same 300° arc / 0.8 s rotation as <see cref="LoadingButton"/>).
    /// </summary>
    Spin,
}
