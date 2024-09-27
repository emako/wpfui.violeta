using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Controls;

public static class ContentDialogHostService
{
    public const string HostName = "PART_ContentPresenterForDialogs";

    public static bool IsPreferMainWindow { get; set; } = true;

    public static ContentPresenter? ContentPresenterForDialogs
    {
        get
        {
            if (IsPreferMainWindow && Application.Current.MainWindow != null)
            {
                return Application.Current.MainWindow.Dispatcher.Invoke(() => AttachContentDialogHost(Application.Current.MainWindow));
            }
            else
            {
                Window? window = Application.Current.Windows.OfType<Window>()
                    .Where(win => win.IsActive)
                    .FirstOrDefault();

                if (window != null)
                {
                    return window.Dispatcher.Invoke(() => AttachContentDialogHost(window));
                }
            }

            // Throw InvalidOperationException if ContentDialog::ShowAsync is called with a null DialogHost.
            return null;

            static ContentPresenter? AttachContentDialogHost(Window window)
            {
                if (window.Content is System.Windows.Controls.Grid rootGrid)
                {
                    if (rootGrid.ColumnDefinitions.Count != 0)
                    {
                        throw new InvalidOperationException("The root Grid's ColumnDefinitions in the Window should be empty.");
                    }
                    if (rootGrid.RowDefinitions.Count != 0)
                    {
                        throw new InvalidOperationException("The root Grid's RowDefinitions in the Window should be empty.");
                    }

                    if (window.FindName(HostName) is ContentPresenter _host)
                    {
                        return _host;
                    }
                    else
                    {
                        ContentPresenter host = new() { Name = HostName };
                        rootGrid.Children.Add(host);
                        window.RegisterName(HostName, host);
                        return host;
                    }
                }
                else
                {
                    throw new InvalidOperationException("The root element of the Window must be of type Grid.");
                }
            }
        }
    }
}
