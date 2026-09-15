using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Wpf.Ui.Violeta.Controls.Svg.Utils;

internal static class StyleParser
{
    [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
    [SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.")]
    private static Regex _regexStyle =
        new("([\\.,<>a-zA-Z0-9: \\-#]*){([^}]*)}", RegexOptions.Compiled | RegexOptions.Singleline);

    public static List<StyleItem> SplitStyle(SVG svg, string fullstyle)
    {
        _ = svg;

        List<StyleItem> list = [];
        if (fullstyle.Length == 0)
            return list;
        // style contains attributes in format of "attrname:value;attrname:value"
        string[] attrs = fullstyle.Split(';');
        foreach (string attr in attrs)
        {
            string[] s = attr.Split(':');
            if (s.Length != 2)
                continue;
            list.Add(new StyleItem(s[0].Trim(), s[1].Trim()));
        }
        return list;
    }

    [SuppressMessage("Performance", "CA1854:Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method")]
    public static void ParseStyle(SVG svg, string style)
    {
        var svgStyles = svg.Styles;

        var match = _regexStyle.Match(style);
        while (match.Success)
        {
            var name = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim();
            foreach (var nm in name.Split(','))
            {
                if (!svgStyles.ContainsKey(nm))
                {
                    svgStyles.Add(nm, []);
                }

                foreach (StyleItem styleitem in SplitStyle(svg, value))
                {
                    svgStyles[nm].Add(new StyleItem(styleitem.Name, styleitem.Value));
                }
            }

            match = match.NextMatch();
        }
    }
}
