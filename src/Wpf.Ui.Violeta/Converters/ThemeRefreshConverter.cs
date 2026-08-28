using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Theme-aware wrapper around an inner <see cref="IValueConverter"/> (e.g. a
/// LiteObservableConverters <c>ValueConverterGroup</c>).
/// <para>
/// Use as an <see cref="IMultiValueConverter"/>: first binding is the source value,
/// second binding is <see cref="Revision"/>. Theme changes increment <see cref="Revision"/>,
/// so WPF re-runs <c>Convert</c> with no target / visual-tree scanning.
/// </para>
/// </summary>
/// <example>
/// <code language="xml">
/// <![CDATA[
/// <vio:ThemeRefreshConverter x:Key="BoolToBrushGroup">
///   <vio:ThemeRefreshConverter.Converter>
///     <c:ValueConverterGroup>
///       <c:BoolToBrushConverter TrueValue="{DynamicResource TextFillColorPrimaryBrush}"
///                                FalseValue="{DynamicResource AccentFillColorDefaultBrush}" />
///     </c:ValueConverterGroup>
///   </vio:ThemeRefreshConverter.Converter>
/// </vio:ThemeRefreshConverter>
///
/// <Border.Background>
///   <MultiBinding Converter="{StaticResource BoolToBrushGroup}">
///     <Binding Path="IsOn" ElementName="Toggle" />
///     <Binding Path="Revision" Source="{StaticResource BoolToBrushGroup}" />
///   </MultiBinding>
/// </Border.Background>
/// ]]>
/// </code>
/// </example>
[ContentProperty(nameof(Converter))]
public sealed class ThemeRefreshConverter : DependencyObject, IMultiValueConverter
{
    public static readonly DependencyProperty ConverterProperty = DependencyProperty.Register(
        nameof(Converter),
        typeof(IValueConverter),
        typeof(ThemeRefreshConverter),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RevisionProperty = DependencyProperty.Register(
        nameof(Revision),
        typeof(int),
        typeof(ThemeRefreshConverter),
        new PropertyMetadata(0));

    private static bool _subscribed;
    private static readonly List<WeakReference<ThemeRefreshConverter>> LiveInstances = [];

    public ThemeRefreshConverter()
    {
        EnsureSubscribed();
        lock (LiveInstances)
        {
            LiveInstances.Add(new WeakReference<ThemeRefreshConverter>(this));
        }
    }

    /// <summary>
    /// Inner converter (typically <c>ValueConverterGroup</c>).
    /// </summary>
    public IValueConverter? Converter
    {
        get => (IValueConverter?)GetValue(ConverterProperty);
        set => SetValue(ConverterProperty, value);
    }

    /// <summary>
    /// Bumped on every theme change. Bind this as the second <see cref="MultiBinding"/> input.
    /// </summary>
    public int Revision
    {
        get => (int)GetValue(RevisionProperty);
        private set => SetValue(RevisionProperty, value);
    }

    private static void EnsureSubscribed()
    {
        if (_subscribed)
        {
            return;
        }

        _subscribed = true;
        ThemeManager.Changed += OnThemeChanged;
    }

    private static void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        _ = currentApplicationTheme;
        _ = systemAccent;

        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        _ = app.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(BumpAllRevisions));
    }

    private static void BumpAllRevisions()
    {
        lock (LiveInstances)
        {
            for (int i = LiveInstances.Count - 1; i >= 0; i--)
            {
                if (!LiveInstances[i].TryGetTarget(out ThemeRefreshConverter? instance))
                {
                    LiveInstances.RemoveAt(i);
                    continue;
                }

                InvalidateOwnedResources(instance.Converter);
                instance.Revision++;
            }
        }
    }

    /// <summary>
    /// Invalidates local DPs on the owned converter chain so DynamicResource re-resolves.
    /// </summary>
    private static void InvalidateOwnedResources(object? converter)
    {
        if (converter is null)
        {
            return;
        }

        if (converter is DependencyObject dependencyObject)
        {
            LocalValueEnumerator enumerator = dependencyObject.GetLocalValueEnumerator();
            while (enumerator.MoveNext())
            {
                LocalValueEntry entry = enumerator.Current;
                if (entry.Value is BindingExpressionBase)
                {
                    continue;
                }

                dependencyObject.InvalidateProperty(entry.Property);
            }
        }

        PropertyInfo? convertersProperty = converter.GetType().GetProperty(
            "Converters",
            BindingFlags.Instance | BindingFlags.Public);

        if (convertersProperty?.GetValue(converter) is IEnumerable items)
        {
            foreach (object? item in items)
            {
                InvalidateOwnedResources(item);
            }
        }
    }

    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        EnsureSubscribed();

        object? value = values is { Length: > 0 } ? values[0] : null;

        InvalidateOwnedResources(Converter);

        IValueConverter? converter = Converter;
        return converter is null
            ? value
            : converter.Convert(value, targetType, parameter, culture);
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        IValueConverter? converter = Converter;
        object? converted = converter is null
            ? value
            : converter.ConvertBack(
                value,
                targetTypes is { Length: > 0 } ? targetTypes[0] : typeof(object),
                parameter,
                culture);

        var result = new object?[targetTypes.Length];
        if (result.Length > 0)
        {
            result[0] = converted;
        }

        for (int i = 1; i < result.Length; i++)
        {
            result[i] = Binding.DoNothing;
        }

        return result;
    }
}
