using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Win32;

namespace Wpf.Ui.Violeta.Controls;

public partial class ToastControl : UserControl
{
    public Window Window { get; internal set; } = null!;
    public FrameworkElement Owner { get; private set; } = null!;
    public Popup Popup { get; internal set; } = null!;
    public DispatcherTimer Timer { get; set; } = null!;
    public Thickness OffsetMargin { get; set; } = new Thickness(15);
    public ToastConfig? Options { get; set; } = null!;

    public ToastControl() : this(null!, string.Empty)
    {
    }

    public ToastControl(FrameworkElement owner, string message, ToastConfig? options = null)
    {
        InitializeComponent();
        DataContext = this;
        Options = options;

        if (Options != null)
        {
            Time = Options.Time;
            ToastIcon = Options.ToastIcon;
            IconSize = Options.IconSize;
            Location = Options.Location;
            FontStyle = Options.FontStyle;
            FontStretch = Options.FontStretch;
            FontSize = Options.FontSize;
            FontWeight = Options.FontWeight;
            BorderBrush = Options.BorderBrush;
            BorderThickness = Options.BorderThickness;
            CornerRadius = Options.CornerRadius;
            HorizontalContentAlignment = Options.HorizontalContentAlignment;
            VerticalContentAlignment = Options.VerticalContentAlignment;
            OffsetMargin = Options.OffsetMargin;
        }

        Message = message;
        Owner = owner ?? Application.Current.MainWindow;
        Window = Window.GetWindow(Owner);
        if (Window != null)
        {
            Window.Closed += (s, e) => Close();
        }
    }

    internal void ShowCore()
    {
        if (Window == null)
        {
            return;
        }

        Window.Dispatcher.Invoke(() =>
        {
            Popup = new Popup()
            {
                Width = double.NaN,
                Height = double.NaN,
                PopupAnimation = PopupAnimation.Fade,
                AllowsTransparency = true,
                StaysOpen = true,
                Placement = PlacementMode.Relative,
                IsOpen = false,
                Child = this,
                PlacementTarget = Window,
            };

            Window.LocationChanged += OnUpdatePosition;
            Window.SizeChanged += OnUpdatePosition;

            SetPopupOffset(Popup, this);

            Popup.Opened += OnUpdatePosition;
            Popup.Closed += (s, _) =>
            {
                if (s is not Popup popup)
                {
                    return;
                }
                if (popup.Child is not ToastControl t)
                {
                    return;
                }
            };
            Popup.IsOpen = true;
        });

        Timer = new DispatcherTimer();
        Timer.Tick += (_, _) =>
        {
            Popup.IsOpen = false;
            Window.LocationChanged -= OnUpdatePosition;
            Window.SizeChanged -= OnUpdatePosition;
        };
        Timer.Interval = TimeSpan.FromMilliseconds(Options?.Time ?? ToastConfig.NormalTime);
        Timer.Start();
    }

    protected virtual void OnUpdatePosition(object? sender, EventArgs e)
    {
        var up = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (up == null || Popup == null)
        {
            return;
        }
        SetPopupOffset(Popup, this);
        up.Invoke(Popup, null);
    }

