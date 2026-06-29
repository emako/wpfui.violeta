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
        new PropertyMetadata(bool.TrueString, OnDisplayStateChanged));

    public static readonly DependencyProperty FalseTextProperty = DependencyProperty.Register(
        nameof(FalseText),
        typeof(string),
        typeof(BoolStateTextBlock),
        new PropertyMetadata(bool.FalseString, OnDisplayStateChanged));

    public static readonly DependencyProperty FontOptionsProperty = DependencyProperty.Register(
        nameof(FontOptions),
        typeof(BoolStateTextBlockFontOptions),
        typeof(BoolStateTextBlock),
        new PropertyMetadata(null, OnFontOptionsChanged));

    public static readonly DependencyProperty TrueFontOptionsProperty = DependencyProperty.Register(
        nameof(TrueFontOptions),
        typeof(BoolStateTextBlockFontOptions),
        typeof(BoolStateTextBlock),
        new PropertyMetadata(null, OnStateFontOptionsChanged));

    public static readonly DependencyProperty FalseFontOptionsProperty = DependencyProperty.Register(
        nameof(FalseFontOptions),
        typeof(BoolStateTextBlockFontOptions),
        typeof(BoolStateTextBlock),
        new PropertyMetadata(null, OnStateFontOptionsChanged));

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

    public BoolStateTextBlockFontOptions? TrueFontOptions
    {
        get => (BoolStateTextBlockFontOptions?)GetValue(TrueFontOptionsProperty);
        set => SetValue(TrueFontOptionsProperty, value);
    }

    public BoolStateTextBlockFontOptions? FalseFontOptions
    {
        get => (BoolStateTextBlockFontOptions?)GetValue(FalseFontOptionsProperty);
        set => SetValue(FalseFontOptionsProperty, value);
    }

    public BoolStateTextBlock()
    {
        UpdateVisualState();
    }

    private static void OnDisplayStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BoolStateTextBlock boolStateTextBlock)
        {
            boolStateTextBlock.UpdateVisualState();
        }
    }

    private static void OnFontOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BoolStateTextBlock boolStateTextBlock)
        {
            return;
        }

        UpdateFontOptionsSubscription(boolStateTextBlock, e);
        boolStateTextBlock.ApplyFontOptions();
    }

    private static void OnStateFontOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BoolStateTextBlock boolStateTextBlock)
        {
            return;
        }

        UpdateFontOptionsSubscription(boolStateTextBlock, e);
        boolStateTextBlock.ApplyFontOptions();
    }

    private static void UpdateFontOptionsSubscription(BoolStateTextBlock boolStateTextBlock, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BoolStateTextBlockFontOptions oldOptions)
        {
            WeakEventManager<Freezable, EventArgs>.RemoveHandler(oldOptions, nameof(Freezable.Changed), boolStateTextBlock.OnFontOptionsInstanceChanged);
        }

        if (e.NewValue is BoolStateTextBlockFontOptions newOptions)
        {
            WeakEventManager<Freezable, EventArgs>.AddHandler(newOptions, nameof(Freezable.Changed), boolStateTextBlock.OnFontOptionsInstanceChanged);
        }
    }

    private void OnFontOptionsInstanceChanged(object? sender, EventArgs e)
    {
        ApplyFontOptions();
    }

    private void UpdateDisplayText()
    {
        Text = Value ? TrueText : FalseText;
    }

    private void UpdateVisualState()
    {
        UpdateDisplayText();
        ApplyFontOptions();
    }

    private void ApplyFontOptions()
    {
        BoolStateTextBlockFontOptions? activeFontOptions = Value
            ? TrueFontOptions ?? FontOptions
            : FalseFontOptions ?? FontOptions;

        if (activeFontOptions is null)
        {
            return;
        }

        SetValue(FontFamilyProperty, activeFontOptions.FontFamily);
        SetValue(FontSizeProperty, activeFontOptions.FontSize);
        SetValue(FontWeightProperty, activeFontOptions.FontWeight);
        SetValue(FontStyleProperty, activeFontOptions.FontStyle);
        SetValue(FontStretchProperty, activeFontOptions.FontStretch);
        SetValue(ForegroundProperty, activeFontOptions.Foreground);
        SetValue(LineHeightProperty, activeFontOptions.LineHeight);
        SetValue(TextDecorationsProperty, activeFontOptions.TextDecorations);
    }
}
