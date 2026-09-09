using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Lets the user pick an image file and preview it. Click again to clear the selection.
/// Adapted from HandyControl's ImageSelector, themed with WPF-UI brushes.
/// </summary>
[TemplatePart(Name = PartButton, Type = typeof(Button))]
public class ImageSelector : Control
{
    private const string PartButton = "PART_Button";

    private Button? _button;

    public static readonly RoutedEvent ImageSelectedEvent = EventManager.RegisterRoutedEvent(
        nameof(ImageSelected),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(ImageSelector));

    public event RoutedEventHandler ImageSelected
    {
        add => AddHandler(ImageSelectedEvent, value);
        remove => RemoveHandler(ImageSelectedEvent, value);
    }

    public static readonly RoutedEvent ImageUnselectedEvent = EventManager.RegisterRoutedEvent(
        nameof(ImageUnselected),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(ImageSelector));

    public event RoutedEventHandler ImageUnselected
    {
        add => AddHandler(ImageUnselectedEvent, value);
        remove => RemoveHandler(ImageUnselectedEvent, value);
    }

    static ImageSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ImageSelector),
            new FrameworkPropertyMetadata(typeof(ImageSelector)));
    }

    public override void OnApplyTemplate()
    {
        _button?.Click -= OnButtonClick;

        base.OnApplyTemplate();

        _button = GetTemplateChild(PartButton) as Button;

        _button?.Click += OnButtonClick;
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        SwitchImage();
    }

    private void SwitchImage()
    {
        if (!HasValue)
        {
            var dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                Filter = Filter,
                DefaultExt = DefaultExt,
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var uri = new Uri(dialog.FileName, UriKind.RelativeOrAbsolute);
            SetValue(UriPropertyKey, uri);
            SetValue(PreviewBrushPropertyKey, CreatePreviewBrush(uri));
            SetValue(HasValuePropertyKey, true);
            SetCurrentValue(ToolTipProperty, dialog.FileName);
            RaiseEvent(new RoutedEventArgs(ImageSelectedEvent, this));
        }
        else
        {
            SetValue(UriPropertyKey, null);
            SetValue(PreviewBrushPropertyKey, null);
            SetValue(HasValuePropertyKey, false);
            SetCurrentValue(ToolTipProperty, null);
            RaiseEvent(new RoutedEventArgs(ImageUnselectedEvent, this));
        }
    }

    private ImageBrush CreatePreviewBrush(Uri uri)
    {
        return new ImageBrush(BitmapFrame.Create(uri, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.None))
        {
            Stretch = Stretch,
        };
    }

    #region Stretch

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
        nameof(Stretch),
        typeof(Stretch),
        typeof(ImageSelector),
        new PropertyMetadata(Stretch.UniformToFill, OnStretchChanged));

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageSelector { PreviewBrush: ImageBrush brush })
        {
            brush.Stretch = (Stretch)e.NewValue;
        }
    }

    #endregion Stretch

    #region Uri

    private static readonly DependencyPropertyKey UriPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Uri),
        typeof(Uri),
        typeof(ImageSelector),
        new PropertyMetadata(null));

    public static readonly DependencyProperty UriProperty = UriPropertyKey.DependencyProperty;

    public Uri? Uri => (Uri?)GetValue(UriProperty);

    #endregion Uri

    #region PreviewBrush

    private static readonly DependencyPropertyKey PreviewBrushPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(PreviewBrush),
        typeof(Brush),
        typeof(ImageSelector),
        new PropertyMetadata(null));

    public static readonly DependencyProperty PreviewBrushProperty = PreviewBrushPropertyKey.DependencyProperty;

    public Brush? PreviewBrush => (Brush?)GetValue(PreviewBrushProperty);

    #endregion PreviewBrush

    #region HasValue

    private static readonly DependencyPropertyKey HasValuePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasValue),
        typeof(bool),
        typeof(ImageSelector),
        new PropertyMetadata(false));

    public static readonly DependencyProperty HasValueProperty = HasValuePropertyKey.DependencyProperty;

    public bool HasValue => (bool)GetValue(HasValueProperty);

    #endregion HasValue

    #region StrokeThickness

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness),
        typeof(double),
        typeof(ImageSelector),
        new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    #endregion StrokeThickness

    #region StrokeDashArray

    public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
        nameof(StrokeDashArray),
        typeof(DoubleCollection),
        typeof(ImageSelector),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public DoubleCollection? StrokeDashArray
    {
        get => (DoubleCollection?)GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    #endregion StrokeDashArray

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(ImageSelector),
        new FrameworkPropertyMetadata(new CornerRadius(4), FrameworkPropertyMetadataOptions.AffectsRender));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius

    #region DefaultExt

    public static readonly DependencyProperty DefaultExtProperty = DependencyProperty.Register(
        nameof(DefaultExt),
        typeof(string),
        typeof(ImageSelector),
        new PropertyMetadata(".jpg"));

    public string DefaultExt
    {
        get => (string)GetValue(DefaultExtProperty);
        set => SetValue(DefaultExtProperty, value);
    }

    #endregion DefaultExt

    #region Filter

    public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
        nameof(Filter),
        typeof(string),
        typeof(ImageSelector),
        new PropertyMetadata("Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|JPEG|*.jpg;*.jpeg|PNG|*.png|BMP|*.bmp|GIF|*.gif"));

    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    #endregion Filter
}
