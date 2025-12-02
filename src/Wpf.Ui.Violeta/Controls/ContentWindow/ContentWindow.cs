using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

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

    public ContentWindowControl Control
    {
        get => field;
        set
        {
            field = value;
            if (field != null)
            {
                //contentPresenter.Content = value;
            }
        }
    } = null!;

    static ContentWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentWindow), new FrameworkPropertyMetadata(typeof(ContentWindow)));
    }

    public ContentWindow()
    {
        MouseMove += (s, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };

        KeyDown += (s, e) =>
        {
            if (CanKeyDownResult)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        OnResultCommandExecuted(AcceptResult);
                        break;

                    case Key.Escape:
                        OnResultCommandExecuted(CancelResult);
                        break;
                }
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

    public virtual void OnResultCommandExecuted(ContentWindowResult result)
    {
        ContentWindowResultEventArgs e = new(result);
        ResultCommandExecuted?.Invoke(this, e);

        if (e.Handled) return;

        Result = result;
        showTcs?.TrySetResult(Result);
        showTcs = null;
    }
}

public partial class ContentWindow
{
    public static ContentWindow Create<T>() where T : ContentWindowControl, new()
        => Create<T>(out _);

    public static ContentWindow Create<T>(out T? dialogControl) where T : ContentWindowControl, new()
    {
        var dialog = new ContentWindow()
        {
            Control = (T)Activator.CreateInstance(typeof(T))!,
        };
        if (dialog.Control is T control)
        {
            control.Owner = dialog;
            dialog.Title = control.Title;
            dialogControl = control;
        }
        else
        {
            dialogControl = null;
        }
        return dialog;
    }

    public static ContentWindow Create<T>(T control) where T : ContentWindowControl
    {
        var dialog = new ContentWindow()
        {
            Control = control,
        };
        control.Owner = dialog;
        dialog.Title = control.Title;
        return dialog;
    }

    public static ContentWindowResult ShowDialog<T>(DependencyObject d, out T? dialogControl) where T : ContentWindowControl, new()
    {
        ContentWindow window = Create<T>(out dialogControl);

        window.Owner = GetWindow(d);
        _ = window.ShowDialog();
        return window.Result;
    }

    public static ContentWindowResult ShowDialog<T>(out T? dialogControl) where T : ContentWindowControl, new()
    {
        ContentWindow window = Create<T>(out dialogControl);

        _ = window.ShowDialog();
        return window.Result;
    }

    public static ContentWindowResult ShowDialog<T>() where T : ContentWindowControl, new()
    {
        ContentWindow window = Create<T>(out _);

        _ = window.ShowDialog();
        return window.Result;
    }
}
