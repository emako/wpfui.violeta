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
        set
        {
            // "zh" is a Neutral Chinese language that requires fallback to Simplified Chinese named "zh-Hans"
            if (value.ToString() == "zh")
            {
                value = new CultureInfo("zh-Hans");
            }
            SH.Culture = value;
        }
    }
}
