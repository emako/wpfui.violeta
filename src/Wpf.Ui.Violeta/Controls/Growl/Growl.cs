using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Stacked notification card inspired by HandyControl Growl, themed with WPF-UI brushes
/// and Toast-style icon colors.
/// </summary>
[TemplatePart(Name = ElementPanelMore, Type = typeof(Panel))]
[TemplatePart(Name = ElementGridMain, Type = typeof(Grid))]
[TemplatePart(Name = ElementButtonClose, Type = typeof(Button))]
public class Growl : Control
{
    private const string ElementPanelMore = "PART_PanelMore";
    private const string ElementGridMain = "PART_GridMain";
    private const string ElementButtonClose = "PART_ButtonClose";
    private const int MinWaitTime = 2;
    private const int TranslateTransformIndex = 3;

    public static readonly RoutedCommand CloseCommand = new(nameof(CloseCommand), typeof(Growl));
    public static readonly RoutedCommand CancelCommand = new(nameof(CancelCommand), typeof(Growl));
    public static readonly RoutedCommand ConfirmCommand = new(nameof(ConfirmCommand), typeof(Growl));

    private static GrowlWindow? s_growlWindow;
    private static GrowlAdorner? s_defaultAdorner;

    private static readonly ControlTokenManager<Panel> TokenManager =
        new(registerCallback: OnTokenRegistered, unregisterCallback: OnTokenUnregistered);

    private static readonly SolidColorBrush InfoBrush = CreateFrozenBrush("#55CEF1");
    private static readonly SolidColorBrush SuccessBrush = CreateFrozenBrush("#75CD43");
    private static readonly SolidColorBrush WarningBrush = CreateFrozenBrush("#F9D01A");
    private static readonly SolidColorBrush ErrorBrush = CreateFrozenBrush("#FF5656");
    private static readonly SolidColorBrush FatalBrush = CreateFrozenBrush("#C8C8C8");

    public static readonly DependencyProperty GrowlParentProperty = DependencyProperty.RegisterAttached(
        "GrowlParent", typeof(bool), typeof(Growl),
        new PropertyMetadata(false, OnGrowlParentChanged));

