using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.Ui.Controls;

/// <summary>
/// Provides attached properties that extend the built-in WPF controls.
/// </summary>
/// <remarks>
/// <see cref="PlaceholderTextProperty"/> reuses <see cref="TextBox.PlaceholderTextProperty"/> by
/// calling <see cref="DependencyProperty.AddOwner(System.Type)"/>, so both properties share the
/// same backing storage.
/// <see cref="IconFontFamilyProperty"/> / <see cref="IconFontSizeProperty"/> allow a
/// <see cref="ContextMenu"/>, <see cref="Menu"/>, <see cref="MenuItem"/>, or other control
/// (for example <c>CopyButton</c>) to style icon glyphs independently of the host text.
/// Clearing them restores the icons' original fonts / sizes.
/// </remarks>
public static class ControlHelper
{
    // -- PlaceholderText -----------------------------------------------------

    /// <summary>
    /// Identifies the PlaceholderText attached property.
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        TextBox.PlaceholderTextProperty.AddOwner(typeof(ControlHelper));

    /// <summary>
    /// Gets the placeholder text.
    /// </summary>
    public static string GetPlaceholderText(DependencyObject element) =>
        (string)element.GetValue(PlaceholderTextProperty);

    /// <summary>
    /// Sets the placeholder text.
    /// </summary>
    public static void SetPlaceholderText(DependencyObject element, string value) =>
        element.SetValue(PlaceholderTextProperty, value);

    // -- IconFontFamily ------------------------------------------------------

    /// <summary>
    /// Identifies the IconFontFamily attached property.
    /// </summary>
    public static readonly DependencyProperty IconFontFamilyProperty =
        DependencyProperty.RegisterAttached(
            "IconFontFamily",
            typeof(FontFamily),
            typeof(ControlHelper),
            new PropertyMetadata(null, OnIconAppearanceChanged));

    /// <summary>
    /// Gets the font family applied to icon glyphs.
    /// </summary>
    public static FontFamily? GetIconFontFamily(DependencyObject element) =>
        (FontFamily?)element.GetValue(IconFontFamilyProperty);

    /// <summary>
    /// Sets the font family applied to icon glyphs.
    /// </summary>
    public static void SetIconFontFamily(DependencyObject element, FontFamily? value) =>
        element.SetValue(IconFontFamilyProperty, value);

    // -- IconFontSize --------------------------------------------------------

