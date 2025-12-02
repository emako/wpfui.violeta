using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

public class ContentWindowControl : UserControl, IContentWindowControl
{
    public ContentWindow Owner
    {
        get => field;
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

    public string Title { get; set; } = string.Empty;

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
