using System;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button that plays a WinUI AnimatedVisuals–aligned symbol animation selected by
/// <see cref="Kind"/>. Layout matches <see cref="CopyButton"/> (symbol host + optional Content).
/// </summary>
/// <remarks>
/// Primary / secondary icon appearance:
/// <see cref="IconFontFamily"/>, <see cref="IconFontSize"/>, <see cref="IconGlyph"/> and
/// <see cref="SecondaryIconFontFamily"/>, <see cref="SecondaryIconFontSize"/>, <see cref="SecondaryIconGlyph"/>
/// (secondary is used by two-glyph kinds such as <see cref="AnimatedSymbolKind.CopyToClipboard"/>).
/// </remarks>
[TemplatePart(Name = RootGridPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = ClipHostPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = AnimatedVisualPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = SuccessGlyphPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = DefaultGlyphPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = IconHostPart, Type = typeof(FrameworkElement))]
public partial class AnimatedSymbolButton : Wpf.Ui.Controls.Button
{
    internal const string RootGridPart = "PART_RootGrid";
    internal const string ClipHostPart = "PART_ClipHost";
    internal const string AnimatedVisualPart = "PART_AnimatedVisual";
    internal const string SuccessGlyphPart = "PART_SuccessGlyph";
    internal const string DefaultGlyphPart = "PART_DefaultGlyph";
    internal const string IconHostPart = "PART_IconHost";

    private AnimatedSymbolBehavior? _behavior;

