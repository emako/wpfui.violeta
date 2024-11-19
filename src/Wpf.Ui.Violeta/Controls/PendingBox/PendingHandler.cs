using System;
using System.Diagnostics.CodeAnalysis;

namespace Wpf.Ui.Violeta.Controls;

public class PendingHandler(PendingBoxDialog pendingBoxDialog) : IPendingHandler
{
    public PendingBoxDialog Dialog { get; protected set; } = pendingBoxDialog;

    public event EventHandler? Closed;

    public event EventHandler? Cancel;

    public virtual string? Message
    {
        get => Dialog.Dispatcher.Invoke(() => Dialog.Message);
        set => Dialog.Dispatcher.Invoke(() => Dialog.Message = value);
    }

    public virtual bool Cancelable
    {
        get => Dialog.Dispatcher.Invoke(() => Dialog.IsShowCancel);
        set => Dialog.Dispatcher.Invoke(() => Dialog.IsShowCancel = value);
    }

    public virtual bool Canceled { get; protected set; } = false;

    public virtual bool CloseOnCanceled { get; set; } = true;

    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    public virtual void Dispose()
    {
        try
        {
            Close();
        }
        catch
        {
            ///
        }
    }

    public virtual void Show()
        => Dialog?.Show();

    [Obsolete("Use Show instead")]
    public virtual bool? ShowDialog()
        => Dialog?.ShowDialog();

    public virtual void Close()
    {
        Dialog?.Dispatcher.Invoke(() =>
        {
            if (Dialog != null)
            {
                Dialog.IsClosedByHandler = true;
                Dialog.Close();
            }
        });
    }

    public virtual void RaiseClosedEvent(object? sender, EventArgs e)
        => Closed?.Invoke(sender, e);

    public virtual void RaiseCanceledEvent(object? sender, EventArgs e)
    {
        Cancel?.Invoke(sender, e);
        if (CloseOnCanceled)
        {
            Close();
        }
        Canceled = true;
    }
}

public class PendingHandlerAsync(PendingBoxDialog pendingBoxDialog) : PendingHandler(pendingBoxDialog);
