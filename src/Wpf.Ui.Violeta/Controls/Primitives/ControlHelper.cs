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
/// <para />
/// <see cref="IconFontFamilyProperty"/> / <see cref="IconFontSizeProperty"/> style
/// <see cref="MenuItem.Icon"/> glyphs on a <see cref="ContextMenu"/>, <see cref="Menu"/> or
/// <see cref="MenuItem"/> independently of the menu text. Set them on the menu root to affect
/// all items, or on a single <see cref="MenuItem"/> for that item (and its submenu).
/// An explicit <c>FontSize</c> / <c>FontFamily</c> on the icon element itself always wins.
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
    /// Marks a container whose generation / Loaded / Icon / Opened hooks are already attached.
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

        var family = GetIconFontFamily(d);
        var size = GetIconFontSize(d);

        // MenuItem: apply to THIS item's Icon first, then walk submenu children.
        if (d is MenuItem menuItem)
        {
            menuItem.SetValue(EffectiveFontFamilyProperty, family);
            menuItem.SetValue(EffectiveFontSizeProperty, size);
            HookContainer(menuItem);
            ApplyIconAppearance(menuItem.Icon, family, size);
            ApplyToContainer(menuItem, family, size);
            return;
        }

        // ContextMenu / Menu: push into generated item containers.
        if (d is ItemsControl container)
        {
            container.SetValue(EffectiveFontFamilyProperty, family);
            container.SetValue(EffectiveFontSizeProperty, size);
            HookContainer(container);
            ApplyToContainer(container, family, size);
        }
    }

    private static void HookContainer(ItemsControl container)
    {
        if ((bool)container.GetValue(IsHookedProperty))
        {
            return;
        }

        container.SetValue(IsHookedProperty, true);
        container.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        container.Loaded += OnContainerLoaded;

        if (container is ContextMenu contextMenu)
        {
            contextMenu.Opened += OnContextMenuOpened;
        }

        if (container is MenuItem menuItem)
        {
            DependencyPropertyDescriptor.FromProperty(MenuItem.IconProperty, typeof(MenuItem))
                .AddValueChanged(menuItem, OnMenuItemIconChanged);
            menuItem.SubmenuOpened += OnSubmenuOpened;
        }
    }

    private static void OnContainerLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl container)
        {
            RefreshContainer(container);
        }
    }

    private static void OnContextMenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu contextMenu)
        {
            RefreshContainer(contextMenu);
        }
    }

    private static void OnSubmenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            RefreshContainer(menuItem);
        }
    }

    private static void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (sender is ItemsControl container
            && container.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
        {
            RefreshContainer(container);
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

    private static void RefreshContainer(ItemsControl container)
    {
        ApplyToContainer(
            container,
            (FontFamily?)container.GetValue(EffectiveFontFamilyProperty),
            (double)container.GetValue(EffectiveFontSizeProperty));
    }

    private static void ApplyToContainer(ItemsControl container, FontFamily? font, double fontSize)
    {
        var count = container.Items.Count;
        for (var i = 0; i < count; i++)
        {
            // Direct MenuItem children are their own containers; data-templated items need the generator.
            var containerFromIndex = container.ItemContainerGenerator.ContainerFromIndex(i);
            var menuItem = containerFromIndex as MenuItem
                           ?? container.Items[i] as MenuItem;

            if (menuItem is not null)
            {
                ApplyToMenuItem(menuItem, font, fontSize);
            }
        }
    }

    private static void ApplyToMenuItem(MenuItem menuItem, FontFamily? parentFont, double parentFontSize)
    {
        // Prefer values set directly on the MenuItem; otherwise inherit from the parent container.
        var effectiveFont = GetIconFontFamily(menuItem) ?? parentFont;
        var localSize = GetIconFontSize(menuItem);
        var effectiveSize = !double.IsNaN(localSize) ? localSize : parentFontSize;

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

        var original = OriginalStyles.GetValue(node, static _ => new OriginalIconStyle());

        // FontFamily: skip when the icon already has a local value (explicit Icon wins).
        if (font is not null)
        {
            if (!HasLocalFontFamily(node))
            {
                SetFontFamily(node, font);
                original.OverrodeFontFamily = true;
            }
        }
        else if (original.OverrodeFontFamily)
        {
            ClearFontFamily(node);
            original.OverrodeFontFamily = false;
        }

        // FontSize: same local-value priority.
        if (!double.IsNaN(fontSize))
        {
            if (!HasLocalFontSize(node))
            {
                if (!original.OverrodeFontSize)
                {
                    original.FontSize = ReadFontSize(node);
                    original.HasFontSize = true;
                }

                SetFontSize(node, fontSize);
                original.OverrodeFontSize = true;
            }
        }
        else if (original.OverrodeFontSize && original.HasFontSize)
        {
            SetFontSize(node, original.FontSize);
            original.OverrodeFontSize = false;
        }
    }

    private static bool HasLocalFontFamily(DependencyObject node) =>
        node switch
        {
            FontIcon => node.ReadLocalValue(FontIcon.FontFamilyProperty) != DependencyProperty.UnsetValue,
            TextBlock => node.ReadLocalValue(TextBlock.FontFamilyProperty) != DependencyProperty.UnsetValue,
            Control => node.ReadLocalValue(Control.FontFamilyProperty) != DependencyProperty.UnsetValue,
            _ => true,
        };

    private static bool HasLocalFontSize(DependencyObject node) =>
        node switch
        {
            FontIcon => node.ReadLocalValue(FontIcon.FontSizeProperty) != DependencyProperty.UnsetValue,
            TextBlock => node.ReadLocalValue(TextBlock.FontSizeProperty) != DependencyProperty.UnsetValue,
            Control => node.ReadLocalValue(Control.FontSizeProperty) != DependencyProperty.UnsetValue,
            _ => true,
        };

    private static void SetFontFamily(DependencyObject node, FontFamily font)
    {
        switch (node)
        {
            case FontIcon fontIcon:
                fontIcon.SetCurrentValue(FontIcon.FontFamilyProperty, font);
                break;
            case TextBlock textBlock:
                textBlock.SetCurrentValue(TextBlock.FontFamilyProperty, font);
                break;
            case Control control:
                control.SetCurrentValue(Control.FontFamilyProperty, font);
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
                fontIcon.SetCurrentValue(FontIcon.FontSizeProperty, fontSize);
                break;
            case TextBlock textBlock:
                textBlock.SetCurrentValue(TextBlock.FontSizeProperty, fontSize);
                break;
            case Control control:
                control.SetCurrentValue(Control.FontSizeProperty, fontSize);
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
