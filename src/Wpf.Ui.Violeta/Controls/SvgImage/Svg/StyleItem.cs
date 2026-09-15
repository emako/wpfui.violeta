using System;
using System.Diagnostics.CodeAnalysis;

namespace Wpf.Ui.Violeta.Controls.Svg;

public class StyleItem(string name, string value)
{
    private string _value = value;

    public string Name
    {
        get => name;
        set => name = value;
    }

    public string Value
    {
        get => _value;
        set => _value = value;
    }

    public override string ToString()
    {
        return string.Format("{0}:{1}", name, _value);
    }

    [SuppressMessage("Style", "IDE0057:Use range operator")]
    public static StyleItem ReadNextAttr(string inputstring, ref int startpos)
    {
        if (inputstring[startpos] != ' ')
            throw new Exception("inputstring[startpos] must be a whitepace character");
        while (inputstring[startpos] == ' ')
            startpos++;

        int namestart = startpos;
        int nameend = inputstring.IndexOf('=', startpos);
        if (nameend < namestart)
            throw new Exception("did not find xml attribute name");

        int valuestart = inputstring.IndexOf('"', nameend);
        valuestart++;
        int valueend = inputstring.IndexOf('"', valuestart);
        if (valueend < 0 || valueend < valuestart)
            throw new Exception("did not find xml attribute value");

        // search for first occurence of x="yy"
        string attrName = inputstring.Substring(namestart, nameend - namestart).Trim();
        string attrValue = inputstring.Substring(valuestart, valueend - valuestart).Trim();
        startpos = valueend + 1;

        return new StyleItem(attrName, attrValue);
    }
}
