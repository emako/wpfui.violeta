using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// App-wide theme revision. Bind as the second <see cref="MultiBinding"/> input so
/// WPF re-runs the converter when the theme changes.
/// </summary>
public sealed class ThemeRevision : DependencyObject
{
    public static ThemeRevision Current { get; } = new();

    public static readonly DependencyProperty RevisionProperty = DependencyProperty.Register(
        nameof(Revision),
        typeof(int),
        typeof(ThemeRevision),
        new PropertyMetadata(0));

    static ThemeRevision()
    {
        ThemeManager.Changed += OnThemeChanged;
    }

    public int Revision
    {
        get => (int)GetValue(RevisionProperty);
        private set => SetValue(RevisionProperty, value);
    }

    private static void OnThemeChanged(ApplicationTheme theme, Color accent)
    {
        _ = theme;
        _ = accent;

        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        _ = app.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, static () => Current.Revision++);
    }
}

/// <summary>
/// Wraps an <see cref="IValueConverter"/> as an <see cref="IMultiValueConverter"/>.
/// First binding is the source value; second binding is <see cref="ThemeRevision.Revision"/>.
/// </summary>
/// <example>
/// <code language="xml">
/// <![CDATA[
/// <vio:ThemeRefreshConverter x:Key="BoolToBrush">
///   <c:BoolToBrushConverter TrueValue="{DynamicResource AccentFillColorDefaultBrush}"
///                            FalseValue="{DynamicResource TextFillColorPrimaryBrush}" />
/// </vio:ThemeRefreshConverter>
///
/// <Border.Background>
///   <MultiBinding Converter="{StaticResource BoolToBrush}">
///     <Binding Path="IsOn" ElementName="Toggle" />
///     <Binding Path="Revision" Source="{x:Static vio:ThemeRevision.Current}" />
///   </MultiBinding>
/// </Border.Background>
/// ]]>
/// </code>
/// </example>
[ContentProperty(nameof(Converter))]
public sealed class ThemeRefreshConverter : IMultiValueConverter
{
    private static readonly ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, object>> ResourceKeys = new();

    public IValueConverter? Converter { get; set; }

    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        object? value = values is { Length: > 0 } ? values[0] : null;

        // Theme dictionary swaps leave DynamicResource expressions on orphan DOs stale.
        // Re-materialize from Application resources before delegating.
        Rematerialize(Converter);

        return Converter is null
            ? value
            : Converter.Convert(value, targetType, parameter, culture);
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        object? converted = Converter is null
            ? value
            : Converter.ConvertBack(
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

    private static void Rematerialize(object? node)
    {
        if (node is null)
        {
            return;
        }

        if (node is DependencyObject dependencyObject)
        {
            RematerializeResources(dependencyObject);
        }

        PropertyInfo? convertersProperty = node.GetType().GetProperty(
            "Converters",
            BindingFlags.Instance | BindingFlags.Public);

        if (convertersProperty?.GetValue(node) is IEnumerable items)
        {
            foreach (object? item in items)
            {
                Rematerialize(item);
            }
        }
    }

    private static void RematerializeResources(DependencyObject target)
    {
        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        Dictionary<DependencyProperty, object> keys = ResourceKeys.GetOrCreateValue(target);

        LocalValueEnumerator enumerator = target.GetLocalValueEnumerator();
        while (enumerator.MoveNext())
        {
            LocalValueEntry entry = enumerator.Current;
            if (entry.Value is null or BindingExpressionBase)
            {
                continue;
            }

            // ResourceReferenceExpression is internal; detect via ResourceKey.
            PropertyInfo? resourceKeyProperty = entry.Value.GetType().GetProperty(
                "ResourceKey",
                BindingFlags.Instance | BindingFlags.Public);

            if (resourceKeyProperty?.GetValue(entry.Value) is { } key)
            {
                keys[entry.Property] = key;
            }
        }

        foreach (KeyValuePair<DependencyProperty, object> pair in keys)
        {
            if (app.TryFindResource(pair.Value) is { } resource)
            {
                target.SetValue(pair.Key, resource);
            }
        }
    }
}
