using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Resources;
using Wpf.Ui.Violeta.Controls.Primitives;
using Wpf.Ui.Violeta.Controls.Svg;
using Wpf.Ui.Violeta.Controls.Svg.FileLoaders;
using Image = System.Windows.Controls.Image;

namespace Wpf.Ui.Violeta.Controls;

#pragma warning disable IDE0057 // Use range operator

/// <summary>
/// Displays an SVG drawing as an <see cref="Wpf.Ui.Controls.IconElement"/>.
/// </summary>
public class SvgImage : IconElementEx, IUriContext
{
    private Image? _image;
    private SVGRender? _render;
    private Uri? _baseUri;
    private Drawing? _drawing;

    public SvgImage()
    {
        ClipToBounds = true;
        SnapsToDevicePixels = true;
    }

    public SvgImage(Uri uriSource)
        : this()
    {
        UriSource = uriSource;
    }

    public SvgImage(string filePath)
        : this()
    {
        FileSource = filePath;
    }

    /// <summary>
    /// Identifies the <see cref="UriSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UriSourceProperty =
        DependencyProperty.Register(
            nameof(UriSource),
            typeof(Uri),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnUriSourceChanged));

    /// <summary>
    /// Gets or sets the URI of the SVG resource (file, pack, http(s), or data).
    /// </summary>
    public Uri? UriSource
    {
        get => (Uri?)GetValue(UriSourceProperty);
        set => SetValue(UriSourceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Source"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(string),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnSourceChanged));

    /// <summary>
    /// Gets or sets a pack-relative SVG resource path (e.g. <c>/Images/icon.svg</c>).
    /// </summary>
    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="FileSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty FileSourceProperty =
        DependencyProperty.Register(
            nameof(FileSource),
            typeof(string),
            typeof(SvgImage),
            new PropertyMetadata(null, OnFileSourceChanged));

    /// <summary>
    /// Gets or sets a local filesystem path to an SVG file.
    /// </summary>
    public string? FileSource
    {
        get => (string?)GetValue(FileSourceProperty);
        set => SetValue(FileSourceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="ImageSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(Drawing),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnImageSourceChanged));

    /// <summary>
    /// Gets or sets a pre-rendered <see cref="Drawing"/> to display.
    /// </summary>
    public Drawing? ImageSource
    {
        get => (Drawing?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Stretch"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StretchProperty =
        Image.StretchProperty.AddOwner(
            typeof(SvgImage),
            new FrameworkPropertyMetadata(Stretch.Uniform, OnStretchChanged));

    /// <summary>
    /// Gets or sets how the SVG is stretched to fill the layout slot.
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="OverrideColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OverrideColorProperty =
        DependencyProperty.Register(
            nameof(OverrideColor),
            typeof(Color?),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnOverrideColorChanged));

    /// <summary>
    /// Gets or sets a color that replaces both fill and stroke colors.
    /// </summary>
    public Color? OverrideColor
    {
        get => (Color?)GetValue(OverrideColorProperty);
        set => SetValue(OverrideColorProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="OverrideFillColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OverrideFillColorProperty =
        DependencyProperty.Register(
            nameof(OverrideFillColor),
            typeof(Color?),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnOverrideFillColorChanged));

    /// <summary>
    /// Gets or sets a color that replaces fill colors only.
    /// </summary>
    public Color? OverrideFillColor
    {
        get => (Color?)GetValue(OverrideFillColorProperty);
        set => SetValue(OverrideFillColorProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="OverrideStrokeColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OverrideStrokeColorProperty =
        DependencyProperty.Register(
            nameof(OverrideStrokeColor),
            typeof(Color?),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnOverrideStrokeColorChanged));

    /// <summary>
    /// Gets or sets a color that replaces stroke colors only.
    /// </summary>
    public Color? OverrideStrokeColor
    {
        get => (Color?)GetValue(OverrideStrokeColorProperty);
        set => SetValue(OverrideStrokeColorProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="OverrideStrokeWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OverrideStrokeWidthProperty =
        DependencyProperty.Register(
            nameof(OverrideStrokeWidth),
            typeof(double?),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnOverrideStrokeWidthChanged));

    /// <summary>
    /// Gets or sets a stroke width that overrides SVG stroke widths.
    /// </summary>
    public double? OverrideStrokeWidth
    {
        get => (double?)GetValue(OverrideStrokeWidthProperty);
        set => SetValue(OverrideStrokeWidthProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="CustomBrushes"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CustomBrushesProperty =
        DependencyProperty.Register(
            nameof(CustomBrushes),
            typeof(Dictionary<string, Brush>),
            typeof(SvgImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnCustomBrushesChanged));

    /// <summary>
    /// Gets or sets custom brushes keyed by paint-server id.
    /// </summary>
    public Dictionary<string, Brush>? CustomBrushes
    {
        get => (Dictionary<string, Brush>?)GetValue(CustomBrushesProperty);
        set => SetValue(CustomBrushesProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="UseAnimations"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UseAnimationsProperty =
        DependencyProperty.Register(
            nameof(UseAnimations),
            typeof(bool),
            typeof(SvgImage),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether SMIL animations in the SVG are applied.
    /// </summary>
    public bool UseAnimations
    {
        get => (bool)GetValue(UseAnimationsProperty);
        set => SetValue(UseAnimationsProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="ExternalFileLoader"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ExternalFileLoaderProperty =
        DependencyProperty.Register(
            nameof(ExternalFileLoader),
            typeof(IExternalFileLoader),
            typeof(SvgImage),
            new PropertyMetadata(FileSystemLoader.Instance));

    /// <summary>
    /// Gets or sets the loader used for external SVG references.
    /// </summary>
    public IExternalFileLoader? ExternalFileLoader
    {
        get => (IExternalFileLoader?)GetValue(ExternalFileLoaderProperty);
        set => SetValue(ExternalFileLoaderProperty, value);
    }

    /// <inheritdoc />
    public Uri? BaseUri
    {
        get => _baseUri;
        set => _baseUri = value;
    }

    /// <summary>
    /// Gets the parsed SVG model when available.
    /// </summary>
    public SVG? Svg => _render?.SVG;

    protected override UIElement InitializeChildren()
    {
        _image = new Image
        {
            Stretch = Stretch,
            SnapsToDevicePixels = true,
        };

        ApplyCurrentDrawing();
        Children.Add(_image);
        return _image;
    }

    /// <summary>
    /// Loads an SVG from a filesystem path.
    /// </summary>
    public void SetImage(string svgFilename)
    {
        if (string.IsNullOrWhiteSpace(svgFilename))
        {
            SetImage((Drawing?)null);
            return;
        }

        try
        {
            var render = CreateRender(useAnimations: false);
            SetImage(render.LoadDrawing(svgFilename));
        }
        catch
        {
            SetImage((Drawing?)null);
        }
    }

    /// <summary>
    /// Loads an SVG from a stream.
    /// </summary>
    public void SetImage(Stream? stream)
    {
        if (stream is null)
        {
            SetImage((Drawing?)null);
            return;
        }

        try
        {
            var render = CreateRender(useAnimations: false);
            SetImage(render.LoadDrawing(stream));
        }
        catch
        {
            SetImage((Drawing?)null);
        }
    }

    /// <summary>
    /// Loads an SVG from a URI.
    /// </summary>
    public void SetImage(Uri? uriSource)
    {
        if (uriSource is null)
        {
            SetImage((Drawing?)null);
            return;
        }

        try
        {
            var render = CreateRender(useAnimations: UseAnimations);
            var drawing = LoadDrawing(ResolveUri(uriSource), render);
            SetImage(drawing);
        }
        catch
        {
            SetImage((Drawing?)null);
        }
    }

    /// <summary>
    /// Displays a pre-built drawing.
    /// </summary>
    public void SetImage(Drawing? drawing)
    {
        _drawing = drawing;
        ApplyCurrentDrawing();
        InvalidateMeasure();
    }

    /// <summary>
    /// Re-renders the current SVG after override brush/color changes.
    /// </summary>
    public void ReRenderSvg()
    {
        if (_render?.SVG is null)
            return;

        SetImage(_render.CreateDrawing(_render.SVG));
    }

    private static void OnUriSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SvgImage)d).SetImage(e.NewValue as Uri);
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        if (e.NewValue is not string path || string.IsNullOrWhiteSpace(path))
        {
            control.SetImage((Drawing?)null);
            return;
        }

        try
        {
            var uri = new Uri(path, UriKind.Relative);
            var resource = Application.GetResourceStream(uri);
            control.SetImage(resource?.Stream);
        }
        catch
        {
            control.SetImage((Drawing?)null);
        }
    }

    private static void OnFileSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        if (e.NewValue is string path && !string.IsNullOrWhiteSpace(path))
            control.SetImage(path);
        else
            control.SetImage((Drawing?)null);
    }

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SvgImage)d).SetImage(e.NewValue as Drawing);
    }

    private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        control._image?.Stretch = (Stretch)e.NewValue;
    }

    private static void OnOverrideColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        if (control._render is null)
            return;

        control._render.OverrideColor = e.NewValue as Color?;
        control.ReRenderSvg();
    }

    private static void OnOverrideFillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        if (control._render is null)
            return;

        control._render.OverrideFillColor = e.NewValue as Color?;
        control.ReRenderSvg();
    }

    private static void OnOverrideStrokeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        if (control._render is null)
            return;

        control._render.OverrideStrokeColor = e.NewValue as Color?;
        control.ReRenderSvg();
    }

    private static void OnOverrideStrokeWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        if (control._render is null)
            return;

        control._render.OverrideStrokeWidth = e.NewValue as double?;
        control.ReRenderSvg();
    }

    private static void OnCustomBrushesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SvgImage)d;
        if (control._render is null || e.NewValue is not Dictionary<string, Brush> newBrushes)
            return;

        if (control._render.CustomBrushes is not null)
        {
            var merged = new Dictionary<string, Brush>(control._render.CustomBrushes);
            foreach (var brush in newBrushes)
                merged[brush.Key] = brush.Value;
            control._render.CustomBrushes = merged;
        }
        else
        {
            control._render.CustomBrushes = newBrushes;
        }

        control.ReRenderSvg();
    }

    private SVGRender CreateRender(bool useAnimations)
    {
        _render = new SVGRender(ExternalFileLoader ?? FileSystemLoader.Instance)
        {
            UseAnimations = useAnimations,
            OverrideColor = OverrideColor,
            OverrideFillColor = OverrideFillColor,
            OverrideStrokeColor = OverrideStrokeColor,
            OverrideStrokeWidth = OverrideStrokeWidth,
            CustomBrushes = CustomBrushes!,
        };
        return _render;
    }

    private void ApplyCurrentDrawing()
    {
        if (_image is null)
            return;

        if (_drawing is null)
        {
            _image.ClearValue(Image.SourceProperty);
            return;
        }

        var image = new DrawingImage(_drawing);
        if (image.CanFreeze)
            image.Freeze();
        _image.Source = image;
    }

    private Uri? ResolveUri(Uri svgSource)
    {
        if (svgSource.IsAbsoluteUri)
            return svgSource;

        var svgPath = svgSource.ToString();
        if (svgPath.Length > 0 && (svgPath[0] == '\\' || svgPath[0] == '/'))
            svgPath = svgPath.Substring(1);
        svgPath = svgPath.Replace('/', '\\');

        var appBaseDirectory = AppContext.BaseDirectory;
        var localFile = Path.Combine(appBaseDirectory, svgPath);
        if (File.Exists(localFile))
            return new Uri(localFile);

        if (_baseUri is not null)
            return new Uri(_baseUri, svgSource);

        return svgSource;
    }

    private DrawingGroup? LoadDrawing(Uri? svgSource, SVGRender render)
    {
        if (svgSource is null)
            return null;

        string? scheme = null;
        var designTime = DesignerProperties.GetIsInDesignMode(this);
        if (designTime && !svgSource.IsAbsoluteUri)
            scheme = "pack";
        else if (svgSource.IsAbsoluteUri)
            scheme = svgSource.Scheme;

        if (string.IsNullOrWhiteSpace(scheme))
            return null;

        switch (scheme)
        {
            case "file":
            case "https":
            case "http":
                return render.LoadDrawing(svgSource);

            case "pack":
                {
                    StreamResourceInfo? svgStreamInfo = null;
                    if (svgSource.ToString().IndexOf("siteoforigin", StringComparison.OrdinalIgnoreCase) >= 0)
                        svgStreamInfo = Application.GetRemoteStream(svgSource);
                    else
                        svgStreamInfo = Application.GetResourceStream(svgSource);

                    var svgStream = svgStreamInfo?.Stream;
                    if (svgStream is null)
                        return null;

                    var fileExt = Path.GetExtension(svgSource.ToString());
                    var isCompressed = !string.IsNullOrWhiteSpace(fileExt) &&
                                       string.Equals(fileExt, ".svgz", StringComparison.OrdinalIgnoreCase);

                    using (svgStream)
                    {
                        if (isCompressed)
                        {
                            using var zipStream = new GZipStream(svgStream, CompressionMode.Decompress);
                            return render.LoadDrawing(zipStream);
                        }

                        return render.LoadDrawing(svgStream);
                    }
                }

            case "data":
                {
                    var sourceData = svgSource.OriginalString.Replace(" ", string.Empty);
                    var nColon = sourceData.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                    var nSemiColon = sourceData.IndexOf(";", StringComparison.OrdinalIgnoreCase);
                    var nComma = sourceData.IndexOf(",", StringComparison.OrdinalIgnoreCase);
                    if (nColon < 0 || nSemiColon < 0 || nComma < 0)
                        return null;

                    var sMimeType = sourceData.Substring(nColon + 1, nSemiColon - nColon - 1);
                    var sEncoding = sourceData.Substring(nSemiColon + 1, nComma - nSemiColon - 1);
                    if (!string.Equals(sMimeType.Trim(), "image/svg+xml", StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(sEncoding.Trim(), "base64", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    var sContent = RemoveWhitespace(sourceData.Substring(nComma + 1));
                    var imageBytes = Convert.FromBase64CharArray(sContent.ToCharArray(), 0, sContent.Length);
                    var isGZiped = sContent.StartsWith("H4sI", StringComparison.Ordinal);
                    using var stream = new MemoryStream(imageBytes);
                    if (isGZiped)
                    {
                        using var zipStream = new GZipStream(stream, CompressionMode.Decompress);
                        return render.LoadDrawing(zipStream);
                    }

                    return render.LoadDrawing(stream);
                }
        }

        return null;
    }

    private static string RemoveWhitespace(string str)
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        var src = str.ToCharArray();
        var dstIdx = 0;
        for (var i = 0; i < src.Length; i++)
        {
            var ch = src[i];
            switch (ch)
            {
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    continue;
                default:
                    src[dstIdx++] = ch;
                    break;
            }
        }

        return new string(src, 0, dstIdx);
    }
}
