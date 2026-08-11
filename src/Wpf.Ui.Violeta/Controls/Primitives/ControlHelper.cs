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
/// Attached properties that extend the stock WPF controls.
/// </summary>
/// <remarks>
/// <see cref="PlaceholderTextProperty"/> reuses <see cref="TextBox.PlaceholderTextProperty"/> via
/// <see cref="DependencyProperty.AddOwner(System.Type)"/>, so values are stored on the same DP.
/// <see cref="Wpf.Ui.Controls.ComboBoxHelper"/> aliases this property, so either helper can be used in XAML.
/// <see cref="IconFontFamilyProperty"/> lets a <see cref="ContextMenu"/>, <see cref="Menu"/> or
/// <see cref="MenuItem"/> define a font for the glyphs inside its <see cref="MenuItem.Icon"/>
/// independently of the menu text. Setting it to <see langword="null"/> restores the icon's
/// original fonts.
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
            new PropertyMetadata(null, OnIconFontFamilyChanged));

    /// <summary>
    /// Gets the font family applied to the glyphs inside <see cref="MenuItem.Icon"/>.
    /// </summary>
    public static FontFamily? GetIconFontFamily(DependencyObject element) =>
        (FontFamily?)element.GetValue(IconFontFamilyProperty);

    /// <summary>
    /// Sets the font family applied to the glyphs inside <see cref="MenuItem.Icon"/>.
    /// </summary>
    public static void SetIconFontFamily(DependencyObject element, FontFamily? value) =>
        element.SetValue(IconFontFamilyProperty, value);

    /// <summary>
    /// Stores the effective font for an <see cref="ItemsControl"/>, propagated explicitly
    /// because submenus live in popups that do not participate in property inheritance.
    /// </summary>
    private static readonly DependencyProperty EffectiveFontProperty =
        DependencyProperty.RegisterAttached(
            "IconFontFamily_Effective",
            typeof(FontFamily),
            typeof(ControlHelper),
            new PropertyMetadata(null));

    /// <summary>
    /// Marks a container whose events (container generation, Loaded, Icon changes) are already hooked.
    /// </summary>
    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IconFontFamily_IsHooked",
            typeof(bool),
            typeof(ControlHelper),
            new PropertyMetadata(false));

    /// <summary>
    /// Original fonts of the icon text blocks, so that clearing <see cref="IconFontFamilyProperty"/>
    /// can restore them.
    /// </summary>
    [SuppressMessage("Style", "IDE0028:Simplify collection initialization")]
    private static readonly ConditionalWeakTable<TextBlock, FontFamily> OriginalFonts = new();

    private static void OnIconFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ItemsControl container)
        {
            return;
        }

        var font = (FontFamily?)e.NewValue;
        container.SetValue(EffectiveFontProperty, font);
        HookContainer(container);
        ApplyToContainer(container, font);
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
            ApplyToContainer(container, (FontFamily?)container.GetValue(EffectiveFontProperty));
        }
    }

    private static void OnMenuItemLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            ApplyIconFont(menuItem.Icon, (FontFamily?)menuItem.GetValue(EffectiveFontProperty));
        }
    }

    private static void OnMenuItemIconChanged(object? sender, EventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            ApplyIconFont(menuItem.Icon, (FontFamily?)menuItem.GetValue(EffectiveFontProperty));
        }
    }

    private static void ApplyToContainer(ItemsControl container, FontFamily? font)
    {
        for (var i = 0; i < container.Items.Count; i++)
        {
            if (container.ItemContainerGenerator.ContainerFromIndex(i) is MenuItem menuItem)
            {
                ApplyToMenuItem(menuItem, font);
            }
        }
    }

    private static void ApplyToMenuItem(MenuItem menuItem, FontFamily? font)
    {
        menuItem.SetValue(EffectiveFontProperty, font);
        HookContainer(menuItem);
        ApplyIconFont(menuItem.Icon, font);
        ApplyToContainer(menuItem, font);
    }

    private static void ApplyIconFont(object? icon, FontFamily? font)
    {
        if (icon is not DependencyObject root)
        {
            return;
        }

        foreach (var textBlock in EnumerateTextBlocks(root))
        {
            if (font is null)
            {
                if (OriginalFonts.TryGetValue(textBlock, out var original))
                {
                    textBlock.FontFamily = original;
                }

                OriginalFonts.Remove(textBlock);
            }
            else
            {
                if (!OriginalFonts.TryGetValue(textBlock, out _))
                {
                    OriginalFonts.Add(textBlock, textBlock.FontFamily);
                }

                textBlock.FontFamily = font;
            }
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
