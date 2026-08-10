using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

[TemplatePart(Name = nameof(PART_TitleBar), Type = typeof(TitleBar))]
[TemplatePart(Name = nameof(PART_ContentPresenter), Type = typeof(System.Windows.Controls.ContentPresenter))]
public partial class ContentWindow : ShellWindow
{
    public event EventHandler<ContentWindowResultEventArgs>? ResultCommandExecuted = null;

    private TaskCompletionSource<ContentWindowResult>? showTcs;

    public virtual bool ResultNeverSet { get; protected set; } = true;

    public ContentWindowResult Result
    {
        get => field;
        internal set
        {
            field = value;
            ResultNeverSet = false;
            if (!IsOnClosing && !IsOnClosed) Close();
        }
    } = ContentWindowResult.None;

    internal bool IsOnClosing { get; private set; } = false;

    internal bool IsOnClosed { get; private set; } = false;

    public bool CanKeyDownResult { get; set; } = false;

    public ContentWindowResult AcceptResult { get; set; } = ContentWindowResult.OK;

    public ContentWindowResult CancelResult { get; set; } = ContentWindowResult.Cancel;

    /// <summary>
    /// Gets the embedded <see cref="TitleBar"/> after the template is applied.
    /// </summary>
    public TitleBar? TitleBar { get; private set; }

    public static readonly DependencyProperty ControlProperty =
        DependencyProperty.Register(
            nameof(Control),
            typeof(ContentWindowControl),
            typeof(ContentWindow),
            new PropertyMetadata(null, OnControlChanged));

    public ContentWindowControl? Control
    {
        get => (ContentWindowControl?)GetValue(ControlProperty);
        set => SetValue(ControlProperty, value);
    }

