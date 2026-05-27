using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A time entry control composed of up to four numeric fields (HH:MM:SS.mmm).
/// Supports keyboard navigation, colon/dot-advance, Up/Down increment, paste of time strings, and basic validation.
/// </summary>
public class TimeBox : Control
{
    private TextBox? _hourBox;
    private TextBox? _minuteBox;
    private TextBox? _secondBox;
    private TextBox? _millisecondBox;

    private bool _isInternalUpdate;

    // Section limits: Hours[0-23], Minutes[0-59], Seconds[0-59], Milliseconds[0-999]
    private static readonly int[] s_sectionMaxDigits = [2, 2, 2, 3];

    private static readonly int[] s_sectionLimits = [24, 60, 60, 1000];

    static TimeBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeBox), new FrameworkPropertyMetadata(typeof(TimeBox)));
    }

    #region Dependency Properties

    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        nameof(Time),
        typeof(TimeSpan?),
        typeof(TimeBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChanged));

    public TimeSpan? Time
    {
        get => (TimeSpan?)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public static readonly DependencyProperty ShowLeadingZeroProperty = DependencyProperty.Register(
        nameof(ShowLeadingZero),
        typeof(bool),
        typeof(TimeBox),
        new PropertyMetadata(true, OnShowLeadingZeroChanged));

    public bool ShowLeadingZero
    {
        get => (bool)GetValue(ShowLeadingZeroProperty);
        set => SetValue(ShowLeadingZeroProperty, value);
    }

    public static readonly DependencyProperty ShowMillisecondProperty = DependencyProperty.Register(
        nameof(ShowMillisecond),
        typeof(bool),
        typeof(TimeBox),
        new PropertyMetadata(false, OnShowMillisecondChanged));

    /// <summary>
    /// Whether to display the millisecond section.
    /// </summary>
    public bool ShowMillisecond
    {
        get => (bool)GetValue(ShowMillisecondProperty);
        set => SetValue(ShowMillisecondProperty, value);
    }

    #endregion Dependency Properties

    #region Property Changed Callbacks

    private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBox box)
        {
            box.UpdateSectionsFromTime();
        }
    }

    private static void OnShowLeadingZeroChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBox box)
        {
            box.UpdateSectionsFromTime();
        }
    }

    private static void OnShowMillisecondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimeBox box && box._millisecondBox is not null)
        {
            // The separator TextBlock before milliseconds shares the same Visibility via a trigger in XAML.
            // Here we just trigger a refresh.
            box.UpdateSectionsFromTime();
        }
    }

    #endregion Property Changed Callbacks

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _hourBox = GetTemplateChild("PART_HourTextBox") as TextBox;
        _minuteBox = GetTemplateChild("PART_MinuteTextBox") as TextBox;
        _secondBox = GetTemplateChild("PART_SecondTextBox") as TextBox;
        _millisecondBox = GetTemplateChild("PART_MillisecondTextBox") as TextBox;

        if (_hourBox is not null) AttachHandlers(_hourBox);
        if (_minuteBox is not null) AttachHandlers(_minuteBox);
        if (_secondBox is not null) AttachHandlers(_secondBox);
        if (_millisecondBox is not null) AttachHandlers(_millisecondBox);

        UpdateSectionsFromTime();
    }

    #region Event Attachment

    private void AttachHandlers(TextBox tb)
    {
        tb.PreviewTextInput += OnPreviewTextInput;
        tb.PreviewKeyDown += OnPreviewKeyDown;
        tb.TextChanged += OnSectionTextChanged;
        DataObject.AddPastingHandler(tb, OnPaste);
    }

    #endregion Event Attachment

    #region Input Handlers

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        // ':' advances to next section
        if (e.Text == ":" || e.Text == ".")
        {
            MoveFocusToNext(sender as TextBox);
            e.Handled = true;
            return;
        }

        if (!char.IsDigit(e.Text, 0))
        {
            e.Handled = true;
            return;
        }

        if (sender is TextBox tb)
        {
            int maxDigits = GetMaxDigitsForSection(tb);
            string newText;
            if (tb.SelectionLength > 0)
            {
                newText = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
            }
            else
            {
                newText = tb.Text.Insert(tb.CaretIndex, e.Text);
            }

            if (newText.Length > maxDigits)
            {
                e.Handled = true;
                return;
            }

            int limit = GetLimitForSection(tb);
            if (int.TryParse(newText, out int val) && val >= limit)
            {
                e.Handled = true;
            }
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;

        switch (e.Key)
        {
            case Key.OemSemicolon when (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift:
            // Shift+; = ':' on standard US keyboard
            case Key.OemPeriod:
            case Key.Decimal:
                MoveFocusToNext(tb);
                e.Handled = true;
                return;

            case Key.Left:
                if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
                {
                    MoveFocusToPrevious(tb);
                    e.Handled = true;
                }
                break;

            case Key.Right:
                if (tb.CaretIndex == tb.Text.Length && tb.SelectionLength == 0)
                {
                    MoveFocusToNext(tb);
                    e.Handled = true;
                }
                break;

            case Key.Back:
                if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
                {
                    MoveFocusToPrevious(tb);
                    e.Handled = true;
                }
                break;

            case Key.Up:
                IncrementSection(tb, +1);
                e.Handled = true;
                break;

            case Key.Down:
                IncrementSection(tb, -1);
                e.Handled = true;
                break;

            case Key.Tab:
                // Allow normal Tab to proceed
                break;

            default:
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
                {
                    var text = Clipboard.GetText();
                    if (!string.IsNullOrEmpty(text) && TrySetTimeFromString(text))
                    {
                        e.Handled = true;
                    }
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
                {
                    if (string.IsNullOrEmpty(tb.SelectedText) && Time.HasValue)
                    {
                        try
                        {
                            Clipboard.SetText(FormatTime(Time.Value));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }

                        e.Handled = true;
                    }
                }
                break;
        }
    }

    private void OnSectionTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInternalUpdate) return;
        if (sender is not TextBox tb) return;

        // Auto-advance when the section is fully filled
        int maxDigits = GetMaxDigitsForSection(tb);
        if (tb.Text.Length >= maxDigits)
        {
            // Validate and clamp
            int limit = GetLimitForSection(tb);
            if (int.TryParse(tb.Text, out int val) && val >= limit)
            {
                _isInternalUpdate = true;
                try
                {
                    tb.Text = (limit - 1).ToString();
                    tb.CaretIndex = tb.Text.Length;
                }
                finally
                {
                    _isInternalUpdate = false;
                }
            }

            MoveFocusToNext(tb);
        }

        UpdateTimeFromSections();
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.SourceDataObject.GetDataPresent(DataFormats.Text))
        {
            string? text = e.SourceDataObject.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(text) && TrySetTimeFromString(text))
            {
                e.CancelCommand();
            }
        }
    }

    #endregion Input Handlers

    #region Navigation

    private void MoveFocusToNext(TextBox? tb)
    {
        if (tb == null) return;
        if (tb == _hourBox) { _minuteBox?.Focus(); _minuteBox?.SelectAll(); }
        else if (tb == _minuteBox) { _secondBox?.Focus(); _secondBox?.SelectAll(); }
        else if (tb == _secondBox && ShowMillisecond) { _millisecondBox?.Focus(); _millisecondBox?.SelectAll(); }
    }

    private void MoveFocusToPrevious(TextBox? tb)
    {
        if (tb == null) return;
        if (tb == _millisecondBox) { _secondBox?.Focus(); _secondBox?.SelectAll(); }
        else if (tb == _secondBox) { _minuteBox?.Focus(); _minuteBox?.SelectAll(); }
        else if (tb == _minuteBox) { _hourBox?.Focus(); _hourBox?.SelectAll(); }
    }

    #endregion Navigation

    #region Increment / Decrement

    private void IncrementSection(TextBox tb, int delta)
    {
        int current = int.TryParse(tb.Text, out int v) ? v : 0;
        int limit = GetLimitForSection(tb);
        current = ((current + delta) % limit + limit) % limit;
        SetSectionText(tb, current);
        UpdateTimeFromSections();
    }

    #endregion Increment / Decrement

    #region Update Helpers

    private void UpdateSectionsFromTime()
    {
        if (_isInternalUpdate) return;
        _isInternalUpdate = true;
        try
        {
            TimeSpan? t = Time;

            if (t is null)
            {
                _hourBox?.Text = string.Empty;
                _minuteBox?.Text = string.Empty;
                _secondBox?.Text = string.Empty;
                _millisecondBox?.Text = string.Empty;
            }
            else
            {
                if (_hourBox is not null) SetSectionText(_hourBox, t.Value.Hours);
                if (_minuteBox is not null) SetSectionText(_minuteBox, t.Value.Minutes);
                if (_secondBox is not null) SetSectionText(_secondBox, t.Value.Seconds);
                if (_millisecondBox is not null) SetSectionTextMs(_millisecondBox, t.Value.Milliseconds);
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private void SetSectionText(TextBox tb, int value)
    {
        tb.Text = ShowLeadingZero ? value.ToString("D2") : value.ToString();
    }

    private void SetSectionTextMs(TextBox tb, int value)
    {
        tb.Text = ShowLeadingZero ? value.ToString("D3") : value.ToString();
    }

    private void UpdateTimeFromSections()
    {
        if (_isInternalUpdate) return;
        _isInternalUpdate = true;
        try
        {
            int h = ParseSection(_hourBox);
            int m = ParseSection(_minuteBox);
            int s = ParseSection(_secondBox);
            int ms = ParseSection(_millisecondBox);

            bool allEmpty = _hourBox?.Text is null or ""
                && _minuteBox?.Text is null or ""
                && _secondBox?.Text is null or ""
                && (_millisecondBox?.Text is null or "" || !ShowMillisecond);

            if (allEmpty)
            {
                SetCurrentValue(TimeProperty, null);
            }
            else
            {
                // Clamp values
                h = Clamp(h, 0, 23);
                m = Clamp(m, 0, 59);
                s = Clamp(s, 0, 59);
                ms = Clamp(ms, 0, 999);
                SetCurrentValue(TimeProperty, new TimeSpan(0, h, m, s, ms));
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private static int ParseSection(TextBox? tb)
    {
        if (tb is null || string.IsNullOrEmpty(tb.Text)) return 0;
        return int.TryParse(tb.Text, out int v) ? v : 0;
    }

    private bool TrySetTimeFromString(string? text)
    {
        if (text is null) return false;
        text = text.Trim();

        // Try standard TimeSpan formats: hh:mm:ss.fff, hh:mm:ss, hh:mm
        if (TimeSpan.TryParse(text, out TimeSpan ts))
        {
            SetCurrentValue(TimeProperty, ts);
            return true;
        }

        return false;
    }

    private static string FormatTime(TimeSpan t)
    {
        return $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}.{t.Milliseconds:D3}";
    }

    private int GetMaxDigitsForSection(TextBox tb)
    {
        if (tb == _millisecondBox) return s_sectionMaxDigits[3];
        return 2;
    }

    private int GetLimitForSection(TextBox tb)
    {
        if (tb == _hourBox) return s_sectionLimits[0];
        if (tb == _minuteBox) return s_sectionLimits[1];
        if (tb == _secondBox) return s_sectionLimits[2];
        if (tb == _millisecondBox) return s_sectionLimits[3];
        return 100;
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;

    #endregion Update Helpers
}
