using System;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public static class MessageBoxImageExtensions
{
    public static string ToGlyph(this MessageBoxSymbolGlyph symbol)
    {
        return char.ConvertFromUtf32((int)symbol);
    }

    public static MessageBoxSymbolGlyph ToSymbol(this MessageBoxImage image)
    {
        return image switch
        {
            MessageBoxImage.Error => MessageBoxSymbolGlyph.Error,
            MessageBoxImage.Information => MessageBoxSymbolGlyph.Info,
            MessageBoxImage.Warning => MessageBoxSymbolGlyph.Warning,
            MessageBoxImage.Question => MessageBoxSymbolGlyph.Question,
            MessageBoxImage.None => (MessageBoxSymbolGlyph)0x2007,
            _ => throw new NotSupportedException(),
        };
    }
}
