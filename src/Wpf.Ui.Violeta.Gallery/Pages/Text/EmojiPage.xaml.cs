using Wpf.Ui.Emoji;

namespace Wpf.Ui.Violeta.Gallery.Pages.Text;

public partial class EmojiPage : Wpf.Ui.Violeta.Controls.Page
{
    public EmojiPage()
    {
        InitializeComponent();
    }

    private void OnEmojiPicked(object sender, EmojiPickedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Emoji) || Editor is null)
        {
            return;
        }

        var caret = Editor.CaretIndex;
        Editor.Text = Editor.Text.Insert(caret, e.Emoji);
        Editor.CaretIndex = caret + e.Emoji.Length;
        Editor.Focus();
    }
}
