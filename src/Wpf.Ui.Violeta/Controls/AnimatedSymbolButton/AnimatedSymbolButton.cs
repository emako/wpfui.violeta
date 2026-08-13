using System;
using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A button that plays a WinUI AnimatedVisuals–aligned symbol animation selected by
/// <see cref="Kind"/>. Layout matches <see cref="CopyButton"/> (icon host + optional Content).
/// </summary>
/// <remarks>
/// Kind names follow <c>Microsoft.UI.Xaml.Controls.AnimatedVisuals</c>
/// (<see cref="AnimatedSymbolKind"/>). Per-kind motion lives in
/// <c>AnimatedSymbolButton.&lt;Kind&gt;.cs</c> / <c>.xaml</c> partials.
/// </remarks>
/// <example>
/// <code lang="xml">
/// &lt;vio:AnimatedSymbolButton Kind="Back" /&gt;
/// &lt;vio:AnimatedSymbolButton Kind="Settings" Content="Settings" /&gt;
/// &lt;vio:AnimatedSymbolButton Kind="CopyToClipboard" TextToCopy="payload" /&gt;
/// </code>
/// </example>
[TemplatePart(Name = RootGridPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = ClipHostPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = AnimatedVisualPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = SuccessGlyphPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = DefaultGlyphPart, Type = typeof(FrameworkElement))]
public partial class AnimatedSymbolButton : Wpf.Ui.Controls.Button
{
    internal const string RootGridPart = "PART_RootGrid";
    internal const string ClipHostPart = "PART_ClipHost";
    internal const string AnimatedVisualPart = "PART_AnimatedVisual";
    internal const string SuccessGlyphPart = "PART_SuccessGlyph";
    internal const string DefaultGlyphPart = "PART_DefaultGlyph";

    private AnimatedSymbolBehavior? _behavior;
    private bool _iconAppearanceHooked;

    #region Dependency properties

    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(AnimatedSymbolKind),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(AnimatedSymbolKind.Back, OnKindChanged));

    /// <summary>
    /// Gets or sets which AnimatedVisuals-aligned animation to play.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the expanded state used by <see cref="AnimatedSymbolKind.ChevronUpDownSmall"/>
    /// (0° ↔ 180°). Other kinds ignore this property.
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether a click toggles <see cref="IsExpanded"/> when
    /// <see cref="Kind"/> is <see cref="AnimatedSymbolKind.ChevronUpDownSmall"/>.
    /// </summary>
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

    /// <summary>
    /// Gets or sets clipboard text for <see cref="AnimatedSymbolKind.CopyToClipboard"/>.
    /// </summary>
    public string? TextToCopy
    {
        get => (string?)GetValue(TextToCopyProperty);
        set => SetValue(TextToCopyProperty, value);
    }

    public static readonly DependencyProperty SuccessGlyphProperty = DependencyProperty.Register(
        nameof(SuccessGlyph),
        typeof(string),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata("\uE73E"));

    /// <summary>
    /// Gets or sets the Accept glyph shown by <see cref="AnimatedSymbolKind.CopyToClipboard"/>.
    /// Defaults to <c>E73E</c>.
    /// </summary>
    public string SuccessGlyph
    {
        get => (string)GetValue(SuccessGlyphProperty);
        set => SetValue(SuccessGlyphProperty, value);
    }

    public static readonly DependencyProperty SymbolGlyphProperty = DependencyProperty.Register(
        nameof(SymbolGlyph),
        typeof(string),
        typeof(AnimatedSymbolButton),
        new PropertyMetadata(null, OnSymbolGlyphChanged));

    /// <summary>
    /// Gets or sets an optional override for the built-in Segoe Fluent Icons glyph.
    /// When null, the glyph comes from <see cref="Kind"/>.
    /// </summary>
    public string? SymbolGlyph
    {
        get => (string?)GetValue(SymbolGlyphProperty);
        set => SetValue(SymbolGlyphProperty, value);
    }

    private static readonly DependencyPropertyKey ResolvedSymbolGlyphPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ResolvedSymbolGlyph),
            typeof(string),
            typeof(AnimatedSymbolButton),
            new PropertyMetadata("\uE72B"));

    public static readonly DependencyProperty ResolvedSymbolGlyphProperty =
        ResolvedSymbolGlyphPropertyKey.DependencyProperty;

    /// <summary>Gets the glyph actually shown when <see cref="Icon"/> is unset.</summary>
    public string ResolvedSymbolGlyph => (string)GetValue(ResolvedSymbolGlyphProperty);

    #endregion

    static AnimatedSymbolButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AnimatedSymbolButton),
            new FrameworkPropertyMetadata(typeof(AnimatedSymbolButton)));

        IconProperty.OverrideMetadata(
            typeof(AnimatedSymbolButton),
            new FrameworkPropertyMetadata(null, OnIconChanged));
    }

    public AnimatedSymbolButton()
    {
        Loaded += OnLoaded;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        HookIconAppearanceProperties();
        SyncIconAppearance();
        EnsureBehavior();
        _behavior?.OnTemplateApplied();
        UpdateResolvedGlyph();
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
        var button = (AnimatedSymbolButton)d;
        button._behavior?.NotifyExpandedChanged((bool)e.NewValue);
    }

    private static void OnSymbolGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AnimatedSymbolButton)d).UpdateResolvedGlyph();
    }

    private void OnKindChanged()
    {
        EnsureBehavior(forceRecreate: true);
        UpdateResolvedGlyph();
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

    private void UpdateResolvedGlyph()
    {
        var glyph = !string.IsNullOrEmpty(SymbolGlyph)
            ? SymbolGlyph!
            : _behavior?.DefaultGlyph ?? "\uE72B";
        SetValue(ResolvedSymbolGlyphPropertyKey, glyph);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        HookIconAppearanceProperties();
        SyncIconAppearance();
        EnsureBehavior();
        UpdateResolvedGlyph();
    }

    private void HookIconAppearanceProperties()
    {
        if (_iconAppearanceHooked)
        {
            return;
        }

        _iconAppearanceHooked = true;
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconFontFamilyProperty, typeof(AnimatedSymbolButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconFontSizeProperty, typeof(AnimatedSymbolButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
        DependencyPropertyDescriptor.FromProperty(ControlHelper.IconWidthProperty, typeof(AnimatedSymbolButton))
            .AddValueChanged(this, OnIconAppearanceAttachedPropertyChanged);
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((AnimatedSymbolButton)d).SyncIconAppearance();
    }

    private void OnIconAppearanceAttachedPropertyChanged(object? sender, EventArgs e) => SyncIconAppearance();

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