    public static readonly DependencyProperty ShowModeProperty = DependencyProperty.RegisterAttached(
        "ShowMode", typeof(GrowlShowMode), typeof(Growl),
        new FrameworkPropertyMetadata(GrowlShowMode.Prepend, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty TransitionModeProperty = DependencyProperty.RegisterAttached(
        "TransitionMode", typeof(GrowlTransitionMode), typeof(Growl),
        new FrameworkPropertyMetadata(GrowlTransitionMode.Right2LeftWithFade, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty TransitionStoryboardProperty = DependencyProperty.RegisterAttached(
        "TransitionStoryboard", typeof(Storyboard), typeof(Growl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty TokenProperty = DependencyProperty.RegisterAttached(
        "Token", typeof(string), typeof(Growl),
        new PropertyMetadata(null, TokenManager.OnTokenChanged));

    public static readonly DependencyProperty ShowDateTimeProperty = DependencyProperty.Register(
        nameof(ShowDateTime), typeof(bool), typeof(Growl), new PropertyMetadata(true));

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(Growl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        nameof(Time), typeof(DateTime), typeof(Growl), new PropertyMetadata(default(DateTime)));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(string), typeof(Growl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
        nameof(IconBrush), typeof(Brush), typeof(Growl), new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
        nameof(Type), typeof(GrowlType), typeof(Growl), new PropertyMetadata(GrowlType.Info));

    public static readonly DependencyProperty CancelStrProperty = DependencyProperty.Register(
        nameof(CancelStr), typeof(string), typeof(Growl), new PropertyMetadata("Cancel"));

    public static readonly DependencyProperty ConfirmStrProperty = DependencyProperty.Register(
        nameof(ConfirmStr), typeof(string), typeof(Growl), new PropertyMetadata("Confirm"));

    private static readonly DependencyProperty IsCreatedAutomaticallyProperty = DependencyProperty.RegisterAttached(
        "IsCreatedAutomatically", typeof(bool), typeof(Growl), new PropertyMetadata(false));

    private Panel? _panelMore;
    private Grid? _gridMain;
    private Button? _buttonClose;
    private bool _showCloseButton = true;
    private bool _staysOpen;
    private int _waitTime = 6;
    private int _tickCount;
    private DispatcherTimer? _timerClose;
    private Func<bool, bool>? ActionBeforeClose { get; set; }

    static Growl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Growl),
            new FrameworkPropertyMetadata(typeof(Growl)));
    }

    public Growl()
    {
        CommandBindings.Add(new CommandBinding(CloseCommand, (_, _) => Close(false)));
        CommandBindings.Add(new CommandBinding(CancelCommand, (_, _) => Close(false)));
        CommandBindings.Add(new CommandBinding(ConfirmCommand, (_, _) => Close(true)));
    }

    /// <summary>Default in-window panel used when no token is specified.</summary>
    public static Panel? GrowlPanel { get; set; }

    public GrowlType Type
    {
        get => (GrowlType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public bool ShowDateTime
    {
        get => (bool)GetValue(ShowDateTimeProperty);
        set => SetValue(ShowDateTimeProperty, value);
    }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public DateTime Time
    {
        get => (DateTime)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public string? Icon
    {
        get => (string?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Brush? IconBrush
    {
        get => (Brush?)GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    public string CancelStr
    {
        get => (string)GetValue(CancelStrProperty);
        set => SetValue(CancelStrProperty, value);
    }

    public string ConfirmStr
    {
        get => (string)GetValue(ConfirmStrProperty);
        set => SetValue(ConfirmStrProperty, value);
    }

    public static void SetToken(DependencyObject element, string? value) => element.SetValue(TokenProperty, value);
    public static string? GetToken(DependencyObject element) => (string?)element.GetValue(TokenProperty);

    public static void SetShowMode(DependencyObject element, GrowlShowMode value) => element.SetValue(ShowModeProperty, value);
    public static GrowlShowMode GetShowMode(DependencyObject element) => (GrowlShowMode)element.GetValue(ShowModeProperty);

    public static void SetTransitionMode(DependencyObject element, GrowlTransitionMode value) => element.SetValue(TransitionModeProperty, value);
    public static GrowlTransitionMode GetTransitionMode(DependencyObject element) => (GrowlTransitionMode)element.GetValue(TransitionModeProperty);

    public static void SetTransitionStoryboard(DependencyObject element, Storyboard? value) => element.SetValue(TransitionStoryboardProperty, value);
    public static Storyboard? GetTransitionStoryboard(DependencyObject element) => (Storyboard?)element.GetValue(TransitionStoryboardProperty);

    public static void SetGrowlParent(DependencyObject element, bool value) => element.SetValue(GrowlParentProperty, value);
    public static bool GetGrowlParent(DependencyObject element) => (bool)element.GetValue(GrowlParentProperty);

    private static void SetIsCreatedAutomatically(DependencyObject element, bool value) => element.SetValue(IsCreatedAutomaticallyProperty, value);
    private static bool GetIsCreatedAutomatically(DependencyObject element) => (bool)element.GetValue(IsCreatedAutomaticallyProperty);

    private static void OnGrowlParentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue && d is Panel panel)
        {
            GrowlPanel = panel;
            InitGrowlPanel(panel);
        }
    }

    private static void OnTokenRegistered(string token, Panel panel) => InitGrowlPanel(panel);

    private static void OnTokenUnregistered(string token, Panel panel) => panel.ContextMenu = null;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _panelMore = GetTemplateChild(ElementPanelMore) as Panel;
        _gridMain = GetTemplateChild(ElementGridMain) as Grid;
        _buttonClose = GetTemplateChild(ElementButtonClose) as Button;

        if (_panelMore is null || _gridMain is null || _buttonClose is null)
        {
            throw new InvalidOperationException("Growl template parts are missing.");
        }

        Visibility = Visibility.Collapsed;
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            Update();
            Visibility = Visibility.Visible;
        });
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (_buttonClose is not null)
        {
            _buttonClose.Visibility = _showCloseButton ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_buttonClose is not null)
        {
            _buttonClose.Visibility = Visibility.Collapsed;
        }
    }

    private void Update()
    {
        if (Type == GrowlType.Ask && _panelMore is not null)
        {
            _panelMore.IsEnabled = true;
            _panelMore.Visibility = Visibility.Visible;
        }

        StartTransition(false);

        if (!_staysOpen)
        {
            StartTimer();
        }
    }

    private void StartTimer()
    {
        _timerClose = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timerClose.Tick += (_, _) =>
        {
            if (IsMouseOver)
            {
                _tickCount = 0;
                return;
            }

            _tickCount++;
            if (_tickCount >= _waitTime)
            {
                Close(true);
            }
        };
        _timerClose.Start();
    }

    private static void InitGrowlPanel(Panel panel)
    {
        if (panel.ContextMenu is not null)
        {
            return;
        }

        var menuItem = new MenuItem { Header = "Clear" };
        menuItem.Click += (_, _) => Clear(panel);
        panel.ContextMenu = new ContextMenu { Items = { menuItem } };
    }

    private static void ShowInternal(Panel panel, Growl growl)
    {
        if (GetShowMode(panel) == GrowlShowMode.Prepend)
        {
            panel.Children.Insert(0, growl);
        }
        else
        {
            panel.Children.Add(growl);
        }
    }

    private static Growl CreateGrowl(GrowlInfo growlInfo)
    {
        return new Growl
        {
            Message = growlInfo.Message,
            Time = DateTime.Now,
            Icon = growlInfo.Icon,
            IconBrush = growlInfo.IconBrush,
            _showCloseButton = growlInfo.ShowCloseButton,
            ActionBeforeClose = growlInfo.ActionBeforeClose,
            _staysOpen = growlInfo.StaysOpen,
            ShowDateTime = growlInfo.ShowDateTime,
            ConfirmStr = growlInfo.ConfirmStr,
            CancelStr = growlInfo.CancelStr,
            Type = growlInfo.Type,
            _waitTime = Math.Max(growlInfo.WaitTime, MinWaitTime),
        };
    }

    private static void Show(GrowlInfo growlInfo)
    {
        (Application.Current?.Dispatcher ?? growlInfo.Dispatcher)?.Invoke(() =>
        {
            var ctl = CreateGrowl(growlInfo);

            if (!string.IsNullOrEmpty(growlInfo.Token))
            {
                if (TokenManager.TryGetControl(growlInfo.Token!, out var panel) && panel is not null)
                {
                    ShowInternal(panel, ctl);
                }

                return;
            }

            GrowlPanel ??= CreateDefaultPanel();
            if (GrowlPanel is null)
            {
                return;
            }

            ShowInternal(GrowlPanel, ctl);

            var transitionMode = GetTransitionMode(GrowlPanel);
            GrowlPanel.VerticalAlignment = GetPanelVerticalAlignment(transitionMode);
            GrowlPanel.HorizontalAlignment = GetPanelHorizontalAlignment(transitionMode);
        });
    }

    private static void ShowGlobal(GrowlInfo growlInfo)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            if (s_growlWindow is null)
            {
                s_growlWindow = new GrowlWindow();
                s_growlWindow.Show();
                InitGrowlPanel(s_growlWindow.GrowlPanel);
            }

            var transitionMode = Application.Current.MainWindow is { } main
                ? GetTransitionMode(main)
                : GrowlTransitionMode.Right2LeftWithFade;

            s_growlWindow.UpdatePosition(transitionMode);
            s_growlWindow.Visibility = Visibility.Visible;

            ShowInternal(s_growlWindow.GrowlPanel, CreateGrowl(growlInfo));
        });
    }

    private static Panel? CreateDefaultPanel()
    {
        var window = SharedHelpers.GetActiveWindow() ?? Application.Current?.MainWindow;
        if (window is null)
        {
            return null;
        }

        window.Closed += (_, _) => Clear(GrowlPanel);

        if (window.Content is not UIElement root)
        {
            return null;
        }

        var layer = AdornerLayer.GetAdornerLayer(root) ?? FindAdornerLayer(root);
        if (layer is null)
        {
            return null;
        }

        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
        };

        InitGrowlPanel(panel);
        SetIsCreatedAutomatically(panel, true);

        // Null background on the host Grid => empty regions are not hit-testable.
        var host = new Grid { Children = { panel } };
        s_defaultAdorner = new GrowlAdorner(root, host);
        layer.Add(s_defaultAdorner);

        return panel;
    }

