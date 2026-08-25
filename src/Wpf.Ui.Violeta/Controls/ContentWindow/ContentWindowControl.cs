using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class ContentWindowControl : UserControl, IContentWindowControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ContentWindowControl),
            new PropertyMetadata(string.Empty, OnTitleChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContentWindowControl { Owner: { } window })
        {
            window.Title = (string)e.NewValue;
        }
    }

    public ContentWindow Owner
    {
        get;
        set
        {
            if (value != null && field != value)
            {
                field = value;
                field.ResultCommandExecuted -= ResultCommandExecuted;
                field.ResultCommandExecuted += ResultCommandExecuted;
            }
        }
    } = null!;

    public ContentWindowControl()
    {
        Tag = GetType().Name;
    }

    protected virtual void ResultCommandExecuted(object? sender, ContentWindowResultEventArgs e)
    {
    }

    public void Close()
    {
        Owner.Close();
    }
}

public interface IContentWindowControl
{
    public ContentWindow Owner { get; set; }

    public string Title { get; set; }
}
