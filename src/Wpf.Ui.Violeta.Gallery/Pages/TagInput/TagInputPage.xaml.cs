using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace Wpf.Ui.Violeta.Gallery.Pages.TagInput;

public partial class TagInputPage : Wpf.Ui.Violeta.Controls.Page
{
    private readonly ObservableCollection<string> _basicTags = [];
    private readonly ObservableCollection<string> _separatorTags = [];
    private readonly ObservableCollection<string> _maxCountTags = [];
    private readonly ObservableCollection<string> _prefilledTags = ["WPF", "Fluent", "UI"];

    public TagInputPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TagInputBasic.Tags = _basicTags;
        TagInputSeparator.Tags = _separatorTags;
        TagInputMaxCount.Tags = _maxCountTags;
        TagInputPrefilled.Tags = _prefilledTags;
        TagInputDisabled.Tags = _prefilledTags;

        _basicTags.CollectionChanged += OnBasicTagsChanged;
        UpdateBasicTagsText();
    }

    private void OnBasicTagsChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateBasicTagsText();

    private void UpdateBasicTagsText()
    {
        TagInputBasicResultText.Text = _basicTags.Count == 0
            ? "标签：(无)"
            : "标签：" + string.Join("、", _basicTags);
    }
}
