using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wpf.Ui.Violeta.Attached.DragDrop;

/// <summary>
/// Data presented in drop hint adorner.
/// </summary>
public class DropHintData : INotifyPropertyChanged
{
    private DropHintState hintState;
    private string hintText;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public DropHintData(DropHintState hintState, string hintText)
    {
        this.HintState = hintState;
        this.HintText = hintText;
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// The hint text to display to the user. See <see cref="IDropInfo.DropHintText"/>
    /// and <see cref="IDropHintInfo.DropHintText"/>.
    /// </summary>
    public string HintText
    {
        get => this.hintText;
        set
        {
            if (value == this.hintText) return;
            this.hintText = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// The hint state to display different colors for hints. See <see cref="IDropInfo.DropTargetHintState"/>
    /// and <see cref="IDropHintInfo.DropTargetHintState"/>.
    /// </summary>
    public DropHintState HintState
    {
        get => this.hintState;
        set
        {
            if (value == this.hintState) return;
            this.hintState = value;
            this.OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
