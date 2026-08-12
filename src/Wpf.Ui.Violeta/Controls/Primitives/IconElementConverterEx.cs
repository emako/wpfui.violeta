using System;
using System.ComponentModel;
using System.Globalization;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls.Primitives;

/// <summary>
/// Extends <see cref="IconElementConverter"/> so a XAML string
/// (e.g. <c>Icon="&amp;#xEA66;"</c>) becomes a <see cref="FontIcon"/>.
/// </summary>
/// <remarks>
/// Registered at startup via <see cref="TypeDescriptor.AddAttributes(System.Type, System.Attribute[])"/>
/// so it applies to every <see cref="IconElement"/>-typed property (including
/// <see cref="MenuItem.Icon"/>). Must be public so <see cref="TypeDescriptor"/> can instantiate it.
/// <see cref="FontIcon.Glyph"/> is the string. FontFamily / FontSize / Width are left unset so
/// <see cref="ControlHelper"/> can apply <c>SymbolThemeFontFamily</c>,
/// <see cref="ControlHelper.IconFontSizeProperty"/> and
/// <see cref="ControlHelper.IconWidthProperty"/>.
/// </remarks>
public sealed class IconElementConverterEx : TypeConverter
{
    private readonly IconElementConverter _inner = new();

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return _inner.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        _inner.CanConvertTo(context, destinationType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is string glyph)
        {
            if (string.IsNullOrEmpty(glyph))
            {
                return null;
            }

            return CreateFontIconFromGlyph(glyph);
        }

        return _inner.ConvertFrom(context, culture, value!);
    }

    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType) =>
        _inner.ConvertTo(context, culture, value, destinationType);

    /// <summary>
    /// Builds a <see cref="FontIcon"/> from a glyph string (shared with runtime conversion).
    /// FontFamily / FontSize / Width are left unset so <see cref="ControlHelper"/> can apply
    /// <c>SymbolThemeFontFamily</c>, <see cref="ControlHelper.IconFontSizeProperty"/> and
    /// <see cref="ControlHelper.IconWidthProperty"/>.
    /// </summary>
    public static FontIcon CreateFontIconFromGlyph(string glyph)
    {
        var fontIcon = new FontIcon { Glyph = glyph };

        // FontIcon ctor pins DefaultIconFontSize via SetCurrentValue; clear it so
        // ControlHelper.IconFontSize can act as the default size.
        fontIcon.ClearValue(FontIcon.FontSizeProperty);

        return fontIcon;
    }
}