    #region Kind / expand

    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(AnimatedSymbolKind),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(AnimatedSymbolKind.Back, OnKindChanged));

    public AnimatedSymbolKind Kind
    {
        get => (AnimatedSymbolKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded),
        typeof(bool),
        typeof(AnimatedSymbolButton),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnIsExpandedChanged));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty ToggleExpandedOnClickProperty = DependencyProperty.Register(
        nameof(ToggleExpandedOnClick),
        typeof(bool),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(true));

    public bool ToggleExpandedOnClick
    {
        get => (bool)GetValue(ToggleExpandedOnClickProperty);
        set => SetValue(ToggleExpandedOnClickProperty, value);
    }

    public static readonly DependencyProperty TextToCopyProperty = DependencyProperty.Register(
        nameof(TextToCopy),
        typeof(string),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(null));

    public string? TextToCopy
    {
        get => (string?)GetValue(TextToCopyProperty);
        set => SetValue(TextToCopyProperty, value);
    }

    #endregion

    #region Primary icon (FontFamily / FontSize / Glyph)

    public static readonly DependencyProperty IconFontFamilyProperty = DependencyProperty.Register(
        nameof(IconFontFamily),
        typeof(FontFamily),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(null, OnPrimaryIconChanged));

    /// <summary>Primary symbol font. Null → Kind default (<c>SymbolThemeFontFamily</c>).</summary>
    public FontFamily? IconFontFamily
    {
        get => (FontFamily?)GetValue(IconFontFamilyProperty);
        set => SetValue(IconFontFamilyProperty, value);
    }

    public static readonly DependencyProperty IconFontSizeProperty = DependencyProperty.Register(
        nameof(IconFontSize),
        typeof(double),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(double.NaN, OnPrimaryIconChanged));

    /// <summary>Primary symbol size. <see cref="double.NaN"/> → 16.</summary>
    public double IconFontSize
    {
        get => (double)GetValue(IconFontSizeProperty);
        set => SetValue(IconFontSizeProperty, value);
    }

    public static readonly DependencyProperty IconGlyphProperty = DependencyProperty.Register(
        nameof(IconGlyph),
        typeof(string),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(null, OnPrimaryIconChanged));

    /// <summary>Primary glyph override. Null → Kind default.</summary>
    public string? IconGlyph
    {
        get => (string?)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    private static readonly DependencyPropertyKey ResolvedIconFontFamilyPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ResolvedIconFontFamily),
            typeof(FontFamily),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ResolvedIconFontFamilyProperty =
        ResolvedIconFontFamilyPropertyKey.DependencyProperty;

    public FontFamily? ResolvedIconFontFamily => (FontFamily?)GetValue(ResolvedIconFontFamilyProperty);

    private static readonly DependencyPropertyKey ResolvedIconFontSizePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ResolvedIconFontSize),
            typeof(double),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata(16.0));

    public static readonly DependencyProperty ResolvedIconFontSizeProperty =
        ResolvedIconFontSizePropertyKey.DependencyProperty;

    public double ResolvedIconFontSize => (double)GetValue(ResolvedIconFontSizeProperty);

    private static readonly DependencyPropertyKey ResolvedIconGlyphPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ResolvedIconGlyph),
            typeof(string),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata("\uE72B"));

    public static readonly DependencyProperty ResolvedIconGlyphProperty =
        ResolvedIconGlyphPropertyKey.DependencyProperty;

    public string ResolvedIconGlyph => (string)GetValue(ResolvedIconGlyphProperty);

    #endregion

    #region Secondary icon (two-glyph kinds, e.g. CopyToClipboard)

    public static readonly DependencyProperty SecondaryIconFontFamilyProperty = DependencyProperty.Register(
        nameof(SecondaryIconFontFamily),
        typeof(FontFamily),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(null, OnSecondaryIconChanged));

    public FontFamily? SecondaryIconFontFamily
    {
        get => (FontFamily?)GetValue(SecondaryIconFontFamilyProperty);
        set => SetValue(SecondaryIconFontFamilyProperty, value);
    }

    public static readonly DependencyProperty SecondaryIconFontSizeProperty = DependencyProperty.Register(
        nameof(SecondaryIconFontSize),
        typeof(double),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(double.NaN, OnSecondaryIconChanged));

    public double SecondaryIconFontSize
    {
        get => (double)GetValue(SecondaryIconFontSizeProperty);
        set => SetValue(SecondaryIconFontSizeProperty, value);
    }

    public static readonly DependencyProperty SecondaryIconGlyphProperty = DependencyProperty.Register(
        nameof(SecondaryIconGlyph),
        typeof(string),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(null, OnSecondaryIconChanged));

    /// <summary>
    /// Secondary glyph (e.g. Accept for <see cref="AnimatedSymbolKind.CopyToClipboard"/>).
    /// Null → Kind default.
    /// </summary>
    public string? SecondaryIconGlyph
    {
        get => (string?)GetValue(SecondaryIconGlyphProperty);
        set => SetValue(SecondaryIconGlyphProperty, value);
    }

    private static readonly DependencyPropertyKey ResolvedSecondaryIconFontFamilyPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ResolvedSecondaryIconFontFamily),
            typeof(FontFamily),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ResolvedSecondaryIconFontFamilyProperty =
        ResolvedSecondaryIconFontFamilyPropertyKey.DependencyProperty;

    public FontFamily? ResolvedSecondaryIconFontFamily =>
        (FontFamily?)GetValue(ResolvedSecondaryIconFontFamilyProperty);

    private static readonly DependencyPropertyKey ResolvedSecondaryIconFontSizePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ResolvedSecondaryIconFontSize),
            typeof(double),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata(16.0));

    public static readonly DependencyProperty ResolvedSecondaryIconFontSizeProperty =
        ResolvedSecondaryIconFontSizePropertyKey.DependencyProperty;

    public double ResolvedSecondaryIconFontSize => (double)GetValue(ResolvedSecondaryIconFontSizeProperty);

    private static readonly DependencyPropertyKey ResolvedSecondaryIconGlyphPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ResolvedSecondaryIconGlyph),
            typeof(string),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata("\uE73E"));

    public static readonly DependencyProperty ResolvedSecondaryIconGlyphProperty =
        ResolvedSecondaryIconGlyphPropertyKey.DependencyProperty;

    public string ResolvedSecondaryIconGlyph => (string)GetValue(ResolvedSecondaryIconGlyphProperty);

    #endregion

    #region Layout

    private static readonly DependencyPropertyKey IsSymbolTrailingPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsSymbolTrailing),
            typeof(bool),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsSymbolTrailingProperty =
        IsSymbolTrailingPropertyKey.DependencyProperty;

    /// <summary>
    /// True for chevron kinds — symbol is placed after Content (DropDownButton / ComboBox layout).
    /// </summary>
    public bool IsSymbolTrailing => (bool)GetValue(IsSymbolTrailingProperty);

    #endregion

    static AnimatedSymbolButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AnimatedSymbolButton),
            new FrameworkPropertyMetadata(typeof(AnimatedSymbolButton)));
    }

    public AnimatedSymbolButton()
    {
        Loaded += OnLoaded;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        EnsureBehavior();
        _behavior?.OnTemplateApplied();
        UpdateResolvedIconAppearance();
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (_behavior is CopyToClipboardBehavior { IsAnimating: true })
        {
            return;
        }

        if (Kind == AnimatedSymbolKind.CopyToClipboard && !string.IsNullOrEmpty(TextToCopy))
        {
            Win32.Clipboard.SetText(TextToCopy!);
        }

        if (Kind == AnimatedSymbolKind.ChevronUpDownSmall && ToggleExpandedOnClick)
        {
            SetCurrentValue(IsExpandedProperty, !IsExpanded);
        }

        _behavior?.OnClick();
        base.OnClick();
    }

    internal DependencyObject? GetPart(string name) => GetTemplateChild(name);

    private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AnimatedSymbolButton)d).OnKindChanged();
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AnimatedSymbolButton)d)._behavior?.NotifyExpandedChanged((bool)e.NewValue);
    }

    private static void OnPrimaryIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AnimatedSymbolButton)d).UpdateResolvedIconAppearance();
    }

    private static void OnSecondaryIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AnimatedSymbolButton)d).UpdateResolvedIconAppearance();
    }

    private void OnKindChanged()
    {
        EnsureBehavior(forceRecreate: true);
        UpdateResolvedIconAppearance();
    }

    private void EnsureBehavior(bool forceRecreate = false)
    {
        if (!forceRecreate && _behavior is not null)
        {
            return;
        }

        _behavior?.Detach();
        _behavior = AnimatedSymbolBehavior.Create(Kind);
        _behavior?.Attach(this);
    }

    private void UpdateResolvedIconAppearance()
    {
        var defaults = AnimatedSymbolDefaults.For(Kind);

        var primaryFamily = IconFontFamily
            ?? TryFindResource("SymbolThemeFontFamily") as FontFamily
            ?? defaults.FontFamily;
        var primarySize = !double.IsNaN(IconFontSize) ? IconFontSize : defaults.FontSize;
        var primaryGlyph = !string.IsNullOrEmpty(IconGlyph) ? IconGlyph! : defaults.Glyph;

        SetValue(ResolvedIconFontFamilyPropertyKey, primaryFamily);
        SetValue(ResolvedIconFontSizePropertyKey, primarySize);
        SetValue(ResolvedIconGlyphPropertyKey, primaryGlyph);

        var secondaryFamily = SecondaryIconFontFamily ?? primaryFamily;
        var secondarySize = !double.IsNaN(SecondaryIconFontSize) ? SecondaryIconFontSize : primarySize;
        var secondaryGlyph = !string.IsNullOrEmpty(SecondaryIconGlyph)
            ? SecondaryIconGlyph!
            : defaults.SecondaryGlyph;

        SetValue(ResolvedSecondaryIconFontFamilyPropertyKey, secondaryFamily);
        SetValue(ResolvedSecondaryIconFontSizePropertyKey, secondarySize);
        SetValue(ResolvedSecondaryIconGlyphPropertyKey, secondaryGlyph);

        var trailing = Kind is AnimatedSymbolKind.ChevronDownSmall or AnimatedSymbolKind.ChevronUpDownSmall;
        SetValue(IsSymbolTrailingPropertyKey, trailing);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        EnsureBehavior();
        UpdateResolvedIconAppearance();
    }
}

/// <summary>Per-<see cref="AnimatedSymbolKind"/> default glyph / size (Segoe Fluent Icons).</summary>
internal readonly record struct AnimatedSymbolDefaults(
    string Glyph,
    double FontSize,
    string SecondaryGlyph,
    FontFamily? FontFamily = null)
{
    public static AnimatedSymbolDefaults For(AnimatedSymbolKind kind) =>
        kind switch
        {
            // Segoe Fluent Icons: GlobalNavButton (not Fluent System Icons F4E1 / LineHorizontal3)
            AnimatedSymbolKind.GlobalNavigationButton => new("\uE700", 16, "\uE700"),
            AnimatedSymbolKind.Settings => new("\uE713", 16, "\uE713"),
            AnimatedSymbolKind.ChevronDownSmall => new("\uE70D", 10, "\uE70D"),
            AnimatedSymbolKind.ChevronUpDownSmall => new("\uE70D", 10, "\uE70D"),
            AnimatedSymbolKind.CopyToClipboard => new("\uE8C8", 16, "\uE73E"),
            AnimatedSymbolKind.Back => new("\uE72B", 16, "\uE72B"),
            _ => new("\uE72B", 16, "\uE72B"),
        };
}
