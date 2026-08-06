using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;
using Wpf.Ui.Input;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A Fluent-style search input with a leading search icon and an optional clear button.
/// Event surface follows WinUI <c>Windows.UI.Xaml.Controls.SearchBox</c>:
/// <see cref="QueryChanged"/> when the query text changes (including clear),
/// <see cref="QuerySubmitted"/> when the user submits (Enter).
/// </summary>
/// <example>
/// <code lang="xml">
/// &lt;vio:SearchBox
///     PlaceholderText="Search..."
///     QueryChanged="OnQueryChanged"
///     QuerySubmitted="OnQuerySubmitted" /&gt;
/// </code>
/// </example>
public class SearchBox : TextBox
{
    /// <summary>Identifies the <see cref="FocusCommand"/> dependency property.</summary>
    public static readonly DependencyProperty FocusCommandProperty = DependencyProperty.Register(
        nameof(FocusCommand),
        typeof(ICommand),
        typeof(SearchBox),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="QueryChanged"/> routed event.</summary>
    public static readonly RoutedEvent QueryChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(QueryChanged),
        RoutingStrategy.Bubble,
        typeof(Compat.TypedEventHandler<SearchBox, SearchBoxQueryChangedEventArgs>),
        typeof(SearchBox));

    /// <summary>Identifies the <see cref="QuerySubmitted"/> routed event.</summary>
    public static readonly RoutedEvent QuerySubmittedEvent = EventManager.RegisterRoutedEvent(
        nameof(QuerySubmitted),
        RoutingStrategy.Bubble,
        typeof(Compat.TypedEventHandler<SearchBox, SearchBoxQuerySubmittedEventArgs>),
        typeof(SearchBox));

    /// <summary>
    /// Occurs when the query text changes (typing, paste, or clear button),
    /// matching WinUI <c>SearchBox.QueryChanged</c>.
    /// </summary>
    public event Compat.TypedEventHandler<SearchBox, SearchBoxQueryChangedEventArgs> QueryChanged
    {
        add => AddHandler(QueryChangedEvent, value);
        remove => RemoveHandler(QueryChangedEvent, value);
    }

    /// <summary>
    /// Occurs when the user submits a search query (typically by pressing Enter),
    /// matching WinUI <c>SearchBox.QuerySubmitted</c>.
    /// </summary>
    public event Compat.TypedEventHandler<SearchBox, SearchBoxQuerySubmittedEventArgs> QuerySubmitted
    {
        add => AddHandler(QuerySubmittedEvent, value);
        remove => RemoveHandler(QuerySubmittedEvent, value);
    }

    /// <summary>
    /// Gets the command used to focus this control (e.g. for keyboard shortcuts).
    /// </summary>
    public ICommand FocusCommand => (ICommand)GetValue(FocusCommandProperty);

    static SearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SearchBox),
            new FrameworkPropertyMetadata(typeof(SearchBox)));
    }

    public SearchBox()
    {
        SetValue(FocusCommandProperty, new RelayCommand<object>(_ => Focus()));
    }

    /// <inheritdoc />
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        // Covers typing, paste, programmatic changes, and the clear (X) button.
        OnQueryChanged(Text);
    }

    /// <inheritdoc />
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if (e.Key is Key.Enter)
        {
            OnQuerySubmitted(Text);
            e.Handled = true;
        }
    }

    /// <summary>Raises the <see cref="QueryChanged"/> event.</summary>
    protected virtual void OnQueryChanged(string queryText)
    {
        RaiseEvent(new SearchBoxQueryChangedEventArgs(QueryChangedEvent, this)
        {
            QueryText = queryText ?? string.Empty,
        });
    }

    /// <summary>Raises the <see cref="QuerySubmitted"/> event.</summary>
    protected virtual void OnQuerySubmitted(string queryText)
    {
        RaiseEvent(new SearchBoxQuerySubmittedEventArgs(QuerySubmittedEvent, this)
        {
            QueryText = queryText ?? string.Empty,
        });
    }
}
