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
/// <see cref="IconFontFamilyProperty"/> / <see cref="IconFontSizeProperty"/> /
/// <see cref="IconWidthProperty"/> style <see cref="MenuItem.Icon"/> glyphs on a
/// <see cref="ContextMenu"/>, <see cref="Menu"/> or <see cref="MenuItem"/> independently of
/// the menu text. Set them on the menu root to affect all items, or on a single
/// <see cref="MenuItem"/> for that item (and its submenu).
/// An explicit <c>FontSize</c> / <c>FontFamily</c> / <c>Width</c> on the icon element itself
/// always wins.
/// <para />
/// When <see cref="MenuItem.Icon"/> is assigned a <see cref="string"/> (e.g. XAML
/// <c>Icon="&amp;#xEA66;"</c>), it is automatically converted to a
/// <see cref="FontIcon"/> whose <see cref="FontIcon.Glyph"/> is that string.
/// <see cref="FontIcon.FontFamily"/> defaults to the <c>SymbolThemeFontFamily</c> resource
/// unless <see cref="IconFontFamilyProperty"/> is set; <see cref="IconFontSizeProperty"/>
/// supplies the default glyph size when present; <see cref="IconWidthProperty"/> pins a
/// uniform icon slot width (default <see cref="double.NaN"/> = auto / content width).
/// </remarks>
public static class ControlHelper
{
    private const string SymbolThemeFontFamilyKey = "SymbolThemeFontFamily";

