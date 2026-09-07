namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Backdrop material for <see cref="FluentPopup"/>.
/// </summary>
public enum FluentPopupMaterial
{
    /// <summary>
    /// Legacy composition acrylic (FluentWpfCore default). Tint via <see cref="FluentPopup.Background"/>.
    /// </summary>
    Acrylic = 0,

    /// <summary>
    /// Windows 11 Mica (<c>DWMSBT_MAINWINDOW</c>) — desktop wallpaper material.
    /// Requires a fully transparent <see cref="FluentPopup.Background"/>.
    /// </summary>
    Mica = 1,

    /// <summary>
    /// Windows 11 Mica Alt (<c>DWMSBT_TABBEDWINDOW</c>).
    /// Requires a fully transparent <see cref="FluentPopup.Background"/>.
    /// </summary>
    MicaAlt = 2,

    /// <summary>
    /// Windows 11 flyout acrylic (<c>DWMSBT_TRANSIENTWINDOW</c>).
    /// Requires a fully transparent <see cref="FluentPopup.Background"/>.
    /// </summary>
    SystemAcrylic = 3,
}
