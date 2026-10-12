using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using YamlDotNet.RepresentationModel;

namespace Wpf.Ui.Markdown.FrontMatter;

/// <summary>
/// Attached property for storing parsed FrontMatter YAML on <see cref="FlowDocument"/>.
/// </summary>
public static class FrontMatter
{
    public static readonly DependencyProperty FrontMatterYamlProperty =
        DependencyProperty.RegisterAttached(
            "FrontMatterYaml",
            typeof(YamlNode),
            typeof(FrontMatter),
            new PropertyMetadata(null, OnFrontMatterYamlChanged));

    public static readonly DependencyPropertyKey FrontMatterDicPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "FrontMatterDic",
            typeof(Dictionary<string, object>),
            typeof(FrontMatter),
            new PropertyMetadata(null, null, CoerceFrontMatterDic));

    public static readonly DependencyProperty FrontMatterDicProperty =
        FrontMatterDicPropertyKey.DependencyProperty;


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public static void SetFrontMatterYaml(DependencyObject target, YamlNode value)
    {
        target.SetValue(FrontMatterYamlProperty, value);
        target.CoerceValue(FrontMatterDicProperty);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public static YamlNode? GetFrontMatterYaml(DependencyObject target)
    {
        return (YamlNode?)target.GetValue(FrontMatterYamlProperty);
    }

    public static void ClearFrontMatterYaml(DependencyObject target)
    {
        target.ClearValue(FrontMatterYamlProperty);
        target.CoerceValue(FrontMatterDicProperty);
    }

    public static Dictionary<string, object>? GetFrontMatterDic(DependencyObject target)
    {
        return (Dictionary<string, object>?)target.GetValue(FrontMatterDicProperty);
    }

    private static void OnFrontMatterYamlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.ClearValue(FrontMatterDicPropertyKey);
    }

    private static object? CoerceFrontMatterDic(DependencyObject d, object baseValue)
    { 
        return GetFrontMatterYaml(d) switch
        {
            YamlMappingNode mapping => ConvertMapping(mapping),
            YamlSequenceNode sequence => new Dictionary<string, object>
            {
                ["__Sequence__"] = ConvertSequence(sequence)
            },
            YamlScalarNode scalar => new Dictionary<string, object>
            {
                ["__Scalar__"] = ConvertScalar(scalar)
            },
            _ => null
        };
    }

    private static Dictionary<string, object> ConvertMapping(YamlMappingNode mapping)
    {
        var dict = new Dictionary<string, object>();

        foreach (var kvp in mapping.Children)
        {
            var key = (kvp.Key as YamlScalarNode)?.Value ?? string.Empty;
            dict[key] = ConvertNode(kvp.Value);
        }

        return dict;
    }

    private static List<object> ConvertSequence(YamlSequenceNode sequence)
    {
        var list = new List<object>();

        foreach (var node in sequence.Children)
        {
            list.Add(ConvertNode(node));
        }

        return list;
    }

    private static string ConvertScalar(YamlScalarNode scalar)
    {
        return scalar.Value ?? string.Empty;
    }

    private static object ConvertNode(YamlNode node)
    {
        return node switch
        {
            YamlMappingNode mapping => ConvertMapping(mapping),
            YamlSequenceNode sequence => ConvertSequence(sequence),
            YamlScalarNode scalar => ConvertScalar(scalar),
            _ => string.Empty
        };
    }
}