using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Wpf.Ui.Violeta.Markup;

/// <summary>
/// Creates a <see cref="MultiBinding"/> that feeds the source value plus
/// <see cref="Converters.ThemeRefreshConverter.Revision"/> into a
/// <see cref="Converters.ThemeRefreshConverter"/>.
/// </summary>
/// <example>
/// <code language="xml">
/// <![CDATA[
/// Background="{vio:ThemeBinding IsOn, ElementName=Toggle, Converter={StaticResource BoolToBrushGroup}}"
/// ]]>
/// </code>
/// </example>
[MarkupExtensionReturnType(typeof(object))]
public sealed class ThemeBindingExtension : MarkupExtension
{
    public ThemeBindingExtension()
    {
    }

    public ThemeBindingExtension(PropertyPath path)
    {
        Path = path;
    }

    public PropertyPath? Path { get; set; }

    public string? ElementName { get; set; }

    public object? Source { get; set; }

    public RelativeSource? RelativeSource { get; set; }

    public string? StringFormat { get; set; }

    public object? FallbackValue { get; set; }

    public object? TargetNullValue { get; set; }

    public BindingMode Mode { get; set; } = BindingMode.Default;

    public UpdateSourceTrigger UpdateSourceTrigger { get; set; } = UpdateSourceTrigger.Default;

    /// <summary>
    /// Must be a <see cref="Converters.ThemeRefreshConverter"/> instance.
    /// </summary>
    public IMultiValueConverter? Converter { get; set; }

    public object? ConverterParameter { get; set; }

    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (Converter is null)
        {
            throw new InvalidOperationException($"{nameof(ThemeBindingExtension)} requires {nameof(Converter)}.");
        }

        if (Converter is not Converters.ThemeRefreshConverter themeConverter)
        {
            throw new InvalidOperationException(
                $"{nameof(ThemeBindingExtension)}.{nameof(Converter)} must be a {nameof(Converters.ThemeRefreshConverter)}.");
        }

        var valueBinding = new Binding
        {
            Mode = Mode,
            UpdateSourceTrigger = UpdateSourceTrigger,
        };

        if (Path is not null)
        {
            valueBinding.Path = Path;
        }

        if (ElementName is not null)
        {
            valueBinding.ElementName = ElementName;
        }

        if (Source is not null)
        {
            valueBinding.Source = Source;
        }

        if (RelativeSource is not null)
        {
            valueBinding.RelativeSource = RelativeSource;
        }

        if (StringFormat is not null)
        {
            valueBinding.StringFormat = StringFormat;
        }

        if (FallbackValue is not null)
        {
            valueBinding.FallbackValue = FallbackValue;
        }

        if (TargetNullValue is not null)
        {
            valueBinding.TargetNullValue = TargetNullValue;
        }

        var revisionBinding = new Binding(nameof(Converters.ThemeRefreshConverter.Revision))
        {
            Source = themeConverter,
            Mode = BindingMode.OneWay,
        };

        var multi = new MultiBinding
        {
            Converter = themeConverter,
            ConverterParameter = ConverterParameter,
            Mode = Mode,
        };
        multi.Bindings.Add(valueBinding);
        multi.Bindings.Add(revisionBinding);

        return multi.ProvideValue(serviceProvider);
    }
}
