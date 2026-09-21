using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Converters;

namespace Wpf.Ui.Violeta.Markup;

/// <summary>
/// Provides a <see cref="Brush"/> that switches between light and dark values with the app theme.
/// </summary>
/// <example>
/// <code language="xml">
/// <![CDATA[
/// <Border BorderBrush="{vio:ThemeBrush Light=#999999 Dark=#666666}" />
///
/// <Border BorderBrush="{vio:ThemeBrush
///     Light={DynamicResource ControlElevationBorderBrush}
///     Dark={DynamicResource ControlStrokeColorDefaultBrush}}" />
///
/// <Border Background="{vio:ThemeBrush Light=#FFFFFFFF Dark={DynamicResource ApplicationBackgroundBrush}}" />
/// ]]>
/// </code>
/// </example>
/// <remarks>
/// <see cref="MarkupExtension"/> cannot host dependency properties, so nested
/// <c>{DynamicResource}</c> on <see cref="Light"/> / <see cref="Dark"/> is accepted via
/// <see cref="XamlSetMarkupExtensionAttribute"/> and resolved with
/// <see cref="Application.TryFindResource"/> when the theme revision changes.
/// High contrast uses the <see cref="Dark"/> value.
/// </remarks>
[MarkupExtensionReturnType(typeof(Brush))]
[XamlSetMarkupExtension(nameof(ReceiveMarkupExtension))]
public sealed class ThemeBrushExtension : MarkupExtension
{
    private static readonly BrushConverter BrushConverter = new();

    private object? _light;
    private object? _dark;
    private object? _lightResourceKey;
    private object? _darkResourceKey;

    public ThemeBrushExtension()
    {
    }

    /// <summary>
    /// Brush used when the application theme is <see cref="ApplicationTheme.Light"/>.
    /// Accepts a <see cref="Brush"/>, <see cref="Color"/>, color string (e.g. <c>#999999</c>),
    /// or a nested <c>{DynamicResource}</c>.
    /// </summary>
    public object? Light
    {
        get => _light;
        set
        {
            _light = value;
            _lightResourceKey = null;
        }
    }

    /// <summary>
    /// Brush used when the application theme is dark or high contrast.
    /// Accepts a <see cref="Brush"/>, <see cref="Color"/>, color string (e.g. <c>#999999</c>),
    /// or a nested <c>{DynamicResource}</c>.
    /// </summary>
    public object? Dark
    {
        get => _dark;
        set
        {
            _dark = value;
            _darkResourceKey = null;
        }
    }

    /// <summary>
    /// Intercepts nested markup extensions on this extension's properties.
    /// Required for <see cref="DynamicResourceExtension"/>, which can only target a dependency property.
    /// </summary>
    public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
    {
        if (targetObject is not ThemeBrushExtension extension)
        {
            return;
        }

        if (eventArgs.MarkupExtension is not DynamicResourceExtension dynamicResource)
        {
            return;
        }

        if (eventArgs.Member.Name == nameof(Light))
        {
            extension._light = null;
            extension._lightResourceKey = dynamicResource.ResourceKey;
            eventArgs.Handled = true;
        }
        else if (eventArgs.Member.Name == nameof(Dark))
        {
            extension._dark = null;
            extension._darkResourceKey = dynamicResource.ResourceKey;
            eventArgs.Handled = true;
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        MultiBinding multiBinding = new()
        {
            Converter = new ThemeBrushConverter(this),
            Mode = BindingMode.OneWay,
        };

        multiBinding.Bindings.Add(
            new Binding(nameof(ThemeRevision.Revision))
            {
                Source = ThemeRevision.Current,
                Mode = BindingMode.OneWay,
            });

        return multiBinding.ProvideValue(serviceProvider);
    }

    private Brush Resolve()
    {
        bool isLight = ThemeManager.GetAppTheme() == ApplicationTheme.Light;

        Brush? primary = isLight
            ? ResolveSide(_lightResourceKey, Light)
            : ResolveSide(_darkResourceKey, Dark);

        if (primary is not null)
        {
            return primary;
        }

        Brush? fallback = isLight
            ? ResolveSide(_darkResourceKey, Dark)
            : ResolveSide(_lightResourceKey, Light);

        return fallback ?? Brushes.Transparent;
    }

    private static Brush? ResolveSide(object? resourceKey, object? literal)
    {
        if (resourceKey is not null)
        {
            object? resource = Application.Current?.TryFindResource(resourceKey);
            if (resource is Brush brushFromResource)
            {
                return brushFromResource;
            }

            if (resource is Color colorFromResource)
            {
                return new SolidColorBrush(colorFromResource);
            }

            if (resource is string stringFromResource)
            {
                return ParseBrush(stringFromResource);
            }
        }

        return ResolveLiteral(literal);
    }

    private static Brush? ResolveLiteral(object? value)
    {
        return value switch
        {
            null => null,
            Brush brush => brush,
            Color color => new SolidColorBrush(color),
            string text => ParseBrush(text),
            _ => ParseBrush(Convert.ToString(value, CultureInfo.InvariantCulture)),
        };
    }

    private static Brush? ParseBrush(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            return BrushConverter.ConvertFromInvariantString(text) as Brush;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private sealed class ThemeBrushConverter(ThemeBrushExtension owner) : IMultiValueConverter
    {
        public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            _ = values;
            _ = targetType;
            _ = parameter;
            _ = culture;
            return owner.Resolve();
        }

        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            _ = value;
            _ = targetTypes;
            _ = parameter;
            _ = culture;
            throw new NotSupportedException();
        }
    }
}