    private static AdornerLayer? FindAdornerLayer(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is AdornerDecorator decorator)
            {
                return decorator.AdornerLayer;
            }

            var layer = FindAdornerLayer(child);
            if (layer is not null)
            {
                return layer;
            }
        }

        return root is Visual visual ? AdornerLayer.GetAdornerLayer(visual) : null;
    }

    private static void RemoveDefaultPanel(Panel panel)
    {
        if (s_defaultAdorner is null)
        {
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(s_defaultAdorner.AdornedElement);
        layer?.Remove(s_defaultAdorner);
        s_defaultAdorner = null;
    }

    private static void InitGrowlInfo(GrowlInfo growlInfo, GrowlType infoType)
    {
        if (growlInfo is null)
        {
            throw new ArgumentNullException(nameof(growlInfo));
        }

        growlInfo.Type = infoType;

        switch (infoType)
        {
            case GrowlType.Success:
                ApplyIcon(growlInfo, Wpf.Ui.Controls.FontSymbols.Accept, SuccessBrush);
                break;
            case GrowlType.Info:
                ApplyIcon(growlInfo, Wpf.Ui.Controls.FontSymbols.Info, InfoBrush);
                break;
            case GrowlType.Warning:
                ApplyIcon(growlInfo, Wpf.Ui.Controls.FontSymbols.Warning, WarningBrush);
                break;
            case GrowlType.Error:
                ApplyIcon(growlInfo, Wpf.Ui.Controls.FontSymbols.Cancel, ErrorBrush);
                if (!growlInfo.IsCustom)
                {
                    growlInfo.StaysOpen = true;
                }
                break;
            case GrowlType.Fatal:
                ApplyIcon(growlInfo, Wpf.Ui.Controls.FontSymbols.Cancel, FatalBrush);
                if (!growlInfo.IsCustom)
                {
                    growlInfo.StaysOpen = true;
                    growlInfo.ShowCloseButton = false;
                }
                break;
            case GrowlType.Ask:
                growlInfo.StaysOpen = true;
                growlInfo.ShowCloseButton = false;
                ApplyIcon(growlInfo, Wpf.Ui.Controls.FontSymbols.Unknown, InfoBrush);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(infoType), infoType, null);
        }
    }

    private static void ApplyIcon(GrowlInfo info, string glyph, Brush brush)
    {
        if (!info.IsCustom)
        {
            info.Icon = glyph;
            info.IconBrush = brush;
        }
        else
        {
            info.Icon ??= glyph;
            info.IconBrush ??= brush;
        }
    }

    public static void Success(string message, string token = "") => Success(new GrowlInfo { Message = message, Token = token });
    public static void Success(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Success); Show(growlInfo); }
    public static void SuccessGlobal(string message) => SuccessGlobal(new GrowlInfo { Message = message });
    public static void SuccessGlobal(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Success); ShowGlobal(growlInfo); }

    public static void Info(string message, string token = "") => Info(new GrowlInfo { Message = message, Token = token });
    public static void Info(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Info); Show(growlInfo); }
    public static void InfoGlobal(string message) => InfoGlobal(new GrowlInfo { Message = message });
    public static void InfoGlobal(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Info); ShowGlobal(growlInfo); }

    public static void Warning(string message, string token = "") => Warning(new GrowlInfo { Message = message, Token = token });
    public static void Warning(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Warning); Show(growlInfo); }
    public static void WarningGlobal(string message) => WarningGlobal(new GrowlInfo { Message = message });
    public static void WarningGlobal(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Warning); ShowGlobal(growlInfo); }

    public static void Error(string message, string token = "") => Error(new GrowlInfo { Message = message, Token = token });
    public static void Error(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Error); Show(growlInfo); }
    public static void ErrorGlobal(string message) => ErrorGlobal(new GrowlInfo { Message = message });
    public static void ErrorGlobal(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Error); ShowGlobal(growlInfo); }

    public static void Fatal(string message, string token = "") => Fatal(new GrowlInfo { Message = message, Token = token });
    public static void Fatal(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Fatal); Show(growlInfo); }
    public static void FatalGlobal(string message) => FatalGlobal(new GrowlInfo { Message = message });
    public static void FatalGlobal(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Fatal); ShowGlobal(growlInfo); }

    public static void Ask(string message, Func<bool, bool> actionBeforeClose, string token = "")
        => Ask(new GrowlInfo { Message = message, ActionBeforeClose = actionBeforeClose, Token = token });

    public static void Ask(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Ask); Show(growlInfo); }

    public static void AskGlobal(string message, Func<bool, bool> actionBeforeClose)
        => AskGlobal(new GrowlInfo { Message = message, ActionBeforeClose = actionBeforeClose });

    public static void AskGlobal(GrowlInfo growlInfo) { InitGrowlInfo(growlInfo, GrowlType.Ask); ShowGlobal(growlInfo); }

    private void Close(bool invokeParam, bool isClear = false)
    {
        if (!isClear && ActionBeforeClose?.Invoke(invokeParam) == false)
        {
            return;
        }

        _timerClose?.Stop();
        Panel.SetZIndex(this, int.MinValue);
        StartTransition(true, OnStoryboardCompleted);
        return;

        void OnStoryboardCompleted()
        {
            if (Parent is not Panel panel)
            {
                return;
            }

            panel.Children.Remove(this);

            if (s_growlWindow is not null)
            {
                if (s_growlWindow.GrowlPanel.Children.Count != 0)
                {
                    return;
                }

                s_growlWindow.Close();
                s_growlWindow = null;
                return;
            }

            if (GrowlPanel is not { Children.Count: 0 } || !GetIsCreatedAutomatically(GrowlPanel))
            {
                return;
            }

            RemoveDefaultPanel(GrowlPanel);
            GrowlPanel = null;
        }
    }

    public static void Clear(string token = "")
    {
        if (!string.IsNullOrEmpty(token))
        {
            if (TokenManager.TryGetControl(token, out var panel) && panel is not null)
            {
                Clear(panel);
            }
        }
        else
        {
            Clear(GrowlPanel);
        }
    }

    private static void Clear(Panel? panel)
    {
        if (panel is null)
        {
            return;
        }

        // Close with animation skipped — clear immediately.
        panel.Children.Clear();

        if (ReferenceEquals(panel, GrowlPanel) && GetIsCreatedAutomatically(panel))
        {
            RemoveDefaultPanel(panel);
            GrowlPanel = null;
        }
    }

    public static void ClearGlobal()
    {
        if (s_growlWindow is null)
        {
            return;
        }

        Clear(s_growlWindow.GrowlPanel);
        s_growlWindow.Close();
        s_growlWindow = null;
    }

    private void StartTransition(bool isClose, Action? completed = null)
    {
        if (_gridMain is null)
        {
            completed?.Invoke();
            return;
        }

        var mode = GetTransitionMode(this);
        var actualStoryboard = GetTransitionStoryboard(this) ?? CreateStoryboard(isClose, mode);
        if (actualStoryboard is null)
        {
            completed?.Invoke();
            return;
        }

        if (completed is not null)
        {
            void OnCompleted(object? s, EventArgs e)
            {
                actualStoryboard.Completed -= OnCompleted;
                completed();
            }

            actualStoryboard.Completed += OnCompleted;
        }

        actualStoryboard.Begin();
    }

    private Storyboard CreateStoryboard(bool isClose, GrowlTransitionMode transitionMode)
    {
        var transformLength = GetTransformLength(isClose, transitionMode);
        var transformAnimation = CreateTransformAnimation(isClose, transitionMode, transformLength);
        var storyboard = new Storyboard { Duration = transformAnimation.Duration };

        if (transitionMode is not GrowlTransitionMode.Fade)
        {
            _gridMain!.RenderTransform = CreateRenderTransform(isClose, transitionMode, transformLength);
            Storyboard.SetTarget(transformAnimation, _gridMain);
            storyboard.Children.Add(transformAnimation);
        }

        if (CreateFadeAnimation(isClose, transitionMode) is { } fadeAnimation)
        {
            Storyboard.SetTarget(fadeAnimation, _gridMain!);
            storyboard.Children.Add(fadeAnimation);
        }

        return storyboard;
    }

    private double GetTransformLength(bool isClose, GrowlTransitionMode transitionMode)
    {
        var length = transitionMode switch
        {
            GrowlTransitionMode.Right2Left or GrowlTransitionMode.Right2LeftWithFade => ActualWidth,
            GrowlTransitionMode.Left2Right or GrowlTransitionMode.Left2RightWithFade => -ActualWidth,
            GrowlTransitionMode.Bottom2Top or GrowlTransitionMode.Bottom2TopWithFade => ActualHeight,
            GrowlTransitionMode.Top2Bottom or GrowlTransitionMode.Top2BottomWithFade => -ActualHeight,
            _ => ActualWidth,
        };

        return isClose ? -length : length;
    }

    private static TransformGroup CreateOriginalTransform() => new()
    {
        Children =
        {
            new ScaleTransform(),
            new SkewTransform(),
            new RotateTransform(),
            new TranslateTransform(),
        },
    };

    private static Transform CreateRenderTransform(bool isClose, GrowlTransitionMode transitionMode, double transformLength)
    {
        var transformGroup = CreateOriginalTransform();
        if (isClose)
        {
            return transformGroup;
        }

        switch (GetOrientation(transitionMode))
        {
            case Orientation.Horizontal:
                ((TranslateTransform)transformGroup.Children[TranslateTransformIndex]).X = transformLength;
                break;
            case Orientation.Vertical:
                ((TranslateTransform)transformGroup.Children[TranslateTransformIndex]).Y = transformLength;
                break;
        }

        return transformGroup;
    }

    private static DoubleAnimation CreateTransformAnimation(bool isClose, GrowlTransitionMode transitionMode, double transformLength)
    {
        var animation = new DoubleAnimation
        {
            To = isClose ? -transformLength : 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut },
        };

        var path = GetOrientation(transitionMode) == Orientation.Vertical
            ? $"(UIElement.RenderTransform).(TransformGroup.Children)[{TranslateTransformIndex}].(TranslateTransform.Y)"
            : $"(UIElement.RenderTransform).(TransformGroup.Children)[{TranslateTransformIndex}].(TranslateTransform.X)";

        Storyboard.SetTargetProperty(animation, new PropertyPath(path));
        return animation;
    }

    private static DoubleAnimation? CreateFadeAnimation(bool isClose, GrowlTransitionMode transitionMode)
    {
        if (transitionMode is GrowlTransitionMode.Right2Left or GrowlTransitionMode.Left2Right
            or GrowlTransitionMode.Bottom2Top or GrowlTransitionMode.Top2Bottom)
        {
            return null;
        }

        var animation = new DoubleAnimation
        {
            From = isClose ? 1 : 0,
            To = isClose ? 0 : 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut },
        };
        Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
        return animation;
    }

    private static Orientation GetOrientation(GrowlTransitionMode transitionMode) => transitionMode switch
    {
        GrowlTransitionMode.Bottom2Top or GrowlTransitionMode.Bottom2TopWithFade
            or GrowlTransitionMode.Top2Bottom or GrowlTransitionMode.Top2BottomWithFade => Orientation.Vertical,
        _ => Orientation.Horizontal,
    };

    internal static VerticalAlignment GetPanelVerticalAlignment(GrowlTransitionMode transitionMode) => transitionMode switch
    {
        GrowlTransitionMode.Bottom2Top or GrowlTransitionMode.Bottom2TopWithFade => VerticalAlignment.Bottom,
        GrowlTransitionMode.Top2Bottom or GrowlTransitionMode.Top2BottomWithFade => VerticalAlignment.Top,
        _ => VerticalAlignment.Top,
    };

    internal static HorizontalAlignment GetPanelHorizontalAlignment(GrowlTransitionMode transitionMode) => transitionMode switch
    {
        GrowlTransitionMode.Right2Left or GrowlTransitionMode.Right2LeftWithFade => HorizontalAlignment.Right,
        GrowlTransitionMode.Left2Right or GrowlTransitionMode.Left2RightWithFade => HorizontalAlignment.Left,
        GrowlTransitionMode.Bottom2Top or GrowlTransitionMode.Bottom2TopWithFade
            or GrowlTransitionMode.Top2Bottom or GrowlTransitionMode.Top2BottomWithFade => HorizontalAlignment.Center,
        _ => HorizontalAlignment.Right,
    };

    private static SolidColorBrush CreateFrozenBrush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }
}