    /// <summary>
    /// Identifies the IconFontSize attached property.
    /// Use <see cref="double.NaN"/> (default) to leave the icon size unchanged.
    /// </summary>
    public static readonly DependencyProperty IconFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "IconFontSize",
            typeof(double),
            typeof(ControlHelper),
            new PropertyMetadata(double.NaN, OnIconAppearanceChanged));

    /// <summary>
    /// Gets the font size applied to icon glyphs.
    /// </summary>
    public static double GetIconFontSize(DependencyObject element) =>
        (double)element.GetValue(IconFontSizeProperty);

    /// <summary>
    /// Sets the font size applied to icon glyphs.
    /// </summary>
    public static void SetIconFontSize(DependencyObject element, double value) =>
        element.SetValue(IconFontSizeProperty, value);

    /// <summary>
    /// Effective font propagated into submenu popups (inheritance does not cross Popup hosts).
    /// </summary>
    private static readonly DependencyProperty EffectiveFontFamilyProperty =
        DependencyProperty.RegisterAttached(
            "IconFontFamily_Effective",
            typeof(FontFamily),
            typeof(ControlHelper),
            new PropertyMetadata(null));

    /// <summary>
    /// Effective font size propagated into submenu popups.
    /// </summary>
    private static readonly DependencyProperty EffectiveFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "IconFontSize_Effective",
            typeof(double),
            typeof(ControlHelper),
            new PropertyMetadata(double.NaN));

    /// <summary>
    /// Marks a container whose generation / Loaded / Icon hooks are already attached.
    /// </summary>
    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IconAppearance_IsHooked",
            typeof(bool),
            typeof(ControlHelper),
            new PropertyMetadata(false));

    private sealed class OriginalIconStyle
    {
        public double FontSize { get; set; }
        public bool HasFontSize { get; set; }
        public bool OverrodeFontFamily { get; set; }
        public bool OverrodeFontSize { get; set; }
    }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization")]
    private static readonly ConditionalWeakTable<DependencyObject, OriginalIconStyle> OriginalStyles = new();

    private static void OnIconAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        _ = e;

        if (d is not ItemsControl container)
        {
            // Non-ItemsControl hosts (e.g. CopyButton) consume these APs via template bindings.
            return;
        }

        container.SetValue(EffectiveFontFamilyProperty, GetIconFontFamily(container));
        container.SetValue(EffectiveFontSizeProperty, GetIconFontSize(container));
        HookContainer(container);
        ApplyToContainer(container, GetIconFontFamily(container), GetIconFontSize(container));
    }

    private static void HookContainer(ItemsControl container)
    {
        if ((bool)container.GetValue(IsHookedProperty))
        {
            return;
        }

        container.SetValue(IsHookedProperty, true);
        container.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;

        if (container is MenuItem menuItem)
        {
            menuItem.Loaded += OnMenuItemLoaded;
            DependencyPropertyDescriptor.FromProperty(MenuItem.IconProperty, typeof(MenuItem))
                .AddValueChanged(menuItem, OnMenuItemIconChanged);
        }
    }

    private static void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (sender is ItemsControl container
            && container.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
        {
            ApplyToContainer(
                container,
                (FontFamily?)container.GetValue(EffectiveFontFamilyProperty),
                (double)container.GetValue(EffectiveFontSizeProperty));
        }
    }

    private static void OnMenuItemLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            ApplyIconAppearance(
                menuItem.Icon,
                (FontFamily?)menuItem.GetValue(EffectiveFontFamilyProperty),
                (double)menuItem.GetValue(EffectiveFontSizeProperty));
        }
    }

    private static void OnMenuItemIconChanged(object? sender, EventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            ApplyIconAppearance(
                menuItem.Icon,
                (FontFamily?)menuItem.GetValue(EffectiveFontFamilyProperty),
                (double)menuItem.GetValue(EffectiveFontSizeProperty));
        }
    }

    private static void ApplyToContainer(ItemsControl container, FontFamily? font, double fontSize)
    {
        for (var i = 0; i < container.Items.Count; i++)
        {
            if (container.ItemContainerGenerator.ContainerFromIndex(i) is MenuItem menuItem)
            {
                ApplyToMenuItem(menuItem, font, fontSize);
            }
        }
    }

    private static void ApplyToMenuItem(MenuItem menuItem, FontFamily? font, double fontSize)
    {
        // Prefer values set directly on the MenuItem; otherwise inherit from the parent container.
        var effectiveFont = GetIconFontFamily(menuItem) ?? font;
        var localSize = GetIconFontSize(menuItem);
        var effectiveSize = !double.IsNaN(localSize) ? localSize : fontSize;

        menuItem.SetValue(EffectiveFontFamilyProperty, effectiveFont);
        menuItem.SetValue(EffectiveFontSizeProperty, effectiveSize);
        HookContainer(menuItem);
        ApplyIconAppearance(menuItem.Icon, effectiveFont, effectiveSize);
        ApplyToContainer(menuItem, effectiveFont, effectiveSize);
    }

    private static void ApplyIconAppearance(object? icon, FontFamily? font, double fontSize)
    {
        if (icon is not DependencyObject root)
        {
            return;
        }

        ApplyToNode(root, font, fontSize);

        foreach (var textBlock in EnumerateTextBlocks(root))
        {
            ApplyToNode(textBlock, font, fontSize);
        }
    }

    private static void ApplyToNode(DependencyObject node, FontFamily? font, double fontSize)
    {
        if (node is not (FontIcon or TextBlock or Control))
        {
            return;
        }

        var original = OriginalStyles.GetValue(node, static d => CaptureOriginal(d));

        if (font is not null)
        {
            SetFontFamily(node, font);
            original.OverrodeFontFamily = true;
        }
        else if (original.OverrodeFontFamily)
        {
            ClearFontFamily(node);
            original.OverrodeFontFamily = false;
        }

        if (!double.IsNaN(fontSize))
        {
            if (!original.OverrodeFontSize)
            {
                original.FontSize = ReadFontSize(node);
                original.HasFontSize = true;
            }

            SetFontSize(node, fontSize);
            original.OverrodeFontSize = true;
        }
        else if (original.OverrodeFontSize && original.HasFontSize)
        {
            SetFontSize(node, original.FontSize);
            original.OverrodeFontSize = false;
        }
    }

    private static OriginalIconStyle CaptureOriginal(DependencyObject node) => new();

    private static void SetFontFamily(DependencyObject node, FontFamily font)
    {
        switch (node)
        {
            case FontIcon fontIcon:
                fontIcon.FontFamily = font;
                break;
            case TextBlock textBlock:
                textBlock.FontFamily = font;
                break;
            case Control control:
                control.FontFamily = font;
                break;
        }
    }

    private static void ClearFontFamily(DependencyObject node)
    {
        switch (node)
        {
            case FontIcon:
                node.ClearValue(FontIcon.FontFamilyProperty);
                break;
            case TextBlock:
                node.ClearValue(TextBlock.FontFamilyProperty);
                break;
            case Control:
                node.ClearValue(Control.FontFamilyProperty);
                break;
        }
    }

    private static double ReadFontSize(DependencyObject node) =>
        node switch
        {
            FontIcon fontIcon => fontIcon.FontSize,
            TextBlock textBlock => textBlock.FontSize,
            Control control => control.FontSize,
            _ => SystemFonts.MessageFontSize,
        };

    private static void SetFontSize(DependencyObject node, double fontSize)
    {
        switch (node)
        {
            case FontIcon fontIcon:
                fontIcon.FontSize = fontSize;
                break;
            case TextBlock textBlock:
                textBlock.FontSize = fontSize;
                break;
            case Control control:
                control.FontSize = fontSize;
                break;
        }
    }

    private static IEnumerable<TextBlock> EnumerateTextBlocks(DependencyObject root)
    {
        if (root is TextBlock textBlock)
        {
            yield return textBlock;
        }

        foreach (var child in LogicalTreeHelper.GetChildren(root))
        {
            if (child is DependencyObject dependencyObject)
            {
                foreach (var inner in EnumerateTextBlocks(dependencyObject))
                {
                    yield return inner;
                }
            }
        }
    }
}
