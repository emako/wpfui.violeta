using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A tag-input control that shows a list of string tags as removable chip elements
/// and provides an inline text box for entering new tags.
/// </summary>
/// <remarks>
/// Logic is a WPF port of Ursa.Avalonia's <c>TagInput</c> control.
/// Appearance follows the WPF-UI Fluent design system.
/// </remarks>
/// <example>
/// <code lang="xml">
/// &lt;vio:TagInput
///     Tags="{Binding MyTags}"
///     PlaceholderText="Add a tag…"
///     Separator=","
///     MaxCount="10" /&gt;
/// </code>
/// </example>
[TemplatePart(Name = PART_TagPanel, Type = typeof(TagInputPanel))]
[TemplatePart(Name = PART_Placeholder, Type = typeof(UIElement))]
public class TagInput : Control
{
    public const string PART_TagPanel = "PART_TagPanel";
    public const string PART_Placeholder = "PART_Placeholder";

    private TagInputPanel? _tagPanel;
    private UIElement? _placeholder;
    private TextBox? _inputTextBox;

    // ── Dependency properties ─────────────────────────────────────────────

    public static readonly DependencyProperty TagsProperty =
        DependencyProperty.Register(
            nameof(Tags),
            typeof(IList<string>),
            typeof(TagInput),
            new PropertyMetadata(null, OnTagsChanged));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(TagInput),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MaxCountProperty =
        DependencyProperty.Register(
            nameof(MaxCount),
            typeof(int),
            typeof(TagInput),
            new PropertyMetadata(int.MaxValue));

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(
            nameof(Separator),
            typeof(string),
            typeof(TagInput),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AllowDuplicatesProperty =
        DependencyProperty.Register(
            nameof(AllowDuplicates),
            typeof(bool),
            typeof(TagInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty LostFocusBehaviorProperty =
        DependencyProperty.Register(
            nameof(LostFocusBehavior),
            typeof(LostFocusBehavior),
            typeof(TagInput),
            new PropertyMetadata(LostFocusBehavior.None));

    // ── Properties ────────────────────────────────────────────────────────

    /// <summary>Gets or sets the collection of tag strings.</summary>
    public IList<string>? Tags
    {
        get => (IList<string>?)GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    /// <summary>Gets or sets the placeholder text shown when the control is empty.</summary>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>Gets or sets the maximum number of tags allowed.</summary>
    public int MaxCount
    {
        get => (int)GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    /// <summary>
    /// Gets or sets a separator string. When non-empty, pasting or typing text that
    /// contains the separator creates multiple tags in one go.
    /// </summary>
    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    /// <summary>Gets or sets whether duplicate tags are allowed.</summary>
    public bool AllowDuplicates
    {
        get => (bool)GetValue(AllowDuplicatesProperty);
        set => SetValue(AllowDuplicatesProperty, value);
    }

    /// <summary>Gets or sets what happens to the pending text when the inner text box loses focus.</summary>
    public LostFocusBehavior LostFocusBehavior
    {
        get => (LostFocusBehavior)GetValue(LostFocusBehaviorProperty);
        set => SetValue(LostFocusBehaviorProperty, value);
    }

    // ── Close command ─────────────────────────────────────────────────────

    private ICommand? _closeTagCommand;

    /// <summary>Command bound to each <see cref="ClosableTag"/>'s close button.</summary>
    public ICommand CloseTagCommand => _closeTagCommand ??= new CloseTagCommandImpl(this);

    // ── Static ctor ───────────────────────────────────────────────────────

    static TagInput()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TagInput),
            new FrameworkPropertyMetadata(typeof(TagInput)));
    }

    public TagInput()
    {
        // Start with an empty observable collection so bindings work out-of-the-box.
        SetCurrentValue(TagsProperty, new ObservableCollection<string>());
    }

    // ── Tags change ───────────────────────────────────────────────────────

    private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TagInput)d;

        if (e.OldValue is INotifyCollectionChanged oldObs)
            oldObs.CollectionChanged -= ctrl.OnTagsCollectionChanged;

        if (e.NewValue is INotifyCollectionChanged newObs)
            newObs.CollectionChanged += ctrl.OnTagsCollectionChanged;

        ctrl.RebuildItems();
        ctrl.CheckPlaceholderVisibility();
    }

