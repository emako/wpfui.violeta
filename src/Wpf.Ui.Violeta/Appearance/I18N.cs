using System.Globalization;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Appearance;

public static class I18N
{
    /// <summary>
    /// Expose the getter and setter of internal resource languages
    /// </summary>
    public static CultureInfo Culture
    {
        get => SH.Culture;
        set => SH.Culture = value;
    }
}