    //[ModuleInitializer]
    internal static void InitializeMenuItemStringIcon()
    {
        // XAML Icon="&#xEA66;" on IconElement-typed properties (ui:MenuItem.Icon, …).
        TypeDescriptor.AddAttributes(
            typeof(IconElement),
            new TypeConverterAttribute(typeof(Violeta.Controls.Primitives.IconElementConverterEx)));

        // Runtime string Icon on MenuItem (object DP / code-behind), even without IconFont*.
        EventManager.RegisterClassHandler(
            typeof(MenuItem),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnAnyMenuItemLoaded));
    }

    // -- Header --------------------------------------------------------------

    /// <summary>
    /// Identifies the Header attached property.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.RegisterAttached(
            "Header",
            typeof(object),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(OnHeaderChanged));

    /// <summary>
    /// Gets the content for the control's header.
    /// </summary>
    public static object GetHeader(Control control) =>
        control.GetValue(HeaderProperty);

    /// <summary>
    /// Sets the content for the control's header.
    /// </summary>
    public static void SetHeader(Control control, object value) =>
        control.SetValue(HeaderProperty, value);

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateHeaderVisibility((Control)d);
    }

    // -- HeaderTemplate ------------------------------------------------------

    /// <summary>
    /// Identifies the HeaderTemplate attached property.
    /// </summary>
    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.RegisterAttached(
            "HeaderTemplate",
            typeof(DataTemplate),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(OnHeaderTemplateChanged));

    /// <summary>
    /// Gets the DataTemplate used to display the content of the control's header.
    /// </summary>
    public static DataTemplate GetHeaderTemplate(Control control) =>
        (DataTemplate)control.GetValue(HeaderTemplateProperty);

    /// <summary>
    /// Sets the DataTemplate used to display the content of the control's header.
    /// </summary>
    public static void SetHeaderTemplate(Control control, DataTemplate value) =>
        control.SetValue(HeaderTemplateProperty, value);

    private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateHeaderVisibility((Control)d);
    }

    // -- HeaderVisibility ----------------------------------------------------

    private static readonly DependencyPropertyKey HeaderVisibilityPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "HeaderVisibility",
            typeof(Visibility),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

    /// <summary>
    /// Identifies the HeaderVisibility attached property.
    /// </summary>
    public static readonly DependencyProperty HeaderVisibilityProperty =
        HeaderVisibilityPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the visibility of the control's header.
    /// </summary>
    public static Visibility GetHeaderVisibility(Control control) =>
        (Visibility)control.GetValue(HeaderVisibilityProperty);

    private static void SetHeaderVisibility(Control control, Visibility value) =>
        control.SetValue(HeaderVisibilityPropertyKey, value);

    private static void UpdateHeaderVisibility(Control control)
    {
        Visibility visibility;

        if (GetHeaderTemplate(control) != null)
        {
            visibility = Visibility.Visible;
        }
        else
        {
            visibility = IsNullOrEmptyString(GetHeader(control)) ? Visibility.Collapsed : Visibility.Visible;
        }

        SetHeaderVisibility(control, visibility);
    }

    // -- PlaceholderText -----------------------------------------------------

    /// <summary>
    /// Identifies the PlaceholderText attached property.
    /// </summary>
    /// <remarks>
    /// Must use the parameterless <see cref="DependencyProperty.AddOwner(System.Type)"/> overload:
    /// adding <see cref="PropertyMetadata"/> calls <c>OverrideMetadata</c>, which requires a
    /// <see cref="DependencyObject"/> owner type and fails for this static helper.
    /// </remarks>
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
    public static void SetPlaceholderText(DependencyObject element, string value)
    {
        element.SetValue(PlaceholderTextProperty, value);
        if (element is Control control)
        {
            UpdatePlaceholderTextVisibility(control);
        }
    }

    // -- PlaceholderTextVisibility -------------------------------------------

    private static readonly DependencyPropertyKey PlaceholderTextVisibilityPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "PlaceholderTextVisibility",
            typeof(Visibility),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

    /// <summary>
    /// Identifies the PlaceholderTextVisibility attached property.
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextVisibilityProperty =
        PlaceholderTextVisibilityPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the visibility of the placeholder text.
    /// </summary>
    public static Visibility GetPlaceholderTextVisibility(Control control) =>
        (Visibility)control.GetValue(PlaceholderTextVisibilityProperty);

    private static void SetPlaceholderTextVisibility(Control control, Visibility value) =>
        control.SetValue(PlaceholderTextVisibilityPropertyKey, value);

    private static void UpdatePlaceholderTextVisibility(Control control)
    {
        SetPlaceholderTextVisibility(
            control,
            string.IsNullOrEmpty(GetPlaceholderText(control)) ? Visibility.Collapsed : Visibility.Visible);
    }

    // -- PlaceholderForeground -----------------------------------------------

    /// <summary>
    /// Identifies the PlaceholderForeground attached property.
    /// </summary>
    public static readonly DependencyProperty PlaceholderForegroundProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderForeground",
            typeof(Brush),
            typeof(ControlHelper),
            null);

    /// <summary>
    /// Gets a brush that describes the color of placeholder text.
    /// </summary>
    public static Brush GetPlaceholderForeground(Control control) =>
        (Brush)control.GetValue(PlaceholderForegroundProperty);

    /// <summary>
    /// Sets a brush that describes the color of placeholder text.
    /// </summary>
    public static void SetPlaceholderForeground(Control control, Brush value) =>
        control.SetValue(PlaceholderForegroundProperty, value);

    // -- Description ---------------------------------------------------------

    /// <summary>
    /// Identifies the Description attached property.
    /// </summary>
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.RegisterAttached(
            "Description",
            typeof(object),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(OnDescriptionChanged));

    /// <summary>
    /// Gets content that is shown below the control.
    /// </summary>
    public static object GetDescription(Control control) =>
        control.GetValue(DescriptionProperty);

    /// <summary>
    /// Sets content that is shown below the control.
    /// </summary>
    public static void SetDescription(Control control, object value) =>
        control.SetValue(DescriptionProperty, value);

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateDescriptionVisibility((Control)d);
    }

    // -- DescriptionVisibility -----------------------------------------------

    private static readonly DependencyPropertyKey DescriptionVisibilityPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "DescriptionVisibility",
            typeof(Visibility),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

    /// <summary>
    /// Identifies the DescriptionVisibility attached property.
    /// </summary>
    public static readonly DependencyProperty DescriptionVisibilityProperty =
        DescriptionVisibilityPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the visibility of the description content.
    /// </summary>
    public static Visibility GetDescriptionVisibility(Control control) =>
        (Visibility)control.GetValue(DescriptionVisibilityProperty);

    private static void SetDescriptionVisibility(Control control, Visibility value) =>
        control.SetValue(DescriptionVisibilityPropertyKey, value);

    private static void UpdateDescriptionVisibility(Control control)
    {
        SetDescriptionVisibility(
            control,
            IsNullOrEmptyString(GetDescription(control)) ? Visibility.Collapsed : Visibility.Visible);
    }

    // -- VisualState ---------------------------------------------------------

    /// <summary>
    /// Identifies the VisualState attached property.
    /// </summary>
    public static readonly DependencyProperty VisualStateProperty =
        DependencyProperty.RegisterAttached(
            "VisualState",
            typeof(string),
            typeof(ControlHelper),
            new FrameworkPropertyMetadata(OnVisualStateChanged));

    /// <summary>
    /// Gets the visual state for the control.
    /// </summary>
    public static string GetVisualState(FrameworkElement control) =>
        (string)control.GetValue(VisualStateProperty);

    /// <summary>
    /// Sets the visual state for the control.
    /// </summary>
    public static void SetVisualState(FrameworkElement control, string value) =>
        control.SetValue(VisualStateProperty, value);

    private static void OnVisualStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UpdateVisualState((FrameworkElement)d);
    }

    private static void UpdateVisualState(FrameworkElement control)
    {
        string state = GetVisualState(control);
        if (!string.IsNullOrEmpty(state))
        {
            if (control.IsLoaded)
            {
                VisualStateManager.GoToElementState(control, state, true);
            }
            else
            {
                control.Loaded += (sender, e) => VisualStateManager.GoToElementState(control, state, false);
            }
        }
    }

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

    // -- IconWidth -----------------------------------------------------------

    /// <summary>
    /// Identifies the IconWidth attached property.
    /// Use <see cref="double.NaN"/> (default) to keep the icon's natural content width.
    /// When set, different glyphs share a uniform slot width (useful with
    /// <see cref="IconFontSizeProperty"/> so menu columns align).
    /// </summary>
    public static readonly DependencyProperty IconWidthProperty =
        DependencyProperty.RegisterAttached(
            "IconWidth",
            typeof(double),
            typeof(ControlHelper),
            new PropertyMetadata(double.NaN, OnIconAppearanceChanged));

    /// <summary>
    /// Gets the fixed width applied to icon elements.
    /// </summary>
    public static double GetIconWidth(DependencyObject element) =>
        (double)element.GetValue(IconWidthProperty);

    /// <summary>
    /// Sets the fixed width applied to icon elements.
    /// </summary>
    public static void SetIconWidth(DependencyObject element, double value) =>
        element.SetValue(IconWidthProperty, value);

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
    /// Effective icon width propagated into submenu popups.
    /// </summary>
    private static readonly DependencyProperty EffectiveIconWidthProperty =
        DependencyProperty.RegisterAttached(
            "IconWidth_Effective",
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
        public double Width { get; set; }
        public bool HasWidth { get; set; }
        public bool OverrodeWidth { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public bool HasHorizontalAlignment { get; set; }
        public bool OverrodeHorizontalAlignment { get; set; }
    }

    [SuppressMessage("Style", "IDE0028:Simplify collection initialization")]
    private static readonly ConditionalWeakTable<DependencyObject, OriginalIconStyle> OriginalStyles = new();

    private static void OnIconAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        _ = e;

        var family = GetIconFontFamily(d);
        var size = GetIconFontSize(d);
        var width = GetIconWidth(d);

        // MenuItem: apply to THIS item's Icon first, then walk submenu children.
        if (d is MenuItem menuItem)
        {
            menuItem.SetValue(EffectiveFontFamilyProperty, family);
            menuItem.SetValue(EffectiveFontSizeProperty, size);
            menuItem.SetValue(EffectiveIconWidthProperty, width);
            HookContainer(menuItem);
            TryConvertStringIcon(menuItem);
            ApplyIconAppearance(menuItem.Icon, family, size, width);
            ApplyToContainer(menuItem, family, size, width);
            return;
        }

        // ContextMenu / Menu: push into generated item containers.
        if (d is ItemsControl container)
        {
            container.SetValue(EffectiveFontFamilyProperty, family);
            container.SetValue(EffectiveFontSizeProperty, size);
            container.SetValue(EffectiveIconWidthProperty, width);
            HookContainer(container);
            ApplyToContainer(container, family, size, width);
        }
    }

    private static void OnAnyMenuItemLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
        {
            return;
        }

        HookMenuItemIcon(menuItem);
        TryConvertStringIcon(menuItem);

        // Pull effective icon style from nearest ancestor menu / item when present.
        var family = (FontFamily?)menuItem.GetValue(EffectiveFontFamilyProperty) ?? GetIconFontFamily(menuItem);
        var size = ResolveEffectiveFontSize(menuItem);
        var width = ResolveEffectiveIconWidth(menuItem);

        ApplyIconAppearance(menuItem.Icon, family, size, width);
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
            HookMenuItemIcon(menuItem);
            menuItem.SubmenuOpened += OnSubmenuOpened;
        }
    }

    private static void HookMenuItemIcon(MenuItem menuItem)
    {
        if ((bool)menuItem.GetValue(IsIconHookedProperty))
        {
            return;
        }

        menuItem.SetValue(IsIconHookedProperty, true);
        DependencyPropertyDescriptor.FromProperty(MenuItem.IconProperty, typeof(MenuItem))
            .AddValueChanged(menuItem, OnMenuItemIconChanged);
    }

    /// <summary>
    /// Marks a <see cref="MenuItem"/> whose <see cref="MenuItem.IconProperty"/> change hook is attached.
    /// </summary>
    private static readonly DependencyProperty IsIconHookedProperty =
        DependencyProperty.RegisterAttached(
            "IconAppearance_IsIconHooked",
            typeof(bool),
            typeof(ControlHelper),
            new PropertyMetadata(false));

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
        if (sender is not MenuItem menuItem)
        {
            return;
        }

        TryConvertStringIcon(menuItem);
        ApplyIconAppearance(
            menuItem.Icon,
            (FontFamily?)menuItem.GetValue(EffectiveFontFamilyProperty) ?? GetIconFontFamily(menuItem),
            ResolveEffectiveFontSize(menuItem),
            ResolveEffectiveIconWidth(menuItem));
    }

    private static double ResolveEffectiveFontSize(MenuItem menuItem)
    {
        var size = (double)menuItem.GetValue(EffectiveFontSizeProperty);
        if (!double.IsNaN(size))
        {
            return size;
        }

        return GetIconFontSize(menuItem);
    }

    private static double ResolveEffectiveIconWidth(MenuItem menuItem)
    {
        var width = (double)menuItem.GetValue(EffectiveIconWidthProperty);
        if (!double.IsNaN(width))
        {
            return width;
        }

        return GetIconWidth(menuItem);
    }

    /// <summary>
    /// Turns a string <see cref="MenuItem.Icon"/> into a <see cref="FontIcon"/> (Glyph = string).
    /// </summary>
    private static void TryConvertStringIcon(MenuItem menuItem)
    {
        // Base DP is object; ui:MenuItem.Icon getter casts to IconElement and throws on string.
        // Read the raw DP value so runtime / unconverted strings are still handled.
        var raw = menuItem.GetValue(MenuItem.IconProperty);
        if (raw is not string glyph || string.IsNullOrEmpty(glyph))
        {
            return;
        }

        var fontIcon = Violeta.Controls.Primitives.IconElementConverterEx.CreateFontIconFromGlyph(glyph);
        menuItem.SetCurrentValue(MenuItem.IconProperty, fontIcon);
    }

    private static void RefreshContainer(ItemsControl container)
    {
        ApplyToContainer(
            container,
            (FontFamily?)container.GetValue(EffectiveFontFamilyProperty),
            (double)container.GetValue(EffectiveFontSizeProperty),
            (double)container.GetValue(EffectiveIconWidthProperty));
    }

    private static void ApplyToContainer(ItemsControl container, FontFamily? font, double fontSize, double iconWidth)
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
                ApplyToMenuItem(menuItem, font, fontSize, iconWidth);
            }
        }
    }

    private static void ApplyToMenuItem(MenuItem menuItem, FontFamily? parentFont, double parentFontSize, double parentIconWidth)
    {
        // Prefer values set directly on the MenuItem; otherwise inherit from the parent container.
        var effectiveFont = GetIconFontFamily(menuItem) ?? parentFont;
        var localSize = GetIconFontSize(menuItem);
        var effectiveSize = !double.IsNaN(localSize) ? localSize : parentFontSize;
        var localWidth = GetIconWidth(menuItem);
        var effectiveWidth = !double.IsNaN(localWidth) ? localWidth : parentIconWidth;

        menuItem.SetValue(EffectiveFontFamilyProperty, effectiveFont);
        menuItem.SetValue(EffectiveFontSizeProperty, effectiveSize);
        menuItem.SetValue(EffectiveIconWidthProperty, effectiveWidth);
        HookContainer(menuItem);
        TryConvertStringIcon(menuItem);
        ApplyIconAppearance(menuItem.Icon, effectiveFont, effectiveSize, effectiveWidth);
        ApplyToContainer(menuItem, effectiveFont, effectiveSize, effectiveWidth);
    }

    private static void ApplyIconAppearance(object? icon, FontFamily? font, double fontSize, double iconWidth)
    {
        if (icon is not DependencyObject root)
        {
            return;
        }

        ApplyToNode(root, font, fontSize, iconWidth);

        foreach (var textBlock in EnumerateTextBlocks(root))
        {
            // Nested TextBlocks only receive typography; width is reserved for the icon root.
            ApplyToNode(textBlock, font, fontSize, double.NaN);
        }
    }

    private static void ApplyToNode(DependencyObject node, FontFamily? font, double fontSize, double iconWidth)
    {
        if (node is not (FontIcon or TextBlock or Control or FrameworkElement))
        {
            return;
        }

        var original = OriginalStyles.GetValue(node, static _ => new OriginalIconStyle());

        // FontFamily: explicit local value on the icon always wins.
        // When IconFontFamily is unset, FontIcon without a local family defaults to SymbolThemeFontFamily
        // (string→FontIcon leaves FontFamily unset on purpose).
        if (node is FontIcon or TextBlock or Control)
        {
            if (font is not null)
            {
                if (!HasLocalFontFamily(node) || original.OverrodeFontFamily)
                {
                    SetFontFamily(node, font);
                    original.OverrodeFontFamily = true;
                }
            }
            else if (!HasLocalFontFamily(node) || original.OverrodeFontFamily)
            {
                if (node is FontIcon fontIcon)
                {
                    fontIcon.SetResourceReference(FontIcon.FontFamilyProperty, SymbolThemeFontFamilyKey);
                    original.OverrodeFontFamily = true;
                }
                else if (original.OverrodeFontFamily)
                {
                    ClearFontFamily(node);
                    original.OverrodeFontFamily = false;
                }
            }

            // FontSize: same local-value priority.
            if (!double.IsNaN(fontSize))
            {
                if (!HasLocalFontSize(node) || original.OverrodeFontSize)
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

        // Width: pin a uniform slot so glyphs with different advance widths align.
        // Explicit local Width on the icon always wins. NaN keeps auto sizing.
        // When width is fixed, center the glyph in the slot unless HorizontalAlignment is local.
        if (node is FrameworkElement frameworkElement)
        {
            if (!double.IsNaN(iconWidth))
            {
                if (!HasLocalWidth(frameworkElement) || original.OverrodeWidth)
                {
                    if (!original.OverrodeWidth)
                    {
                        original.Width = frameworkElement.Width;
                        original.HasWidth = true;
                    }

                    frameworkElement.SetCurrentValue(FrameworkElement.WidthProperty, iconWidth);
                    original.OverrodeWidth = true;
                }

                if (!HasLocalHorizontalAlignment(frameworkElement) || original.OverrodeHorizontalAlignment)
                {
                    if (!original.OverrodeHorizontalAlignment)
                    {
                        original.HorizontalAlignment = frameworkElement.HorizontalAlignment;
                        original.HasHorizontalAlignment = true;
                    }

                    frameworkElement.SetCurrentValue(
                        FrameworkElement.HorizontalAlignmentProperty,
                        HorizontalAlignment.Center);
                    original.OverrodeHorizontalAlignment = true;
                }
            }
            else
            {
                if (original.OverrodeWidth && original.HasWidth)
                {
                    frameworkElement.SetCurrentValue(FrameworkElement.WidthProperty, original.Width);
                    original.OverrodeWidth = false;
                }

                if (original.OverrodeHorizontalAlignment && original.HasHorizontalAlignment)
                {
                    frameworkElement.SetCurrentValue(
                        FrameworkElement.HorizontalAlignmentProperty,
                        original.HorizontalAlignment);
                    original.OverrodeHorizontalAlignment = false;
                }
            }
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

    private static bool HasLocalWidth(FrameworkElement element) =>
        element.ReadLocalValue(FrameworkElement.WidthProperty) != DependencyProperty.UnsetValue;

    private static bool HasLocalHorizontalAlignment(FrameworkElement element) =>
        element.ReadLocalValue(FrameworkElement.HorizontalAlignmentProperty) != DependencyProperty.UnsetValue;

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

    internal static bool IsNullOrEmptyString(object? obj) =>
        obj is null || obj is string s && string.IsNullOrEmpty(s);
}