    private void OnTagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildItems();
        CheckPlaceholderVisibility();
    }

    // ── Template ──────────────────────────────────────────────────────────

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _tagPanel = GetTemplateChild(PART_TagPanel) as TagInputPanel;
        _placeholder = GetTemplateChild(PART_Placeholder) as UIElement;

        if (_tagPanel is not null)
        {
            var tb = new TextBox
            {
                MinWidth = 60,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(2, 0, 2, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            // Inherit WPF-UI color tokens.
            tb.SetResourceReference(TextBox.ForegroundProperty, "TextFillColorPrimaryBrush");
            tb.SetResourceReference(TextBox.CaretBrushProperty, "TextFillColorPrimaryBrush");
            tb.SetResourceReference(TextBox.SelectionBrushProperty, "TextControlSelectionHighlightColor");

            tb.PreviewKeyDown += OnInputTextBoxKeyDown;
            tb.LostFocus += OnInputTextBoxLostFocus;
            tb.TextChanged += OnInputTextBoxTextChanged;

            _inputTextBox = tb;
            _tagPanel.TrailingItem = tb;
        }

        RebuildItems();
        CheckPlaceholderVisibility();
    }

    // ── Focus forwarding ──────────────────────────────────────────────────

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        // Forward focus to the inner TextBox when the user clicks on the control background.
        _inputTextBox?.Focus();
    }

    // ── Item management ───────────────────────────────────────────────────

    private void RebuildItems()
    {
        if (_tagPanel is null) return;
        _tagPanel.Children.Clear();

        if (Tags is null) return;

        foreach (var tag in Tags)
        {
            var chip = new ClosableTag
            {
                Content = tag,
                Command = CloseTagCommand,
            };
            chip.CommandParameter = chip; // pass itself for index-based removal
            _tagPanel.Children.Add(chip);
        }
    }

    private void CheckPlaceholderVisibility()
    {
        if (_placeholder is null) return;
        bool isEmpty = (Tags is null || Tags.Count == 0)
                    && string.IsNullOrEmpty(_inputTextBox?.Text);
        _placeholder.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── TextBox event handlers ────────────────────────────────────────────

    private void OnInputTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                AddTags(_inputTextBox!.Text);
                e.Handled = true;
                break;

            case Key.Back:
            case Key.Delete:
                if (string.IsNullOrEmpty(_inputTextBox?.Text))
                {
                    if (Tags is null || Tags.Count == 0) break;
                    Tags.RemoveAt(Tags.Count - 1);
                    e.Handled = true;
                }
                break;
        }
    }

    private void OnInputTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        switch (LostFocusBehavior)
        {
            case LostFocusBehavior.Add:
                AddTags(_inputTextBox?.Text);
                break;
            case LostFocusBehavior.Clear:
                if (_inputTextBox is not null) _inputTextBox.Text = string.Empty;
                break;
        }
    }

    private void OnInputTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        CheckPlaceholderVisibility();
    }

    // ── Tag add / remove ──────────────────────────────────────────────────

    private void AddTags(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (Tags is null) return;
        if (Tags.Count >= MaxCount) return;

        string[] values = string.IsNullOrEmpty(Separator)
            ? [text]
            : text.Split([Separator], StringSplitOptions.RemoveEmptyEntries);

        if (!AllowDuplicates)
            values = values.Distinct().Except(Tags).ToArray();

        foreach (var value in values)
        {
            if (Tags.Count >= MaxCount) break;
            Tags.Add(value);
        }

        if (_inputTextBox is not null)
            _inputTextBox.Text = string.Empty;
    }

    /// <summary>
    /// Removes the tag corresponding to the given <see cref="ClosableTag"/> container.
    /// Called internally by <see cref="CloseTagCommand"/>.
    /// </summary>
    internal void CloseTag(ClosableTag? container)
    {
        if (container is null || _tagPanel is null || Tags is null) return;
        int index = _tagPanel.Children.IndexOf(container);
        if (index >= 0 && index < Tags.Count)
            Tags.RemoveAt(index);
    }

    // ── ICommand implementation ───────────────────────────────────────────

    private sealed class CloseTagCommandImpl : ICommand
    {
        private readonly TagInput _owner;

        public CloseTagCommandImpl(TagInput owner) => _owner = owner;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _owner.CloseTag(parameter as ClosableTag);
    }
}
