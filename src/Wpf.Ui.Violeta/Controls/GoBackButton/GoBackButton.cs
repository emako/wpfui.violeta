using System;
using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A back button with the TitleBar chrome-back glyph and RightToLeft press scale animation.
/// Framework matches <see cref="CopyButton"/> (Icon / Content / ControlHelper icon appearance).
/// </summary>
/// <remarks>
/// Icon appearance uses <see cref="ControlHelper.IconFontFamilyProperty"/> /
/// <see cref="ControlHelper.IconFontSizeProperty"/> /
/// <see cref="ControlHelper.IconWidthProperty"/>. Values applied to
/// <see cref="Wpf.Ui.Controls.Button.Icon"/> only when the icon has no local
/// <c>FontFamily</c> / <c>FontSize</c> / <c>Width</c> — so an explicit value on
/// <c>SymbolIcon</c> / <c>FontIcon</c> wins over the attached properties.
/// </remarks>
/// <example>
/// <code lang="xml">
/// &lt;vio:GoBackButton Command="{Binding GoBackCommand}" /&gt;
/// &lt;vio:GoBackButton Content="Back" /&gt;
/// </code>
/// </example>
public class GoBackButton : Wpf.Ui.Controls.Button
{
    private bool _iconAppearanceHooked;

    /// <summary>Identifies the <see cref="BackGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty BackGlyphProperty =
        DependencyProperty.Register(
            nameof(BackGlyph),
            typeof(string),
            typeof(GoBackButton),
            new PropertyMetadata("\uE72B"));

    /// <summary>
    /// Gets or sets the Segoe Fluent Icons glyph used as the built-in back icon.
    /// Defaults to ChromeBack (<c>E72B</c>), matching <see cref="TitleBar"/> PART_BackButton.
    /// </summary>
    public string BackGlyph
    {
        get => (string)GetValue(BackGlyphProperty);
        set => SetValue(BackGlyphProperty, value);
    }

    static GoBackButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GoBackButton),
            new FrameworkPropertyMetadata(typeof(GoBackButton)));

        IconProperty.OverrideMetadata(
            typeof(GoBackButton),
            new FrameworkPropertyMetadata(null, OnIconChanged));
    }

    public GoBackButton()
    {
        Loaded += OnLoaded;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HookIconAppearanceProperties();
        SyncIconAppearance();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        HookIconAppearanceProperties();
        SyncIconAppearance();
    }

    private void HookIconAppearanceProperties()
    {
        if (_iconAppearanceHooked)
        {
            return;
        }

        _iconAppearanceHooked = true;
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconFontFamilyProperty, typeof(GoBackButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconFontSizeProperty, typeof(GoBackButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconWidthProperty, typeof(GoBackButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((GoBackButton)d).SyncIconAppearance();
    }

    private void OnIconAppearanceAttachedPropertyChanged(object? sender, EventArgs e)
    {
        SyncIconAppearance();
    }

    /// <summary>
    /// Pushes <see cref="ControlHelper"/> icon appearance onto <see cref="Icon"/> when the
    /// icon has no local FontFamily / FontSize / Width (explicit Icon values always win).
    /// </summary>
    private void SyncIconAppearance()
    {
        if (Icon is not FontIcon fontIcon)
        {
            return;
        }

        var family = ControlHelper.GetIconFontFamily(this);
        if (family is not null
            && fontIcon.ReadLocalValue(FontIcon.FontFamilyProperty) == DependencyProperty.UnsetValue)
        {
            fontIcon.SetCurrentValue(FontIcon.FontFamilyProperty, family);
        }

        var size = ControlHelper.GetIconFontSize(this);
        if (!double.IsNaN(size)
            && fontIcon.ReadLocalValue(FontIcon.FontSizeProperty) == DependencyProperty.UnsetValue)
        {
            fontIcon.SetCurrentValue(FontIcon.FontSizeProperty, size);
        }

        var width = ControlHelper.GetIconWidth(this);
        if (!double.IsNaN(width)
            && fontIcon.ReadLocalValue(FrameworkElement.WidthProperty) == DependencyProperty.UnsetValue)
        {
            fontIcon.SetCurrentValue(FrameworkElement.WidthProperty, width);
            if (fontIcon.ReadLocalValue(FrameworkElement.HorizontalAlignmentProperty)
                == DependencyProperty.UnsetValue)
            {
                fontIcon.SetCurrentValue(
                    FrameworkElement.HorizontalAlignmentProperty,
                    HorizontalAlignment.Center);
            }
        }
    }
}