    public static readonly DependencyProperty BackButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(BackButtonVisibility),
            typeof(Visibility),
            typeof(ContentWindow),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility BackButtonVisibility
    {
        get => (Visibility)GetValue(BackButtonVisibilityProperty);
        set => SetValue(BackButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty PaneToggleButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(PaneToggleButtonVisibility),
            typeof(Visibility),
            typeof(ContentWindow),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility PaneToggleButtonVisibility
    {
        get => (Visibility)GetValue(PaneToggleButtonVisibilityProperty);
        set => SetValue(PaneToggleButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MinimizeButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MinimizeButtonVisibility),
            typeof(Visibility),
            typeof(ContentWindow),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility MinimizeButtonVisibility
    {
        get => (Visibility)GetValue(MinimizeButtonVisibilityProperty);
        set => SetValue(MinimizeButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MaximizeButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(MaximizeButtonVisibility),
            typeof(Visibility),
            typeof(ContentWindow),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility MaximizeButtonVisibility
    {
        get => (Visibility)GetValue(MaximizeButtonVisibilityProperty);
        set => SetValue(MaximizeButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty CloseButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(CloseButtonVisibility),
            typeof(Visibility),
            typeof(ContentWindow),
            new PropertyMetadata(Visibility.Visible));

    public Visibility CloseButtonVisibility
    {
        get => (Visibility)GetValue(CloseButtonVisibilityProperty);
        set => SetValue(CloseButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty HelpButtonVisibilityProperty =
        DependencyProperty.Register(
            nameof(HelpButtonVisibility),
            typeof(Visibility),
            typeof(ContentWindow),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility HelpButtonVisibility
    {
        get => (Visibility)GetValue(HelpButtonVisibilityProperty);
        set => SetValue(HelpButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty IsBackButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsBackButtonEnabled),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    public bool IsBackButtonEnabled
    {
        get => (bool)GetValue(IsBackButtonEnabledProperty);
        set => SetValue(IsBackButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsPaneToggleButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsPaneToggleButtonEnabled),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    public bool IsPaneToggleButtonEnabled
    {
        get => (bool)GetValue(IsPaneToggleButtonEnabledProperty);
        set => SetValue(IsPaneToggleButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsMinimizeButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMinimizeButtonEnabled),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    public bool IsMinimizeButtonEnabled
    {
        get => (bool)GetValue(IsMinimizeButtonEnabledProperty);
        set => SetValue(IsMinimizeButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsMaximizeButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsMaximizeButtonEnabled),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    public bool IsMaximizeButtonEnabled
    {
        get => (bool)GetValue(IsMaximizeButtonEnabledProperty);
        set => SetValue(IsMaximizeButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsCloseButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsCloseButtonEnabled),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    public bool IsCloseButtonEnabled
    {
        get => (bool)GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty IsHelpButtonEnabledProperty =
        DependencyProperty.Register(
            nameof(IsHelpButtonEnabled),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    public bool IsHelpButtonEnabled
    {
        get => (bool)GetValue(IsHelpButtonEnabledProperty);
        set => SetValue(IsHelpButtonEnabledProperty, value);
    }

    public static readonly DependencyProperty TitleBarHeaderProperty =
        DependencyProperty.Register(
            nameof(TitleBarHeader),
            typeof(object),
            typeof(ContentWindow),
            new PropertyMetadata(null));

    /// <summary>
    /// Custom header content for the embedded <see cref="TitleBar"/>.
    /// When null, the window <see cref="Window.Title"/> is shown.
    /// </summary>
    public object? TitleBarHeader
    {
        get => GetValue(TitleBarHeaderProperty);
        set => SetValue(TitleBarHeaderProperty, value);
    }

    public static readonly DependencyProperty TitleBarFooterProperty =
        DependencyProperty.Register(
            nameof(TitleBarFooter),
            typeof(object),
            typeof(ContentWindow),
            new PropertyMetadata(null));

    /// <summary>
    /// Custom footer content for the embedded <see cref="TitleBar"/> (left of caption buttons).
    /// </summary>
    public object? TitleBarFooter
    {
        get => GetValue(TitleBarFooterProperty);
        set => SetValue(TitleBarFooterProperty, value);
    }

    public static readonly DependencyProperty TitleBarContentProperty =
        DependencyProperty.Register(
            nameof(TitleBarContent),
            typeof(object),
            typeof(ContentWindow),
            new PropertyMetadata(null));

    /// <summary>
    /// Center content of the embedded <see cref="TitleBar"/>.
    /// </summary>
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    public static readonly DependencyProperty TitleBarVisibilityProperty =
        DependencyProperty.Register(
            nameof(TitleBarVisibility),
            typeof(Visibility),
            typeof(ContentWindow),
            new PropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Gets or sets the visibility of the embedded <see cref="TitleBar"/>.
    /// </summary>
    public Visibility TitleBarVisibility
    {
        get => (Visibility)GetValue(TitleBarVisibilityProperty);
        set => SetValue(TitleBarVisibilityProperty, value);
    }

    public static readonly DependencyProperty InheritIconFromOwnerProperty =
        DependencyProperty.Register(
            nameof(InheritIconFromOwner),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    /// <summary>
    /// When true (default), copies <see cref="Window.Icon"/> from <see cref="Window.Owner"/>
    /// if this window has no icon of its own.
    /// </summary>
    public bool InheritIconFromOwner
    {
        get => (bool)GetValue(InheritIconFromOwnerProperty);
        set => SetValue(InheritIconFromOwnerProperty, value);
    }

    public static readonly DependencyProperty IsIconVisibleProperty =
        DependencyProperty.Register(
            nameof(IsIconVisible),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether the embedded <see cref="TitleBar"/> shows <see cref="Window.Icon"/>.
    /// </summary>
    public bool IsIconVisible
    {
        get => (bool)GetValue(IsIconVisibleProperty);
        set => SetValue(IsIconVisibleProperty, value);
    }

    public static readonly DependencyProperty IsTitleVisibleProperty =
        DependencyProperty.Register(
            nameof(IsTitleVisible),
            typeof(bool),
            typeof(ContentWindow),
            new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets whether the embedded <see cref="TitleBar"/> shows the window title text.
    /// </summary>
    public bool IsTitleVisible
    {
        get => (bool)GetValue(IsTitleVisibleProperty);
        set => SetValue(IsTitleVisibleProperty, value);
    }

    static ContentWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentWindow), new FrameworkPropertyMetadata(typeof(ContentWindow)));
    }

    public ContentWindow()
    {
        SetResourceReference(StyleProperty, typeof(ContentWindow));
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        Loaded += OnLoaded;

        KeyDown += (_, e) =>
        {
            if (!CanKeyDownResult)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    OnResultCommandExecuted(AcceptResult);
                    break;

                case Key.Escape:
                    OnResultCommandExecuted(CancelResult);
                    break;
            }
        };

        Closing += (_, e) =>
        {
            IsOnClosing = true;
            if (ResultNeverSet)
            {
                OnResultCommandExecuted(CancelResult);

                if (ResultNeverSet)
                {
                    e.Cancel = true;
                }
            }
            IsOnClosing = false;
        };

        Closed += (_, _) =>
        {
            IsOnClosed = true;
            if (ResultNeverSet)
            {
                OnResultCommandExecuted(Result = CancelResult);
            }
            showTcs?.TrySetResult(Result);
            showTcs = null;
            IsOnClosed = true;
        };
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_TitleBar = GetTemplateChild(nameof(PART_TitleBar)) as TitleBar;
        PART_ContentPresenter = GetTemplateChild(nameof(PART_ContentPresenter)) as System.Windows.Controls.ContentPresenter;
        TitleBar = PART_TitleBar;
    }

    /// <summary>
    /// Copies the owner window icon when <see cref="InheritIconFromOwner"/> is enabled
    /// and this window does not already have an icon.
    /// </summary>
    public void TryInheritIconFromOwner()
    {
        if (!InheritIconFromOwner || Icon is not null)
        {
            return;
        }

        if (Owner?.Icon is { } ownerIcon)
        {
            Icon = ownerIcon;
        }
    }

    public virtual void OnResultCommandExecuted(ContentWindowResult result)
    {
        ContentWindowResultEventArgs e = new(result);
        ResultCommandExecuted?.Invoke(this, e);

        if (e.Handled) return;

        Result = result;
        showTcs?.TrySetResult(Result);
        showTcs = null;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TryInheritIconFromOwner();
    }

    private static void OnControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (ContentWindow)d;

        if (e.NewValue is ContentWindowControl control)
        {
            control.Owner = window;
            window.Content = control;

            if (!string.IsNullOrEmpty(control.Title))
            {
                window.Title = control.Title;
            }
        }
        else if (e.OldValue is not null && window.Content == e.OldValue)
        {
            window.Content = null;
        }
    }

#pragma warning disable IDE1006 // Naming Styles — template part names
    private TitleBar? PART_TitleBar;
    private System.Windows.Controls.ContentPresenter? PART_ContentPresenter;
#pragma warning restore IDE1006
}

public partial class ContentWindow
{
    public static ContentWindow Create<T>() where T : ContentWindowControl, new()
        => Create<T>(out _);

    public static ContentWindow Create<T>(out T? dialogControl) where T : ContentWindowControl, new()
    {
        var control = (T)Activator.CreateInstance(typeof(T))!;
        var dialog = new ContentWindow
        {
            Control = control,
        };
        dialogControl = control;
        return dialog;
    }

    public static ContentWindow Create<T>(T control) where T : ContentWindowControl
    {
        return new ContentWindow
        {
            Control = control,
        };
    }

    public static ContentWindowResult ShowDialog<T>(DependencyObject d, out T? dialogControl) where T : ContentWindowControl, new()
    {
        ContentWindow window = Create<T>(out dialogControl);

        window.Owner = GetWindow(d);
        window.TryInheritIconFromOwner();
        _ = window.ShowDialog();
        return window.Result;
    }

    public static ContentWindowResult ShowDialog<T>(out T? dialogControl) where T : ContentWindowControl, new()
    {
        ContentWindow window = Create<T>(out dialogControl);

        window.TryInheritIconFromOwner();
        _ = window.ShowDialog();
        return window.Result;
    }

    public static ContentWindowResult ShowDialog<T>() where T : ContentWindowControl, new()
    {
        ContentWindow window = Create<T>(out _);

        window.TryInheritIconFromOwner();
        _ = window.ShowDialog();
        return window.Result;
    }
}
