using System.Collections.Generic;
using System.Globalization;
using LiteObservableLanguages;

namespace Wpf.Ui.Violeta.Gallery.Globalization;

/// <summary>
/// Language menu facade — mirrors YouiToolkit.Globalization.MuiLanguageManager.
/// </summary>
public static class MuiLanguageManager
{
    public static string Language { get; private set; } = MuiLanguage.ChineseSimplified;

    public static string LanguageDefault => MuiLanguage.ChineseSimplified;

    public static Dictionary<string, string> SupportLanguages { get; } = new()
    {
        [MuiLanguage.ChineseSimplified] = "简体中文",
        [MuiLanguage.ChineseTraditional] = "繁體中文",
        [MuiLanguage.English] = "English",
        [MuiLanguage.Japanese] = "日本語",
    };

    public static void SetLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language) || !SupportLanguages.ContainsKey(language))
        {
            language = LanguageDefault;
        }

        Language = language;
        Locale.Default.Culture = ToCultureInfo(language);
    }

    public static CultureInfo ToCultureInfo(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return new CultureInfo("zh-Hans");
        }

        return languageCode switch
        {
            MuiLanguage.ChineseSimplified => new CultureInfo("zh-Hans"),
            MuiLanguage.ChineseTraditional => new CultureInfo("zh-Hant"),
            MuiLanguage.Japanese => new CultureInfo("ja"),
            MuiLanguage.English => new CultureInfo("en-US"),
            _ => new CultureInfo(languageCode),
        };
    }
}