    protected virtual void SetPopupOffset(Popup popup, ToastControl toast)
    {
        double ownerWidth = toast.Owner.RenderSize.Width;
        double ownerHeight = toast.Owner.RenderSize.Height;

        double popupWidth = popup.Child.RenderSize.Width;
        double popupHeight = popup.Child.RenderSize.Height;

        Thickness offset = toast.OffsetMargin;

        if (popup.PlacementTarget == null)
        {
            ownerWidth = SystemParameters.WorkArea.Size.Width * DpiHelper.ScaleX;
            ownerHeight = SystemParameters.WorkArea.Size.Height * DpiHelper.ScaleY;
        }

        switch (toast.Location)
        {
            case ToastLocation.Center:
                popup.HorizontalOffset = (ownerWidth - popupWidth) / 2d;
                popup.VerticalOffset = (ownerHeight - popupHeight) / 2d;
                break;

            case ToastLocation.Left:
                popup.HorizontalOffset = offset.Left;
                popup.VerticalOffset = (ownerHeight - popupHeight) / 2d;
                break;

            case ToastLocation.Right:
                popup.HorizontalOffset = ownerWidth - popupWidth - offset.Right;
                popup.VerticalOffset = (ownerHeight - popupHeight) / 2d;
                break;

            case ToastLocation.TopLeft:
                popup.HorizontalOffset = offset.Left;
                popup.VerticalOffset = toast.OffsetMargin.Top + offset.Top;
                break;

            case ToastLocation.TopCenter:
                popup.HorizontalOffset = (ownerWidth - popupWidth) / 2d;
                popup.VerticalOffset = toast.OffsetMargin.Top + offset.Top;
                break;

            case ToastLocation.TopRight:
                popup.HorizontalOffset = ownerWidth - popupWidth - offset.Right;
                popup.VerticalOffset = toast.OffsetMargin.Top + offset.Top;
                break;

            case ToastLocation.BottomLeft:
                popup.HorizontalOffset = offset.Left;
                popup.VerticalOffset = ownerHeight - popupHeight - offset.Bottom;
                break;

            case ToastLocation.BottomCenter:
                popup.HorizontalOffset = (ownerWidth - popupWidth) / 2d;
                popup.VerticalOffset = ownerHeight - popupHeight - offset.Bottom;
                break;

            case ToastLocation.BottomRight:
                popup.HorizontalOffset = ownerWidth - popupWidth - offset.Right;
                popup.VerticalOffset = ownerHeight - popupHeight - offset.Bottom;
                break;
        }
    }

    public void Close()
    {
        if (Timer != null)
        {
            Timer.Stop();
            Timer = null!;
        }
        Popup.IsOpen = false;
        Window.LocationChanged -= OnUpdatePosition;
        Window.SizeChanged -= OnUpdatePosition;
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(ToastControl), new PropertyMetadata(string.Empty));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ToastControl), new PropertyMetadata(new CornerRadius(5)));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(ToastControl), new PropertyMetadata(26.0));

    public new Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public new static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ToastControl), new PropertyMetadata((Brush)new BrushConverter().ConvertFromString("#E1E1E1")!));

    public new Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public new static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(ToastControl), new PropertyMetadata(new Thickness(0.5d)));

    public new Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public new static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(ToastControl), new PropertyMetadata((Brush)new BrushConverter().ConvertFromString("#FAFAFA")!));

    public new HorizontalAlignment HorizontalContentAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public new static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(ToastControl), new PropertyMetadata(HorizontalAlignment.Left));

    public new VerticalAlignment VerticalContentAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    public new static readonly DependencyProperty VerticalContentAlignmentProperty = DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(ToastControl), new PropertyMetadata(VerticalAlignment.Center));

    public new double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public new static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(double), typeof(ToastControl), new PropertyMetadata(100d));

    public new double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public new static readonly DependencyProperty HeightProperty = DependencyProperty.Register("Height", typeof(double), typeof(ToastControl), new PropertyMetadata(48.0));

    public ToastIcon ToastIcon
    {
        get => (ToastIcon)GetValue(MessageBoxIconProperty);
        set => SetValue(MessageBoxIconProperty, value);
    }

    public static readonly DependencyProperty MessageBoxIconProperty = DependencyProperty.Register("ToastIcon", typeof(ToastIcon), typeof(ToastControl));

    public int Time
    {
        get => (int)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(int), typeof(ToastControl), new PropertyMetadata(ToastConfig.NormalTime));

    public ToastLocation Location
    {
        get => (ToastLocation)GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(ToastLocation), typeof(ToastControl), new PropertyMetadata(ToastLocation.TopCenter));
}
