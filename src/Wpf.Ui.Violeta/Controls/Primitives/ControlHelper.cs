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
/// <see cref="IconFontFamilyProperty"/> allows a <see cref="ContextMenu"/>, <see cref="Menu"/> or
/// <see cref="MenuItem"/> to specify the font used for the glyphs inside its
/// <see cref="MenuItem.Icon"/>, independently of the menu text. Setting it to <see langword="null"/>
/// restores the icons' original fonts.
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
    /// Stores the effective font of an <see cref="ItemsControl"/>. The value is propagated
    /// manually because submenus are hosted in popups, where property inheritance does not work.
    /// </summary>
    private static readonly DependencyProperty EffectiveFontProperty =
        DependencyProperty.RegisterAttached(
            "IconFontFamily_Effective",
            typeof(FontFamily),
            typeof(ControlHelper),
            new PropertyMetadata(null));

    /// <summary>
    /// Marks a container whose events (container generation, Loaded, icon changes)
    /// have already been hooked up.
    /// </summary>
    private static readonly DependencyProperty IsHookedProperty =
        DependencyProperty.RegisterAttached(
            "IconFontFamily_IsHooked",
            typeof(bool),
            typeof(ControlHelper),
            new PropertyMetadata(false));

    /// <summary>
    /// Remembers the original font of every icon text block, so that clearing
    /// <see cref="IconFontFamilyProperty"/> can restore them.
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
