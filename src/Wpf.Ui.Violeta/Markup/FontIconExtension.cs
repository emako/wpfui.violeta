using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Markup;

/// <summary>
/// Custom <see cref="MarkupExtension"/> which can provide <see cref="FontIcon"/>.
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:Button
///     Appearance="Primary"
///     Content="WPF UI button with font icon"
///     Icon="{ui:FontIcon '&#x1F308;'}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:Button Icon="{ui:FontIcon '&amp;#x1F308;'}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:HyperlinkButton Icon="{ui:FontIcon '&amp;#x1F308;'}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:TitleBar Icon="{ui:FontIcon '&amp;#x1F308;'}" /&gt;
/// </code>
/// <code lang="xml">
/// &lt;ui:Button Icon="{ui:FontIcon Glyph='&amp;#xE80F;' FontFamily={DynamicResource SymbolThemeFontFamily}}" /&gt;
/// </code>
/// </example>
/// <remarks>
/// <see cref="MarkupExtension"/> cannot host dependency properties, so nested
/// <c>{DynamicResource}</c> on <see cref="FontFamily"/> is accepted via
/// <see cref="XamlSetMarkupExtensionAttribute"/> and applied with
/// <see cref="FrameworkElement.SetResourceReference"/> on the produced <see cref="FontIcon"/>.
/// </remarks>
[ContentProperty(nameof(Glyph))]
[MarkupExtensionReturnType(typeof(FontIcon))]
[XamlSetMarkupExtension(nameof(ReceiveMarkupExtension))]
public class FontIconExtension : MarkupExtension
{
    private object? _fontFamilyResourceKey;

    public FontIconExtension()
    {
    }

    public FontIconExtension(string glyph)
    {
        Glyph = glyph;
    }

    [ConstructorArgument("glyph")]
    public string? Glyph { get; set; }

    public FontFamily FontFamily
    {
        get;
        set
        {
            field = value ?? new FontFamily("FluentSystemIcons");
            _fontFamilyResourceKey = null;
        }
    } = new FontFamily("FluentSystemIcons");

    public double FontSize { get; set; }

    /// <summary>
    /// Intercepts nested markup extensions on this extension's properties.
    /// Required for <see cref="DynamicResourceExtension"/>, which can only target a dependency property.
    /// </summary>
    public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
    {
        if (targetObject is not FontIconExtension extension)
        {
            return;
        }

        if (eventArgs.Member.Name != nameof(FontFamily))
        {
            return;
        }

        if (eventArgs.MarkupExtension is DynamicResourceExtension dynamicResource)
        {
            extension._fontFamilyResourceKey = dynamicResource.ResourceKey;
            eventArgs.Handled = true;
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        FontIcon fontIcon = new() { Glyph = Glyph! };

        if (_fontFamilyResourceKey is not null)
        {
            fontIcon.SetResourceReference(FontIcon.FontFamilyProperty, _fontFamilyResourceKey);
        }
        else
        {
            fontIcon.FontFamily = FontFamily;
        }

        if (FontSize > 0)
        {
            fontIcon.FontSize = FontSize;
        }

        return fontIcon;
    }
}
