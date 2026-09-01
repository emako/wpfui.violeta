using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Markup;
using Wpf.Ui.Controls;
using SymbolIcon = Wpf.Ui.Violeta.Controls.SymbolIcon;

namespace Wpf.Ui.Violeta.Markup;

/// <summary>
/// Markup extension that provides a Violeta <see cref="SymbolIcon"/> with correct
/// UTF-32 glyph encoding (workaround for upstream <c>GetString</c> bug).
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;ui:Button Icon="{vio:SymbolIcon ArrowExpand16}" /&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Symbol))]
[MarkupExtensionReturnType(typeof(SymbolIcon))]
public class SymbolIconExtension : MarkupExtension
{
    public SymbolIconExtension()
    {
    }

    public SymbolIconExtension(SymbolRegular symbol)
    {
        Symbol = symbol;
    }

    [SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known")]
    public SymbolIconExtension(string symbol)
    {
        Symbol = (SymbolRegular)Enum.Parse(typeof(SymbolRegular), symbol);
    }

    public SymbolIconExtension(SymbolRegular symbol, bool filled)
        : this(symbol)
    {
        Filled = filled;
    }

    [ConstructorArgument("symbol")]
    public SymbolRegular Symbol { get; set; }

    [ConstructorArgument("filled")]
    public bool Filled { get; set; }

    public double FontSize { get; set; }

    /// <summary>
    /// Width of the produced <see cref="SymbolIcon"/>. Defaults to <see cref="double.NaN"/> (auto).
    /// </summary>
    public double Width { get; set; } = double.NaN;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        SymbolIcon symbolIcon = new() { Symbol = Symbol, Filled = Filled };

        if (FontSize > 0)
        {
            symbolIcon.FontSize = FontSize;
        }

        if (!double.IsNaN(Width))
        {
            symbolIcon.Width = Width;
        }

        return symbolIcon;
    }
}
