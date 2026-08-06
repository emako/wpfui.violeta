using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Hosts the selectable swatches inside a <see cref="SwatchPicker"/> popup.
/// </summary>
public class SwatchPickerPresenter : ListBox
{
    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(SwatchPickerPresenter), new PropertyMetadata(8));

    public static readonly DependencyProperty FocusModeProperty =
        DependencyProperty.Register(nameof(FocusMode), typeof(SwatchPickerFocusMode), typeof(SwatchPickerPresenter), new PropertyMetadata(SwatchPickerFocusMode.Arrow));

    static SwatchPickerPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SwatchPickerPresenter), new FrameworkPropertyMetadata(typeof(SwatchPickerPresenter)));
    }

    public int ColumnCount
    {
        get => (int)GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    public SwatchPickerFocusMode FocusMode
    {
        get => (SwatchPickerFocusMode)GetValue(FocusModeProperty);
        set => SetValue(FocusModeProperty, value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || FocusMode != SwatchPickerFocusMode.Arrow)
            return;

        var offset = e.Key switch
        {
            Key.Left => -1,
            Key.Right => 1,
            Key.Up => -System.Math.Max(1, ColumnCount),
            Key.Down => System.Math.Max(1, ColumnCount),
            _ => 0
        };

        if (offset == 0 || Items.Count == 0)
            return;

        var start = SelectedIndex >= 0 ? SelectedIndex : 0;
        var direction = offset > 0 ? 1 : -1;
        var index = start;
        for (var attempt = 0; attempt < Items.Count; attempt++)
        {
            index = (index + offset) % Items.Count;
            if (index < 0)
                index += Items.Count;

            if (ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem item && item.IsEnabled)
            {
                SetCurrentValue(SelectedIndexProperty, index);
                item.Focus();
                e.Handled = true;
                return;
            }

            offset = direction;
        }
    }
}
