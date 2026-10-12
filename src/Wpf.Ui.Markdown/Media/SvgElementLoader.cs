using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Wpf.Ui.Markdown.Plugins;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Controls.Svg;

namespace Wpf.Ui.Markdown.Media;

/// <summary>
/// Renders SVG images as vector drawings hosted in a Violeta <see cref="SvgImage"/>.
/// </summary>
public class SvgElementLoader : IElementLoader, IPreferredLoader
{
    public FrameworkElement? Load(Stream stream)
    {
        var bytes = ReadAllBytes(stream);
        if (!TryReadRoot(bytes, out var hasWidth, out var hasHeight))
            return null;

        var render = new SVGRender();
        DrawingGroup drawing;
        using (var svgStream = new MemoryStream(bytes, false))
        {
            drawing = render.LoadDrawing(svgStream);
        }

        var size = ResolveSize(render.SVG, drawing, hasWidth, hasHeight);
        if (size.IsEmpty)
            return null;

        var image = new SvgImage
        {
            ImageSource = drawing,
            Width = size.Width,
        };

        // Height follows Width so that a resized width keeps the SVG aspect ratio.
        image.SetBinding(FrameworkElement.HeightProperty, new Binding(nameof(FrameworkElement.Width))
        {
            RelativeSource = RelativeSource.Self,
            Converter = new ScaleConverter(size.Height / size.Width),
        });

        return image;
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static bool TryReadRoot(byte[] bytes, out bool hasWidth, out bool hasHeight)
    {
        hasWidth = false;
        hasHeight = false;

        if (!StartsWithMarkup(bytes))
            return false;

        try
        {
            using var memoryStream = new MemoryStream(bytes, false);
            using var reader = XmlReader.Create(memoryStream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });

            if (reader.MoveToContent() != XmlNodeType.Element || reader.LocalName != "svg")
                return false;

            hasWidth = IsAbsoluteLength(reader.GetAttribute("width"));
            hasHeight = IsAbsoluteLength(reader.GetAttribute("height"));
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }

    private static bool StartsWithMarkup(byte[] bytes)
    {
        var index = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;

        for (; index < bytes.Length; ++index)
        {
            var b = bytes[index];
            if (b == '<')
                return true;

            if (b != ' ' && b != '\t' && b != '\r' && b != '\n')
                return false;
        }

        return false;
    }

    private static bool IsAbsoluteLength(string? value)
        => !string.IsNullOrWhiteSpace(value) && !value!.TrimEnd().EndsWith("%", StringComparison.Ordinal);

    private static Size ResolveSize(SVG svg, Drawing drawing, bool hasWidth, bool hasHeight)
    {
        var size = svg.Size;

        if (hasWidth && hasHeight && IsValid(size.Width, size.Height))
            return size;

        if (svg.ViewBox is { } viewBox && IsValid(viewBox.Width, viewBox.Height))
        {
            var ratio = viewBox.Height / viewBox.Width;

            if (hasWidth && IsValid(size.Width, 1))
                return new Size(size.Width, size.Width * ratio);

            if (hasHeight && IsValid(1, size.Height))
                return new Size(size.Height / ratio, size.Height);

            return new Size(viewBox.Width, viewBox.Height);
        }

        var bounds = drawing.Bounds;
        if (!bounds.IsEmpty && IsValid(bounds.Width, bounds.Height))
            return new Size(bounds.Width, bounds.Height);

        return Size.Empty;
    }

    private static bool IsValid(double width, double height)
        => width > 0 && height > 0 && !double.IsInfinity(width) && !double.IsInfinity(height);

    private sealed class ScaleConverter(double scale) : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => value is double width && !double.IsNaN(width) ? width * scale : double.NaN;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotSupportedException();
    }
}
