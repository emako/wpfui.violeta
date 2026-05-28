using System;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class BoolStateTextBlock : TextBlock
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(bool),
        typeof(BoolStateTextBlock),
        new PropertyMetadata(false, OnDisplayStateChanged));

    public static readonly DependencyProperty TrueTextProperty = DependencyProperty.Register(
        nameof(TrueText),
        typeof(string),
        typeof(BoolStateTextBlock),
        new PropertyMetadata("True", OnDisplayStateChanged));

    public static readonly DependencyProperty FalseTextProperty = DependencyProperty.Register(
        nameof(FalseText),
        typeof(string),
        typeof(BoolStateTextBlock),
        new PropertyMetadata("False", OnDisplayStateChanged));

    public static readonly DependencyProperty FontOptionsProperty = DependencyProperty.Register(
        nameof(FontOptions),
        typeof(BoolStateTextBlockFontOptions),
        typeof(BoolStateTextBlock),
        new PropertyMetadata(null, OnFontOptionsChanged));

    public bool Value
    {
        get => (bool)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string TrueText
    {
        get => (string)GetValue(TrueTextProperty);
        set => SetValue(TrueTextProperty, value);
    }

    public string FalseText
    {
        get => (string)GetValue(FalseTextProperty);
        set => SetValue(FalseTextProperty, value);
    }

    public BoolStateTextBlockFontOptions? FontOptions
    {
        get => (BoolStateTextBlockFontOptions?)GetValue(FontOptionsProperty);
        set => SetValue(FontOptionsProperty, value);
    }

    public BoolStateTextBlock()
    {
        UpdateDisplayText();
        ApplyFontOptions();
    }

    private static void OnDisplayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BoolStateTextBlock boolStateTextBlock)
        {
            boolStateTextBlock.UpdateDisplayText();
        }
    }

    private static void OnFontOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BoolStateTextBlock boolStateTextBlock)
        {
            return;
        }

        if (e.OldValue is BoolStateTextBlockFontOptions oldOptions)
        {
            WeakEventManager<Freezable, EventArgs>.RemoveHandler(oldOptions, nameof(Freezable.Changed), boolStateTextBlock.OnFontOptionsInstanceChanged);
        }

        if (e.NewValue is BoolStateTextBlockFontOptions newOptions)
        {
            WeakEventManager<Freezable, EventArgs>.AddHandler(newOptions, nameof(Freezable.Changed), boolStateTextBlock.OnFontOptionsInstanceChanged);
        }

        boolStateTextBlock.ApplyFontOptions();
    }

    private void OnFontOptionsInstanceChanged(object? sender, EventArgs e)
    {
        ApplyFontOptions();
    }

    private void UpdateDisplayText()
    {
        Text = Value ? TrueText : FalseText;
    }

    private void ApplyFontOptions()
    {
        if (FontOptions is null)
        {
            return;
        }

        SetCurrentValue(FontFamilyProperty, FontOptions.FontFamily);
        SetCurrentValue(FontSizeProperty, FontOptions.FontSize);
        SetCurrentValue(FontWeightProperty, FontOptions.FontWeight);
        SetCurrentValue(FontStyleProperty, FontOptions.FontStyle);
        SetCurrentValue(FontStretchProperty, FontOptions.FontStretch);
        SetCurrentValue(ForegroundProperty, FontOptions.Foreground);
        SetCurrentValue(LineHeightProperty, FontOptions.LineHeight);
        SetCurrentValue(TextDecorationsProperty, FontOptions.TextDecorations);
    }
}
