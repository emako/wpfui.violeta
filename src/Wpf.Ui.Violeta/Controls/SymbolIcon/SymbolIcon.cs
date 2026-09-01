using System;
using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Fluent <see cref="SymbolRegular"/> icon that correctly encodes code points above U+FFFF.
/// </summary>
/// <remarks>
/// Upstream <c>Wpf.Ui.Controls.SymbolIcon</c> uses <c>SymbolExtensions.GetString</c>, which
/// treats the enum value as raw UTF-16 code units. Glyphs such as
/// <c>ArrowExpand16 = 0xF0382</c> therefore render blank. Prefer this control via
/// <c>vio:SymbolIcon</c> until upstream is fixed.
/// </remarks>
public class SymbolIcon : FontIcon
{
    /// <summary>Identifies the <see cref="Symbol"/> dependency property.</summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol),
        typeof(SymbolRegular),
        typeof(SymbolIcon),
        new PropertyMetadata(SymbolRegular.Empty, static (o, _) => ((SymbolIcon)o).OnGlyphChanged())
    );

    /// <summary>Identifies the <see cref="Filled"/> dependency property.</summary>
    public static readonly DependencyProperty FilledProperty = DependencyProperty.Register(
        nameof(Filled),
        typeof(bool),
        typeof(SymbolIcon),
        new PropertyMetadata(false, OnFilledChanged)
    );

    /// <summary>
    /// Gets or sets displayed <see cref="SymbolRegular"/>.
    /// </summary>
    public SymbolRegular Symbol
    {
        get => (SymbolRegular)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not we should use the <see cref="SymbolFilled"/>.
    /// </summary>
    public bool Filled
    {
        get => (bool)GetValue(FilledProperty);
        set => SetValue(FilledProperty, value);
    }

    public SymbolIcon()
    {
    }

    public SymbolIcon(SymbolRegular symbol, double fontSize = 14, bool filled = false)
    {
        Symbol = symbol;
        Filled = filled;
        FontSize = fontSize;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        SetFontReference();
    }

    private void OnGlyphChanged()
    {
        SetCurrentValue(GlyphProperty, Filled ? ToGlyph(Symbol.Swap()) : ToGlyph(Symbol));
    }

    private void SetFontReference()
    {
        SetResourceReference(FontFamilyProperty, Filled ? "FluentSystemIconsFilled" : "FluentSystemIcons");
    }

    private static void OnFilledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (SymbolIcon)d;
        self.SetFontReference();
        self.OnGlyphChanged();
    }

    /// <summary>
    /// Converts a Fluent symbol enum value to a UTF-16 string (including surrogate pairs).
    /// </summary>
    internal static string ToGlyph(SymbolRegular icon)
    {
        // Hotfix: https://github.com/lepoco/wpfui/issues/1736
        return icon == SymbolRegular.Empty ? string.Empty : char.ConvertFromUtf32((int)icon);
    }

    /// <summary>
    /// Converts a Fluent filled symbol enum value to a UTF-16 string (including surrogate pairs).
    /// </summary>
    internal static string ToGlyph(SymbolFilled icon)
    {
        // Hotfix: https://github.com/lepoco/wpfui/issues/1736
        return icon == SymbolFilled.Empty ? string.Empty : char.ConvertFromUtf32((int)icon);
    }
}
