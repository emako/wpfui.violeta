using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#pragma warning disable SYSLIB1045

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A simple IPv4 entry control composed of four numeric fields.
/// Supports keyboard navigation, dot-advance, paste of dotted IPs and basic validation (0-255).
/// </summary>
public class IPv4Box : Control
{
    private TextBox? _octet0;
    private TextBox? _octet1;
    private TextBox? _octet2;
    private TextBox? _octet3;

    private bool _isInternalUpdate;

    static IPv4Box()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IPv4Box), new FrameworkPropertyMetadata(typeof(IPv4Box)));
    }

    public IPv4Box()
    {
    }

    public static readonly DependencyProperty IpAddressProperty = DependencyProperty.Register(
        nameof(IpAddress),
        typeof(string),
        typeof(IPv4Box),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIpAddressChanged)
    );

    public string IpAddress
    {
        get => (string)GetValue(IpAddressProperty);
        set => SetValue(IpAddressProperty, value);
    }

    private static void OnIpAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IPv4Box box)
        {
            box.UpdateTextBoxesFromIp();
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _octet0 = GetTemplateChild("PART_Octet0") as TextBox;
        _octet1 = GetTemplateChild("PART_Octet1") as TextBox;
        _octet2 = GetTemplateChild("PART_Octet2") as TextBox;
        _octet3 = GetTemplateChild("PART_Octet3") as TextBox;

        if (_octet0 != null) AttachHandlers(_octet0);
        if (_octet1 != null) AttachHandlers(_octet1);
        if (_octet2 != null) AttachHandlers(_octet2);
        if (_octet3 != null) AttachHandlers(_octet3);

        UpdateTextBoxesFromIp();
    }

    private void AttachHandlers(TextBox tb)
    {
        tb.PreviewTextInput += Octet_PreviewTextInput;
        tb.PreviewKeyDown += Octet_PreviewKeyDown;
        tb.TextChanged += Octet_TextChanged;
        DataObject.AddPastingHandler(tb, OnPaste);
    }

    private void Octet_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        if (e.Text == ".")
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
            string newText;
            if (tb.SelectionLength > 0)
            {
                newText = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);
            }
            else
            {
                newText = tb.Text.Insert(tb.CaretIndex, e.Text);
            }

            if (newText.Length > 3)
            {
                e.Handled = true;
                return;
            }

            if (int.TryParse(newText, out var val))
            {
                if (val > 255)
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }

    private void Octet_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;

        if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
        {
            MoveFocusToNext(tb);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Left)
        {
            if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
            {
                MoveFocusToPrevious(tb);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Right)
        {
            if (tb.CaretIndex == tb.Text.Length)
            {
                MoveFocusToNext(tb);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Back)
        {
            if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
            {
                MoveFocusToPrevious(tb);
                e.Handled = true;
            }
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
        {
            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text) && TrySetIpFromClipboard(text))
            {
                e.Handled = true;
            }
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
        {
            if (Keyboard.FocusedElement is TextBox octet
                && string.IsNullOrEmpty(octet.SelectedText))
            {
                try
                {
                    Clipboard.SetText(IpAddress);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }

    private void Octet_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isInternalUpdate) return;
        if (sender is not TextBox tb) return;

        // keep digits only
        string cleaned = Regex.Replace(tb.Text ?? string.Empty, "[^0-9]", string.Empty);
        if (cleaned != tb.Text)
        {
            int sel = tb.SelectionStart;
            tb.Text = cleaned;
            tb.SelectionStart = Math.Min(sel, tb.Text.Length);
        }

        if (int.TryParse(tb.Text, out int v))
        {
            if (v > 255)
            {
                tb.Text = "255";
                tb.SelectionStart = tb.Text.Length;
            }
        }

        if (tb.Text.Length >= 3)
        {
            MoveFocusToNext(tb);
        }

        UpdateIpFromTextBoxes();
    }

    private void OnPaste(object? sender, DataObjectPastingEventArgs e)
    {
        if (e.SourceDataObject.GetDataPresent(DataFormats.Text))
        {
            string? text = e.SourceDataObject.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(text) && TrySetIpFromClipboard(text))
            {
                e.CancelCommand();
            }
        }
    }

    private bool TrySetIpFromClipboard(string? text)
    {
        if (text is null) return false;
        var parts = text.Trim().Split('.');
        if (parts.Length == 4)
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (!byte.TryParse(parts[i], out bytes[i])) return false;
            }

            _isInternalUpdate = true;
            try
            {
                _octet0?.Text = bytes[0].ToString();
                _octet1?.Text = bytes[1].ToString();
                _octet2?.Text = bytes[2].ToString();
                _octet3?.Text = bytes[3].ToString();
                UpdateIpFromTextBoxes();
            }
            finally { _isInternalUpdate = false; }

            return true;
        }

        return false;
    }

    private void MoveFocusToNext(TextBox? tb)
    {
        if (tb == null) return;
        if (tb == _octet0) { _octet1?.Focus(); _octet1?.SelectAll(); }
        else if (tb == _octet1) { _octet2?.Focus(); _octet2?.SelectAll(); }
        else if (tb == _octet2) { _octet3?.Focus(); _octet3?.SelectAll(); }
        else { /* last */ }
    }

    private void MoveFocusToPrevious(TextBox? tb)
    {
        if (tb == null) return;
        if (tb == _octet3) { _octet2?.Focus(); _octet2?.SelectAll(); }
        else if (tb == _octet2) { _octet1?.Focus(); _octet1?.SelectAll(); }
        else if (tb == _octet1) { _octet0?.Focus(); _octet0?.SelectAll(); }
        else { /* first */ }
    }

    private void UpdateTextBoxesFromIp()
    {
        if (_isInternalUpdate) return;
        _isInternalUpdate = true;
        try
        {
            var parts = ParseIp(IpAddress);
            _octet0?.Text = parts.Length > 0 ? parts[0] : string.Empty;
            _octet1?.Text = parts.Length > 1 ? parts[1] : string.Empty;
            _octet2?.Text = parts.Length > 2 ? parts[2] : string.Empty;
            _octet3?.Text = parts.Length > 3 ? parts[3] : string.Empty;
        }
        finally { _isInternalUpdate = false; }
    }

    private static string[] ParseIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return [];
        string[] p = ip!.Trim().Split('.');
        return p;
    }

    private void UpdateIpFromTextBoxes()
    {
        if (_isInternalUpdate) return;
        _isInternalUpdate = true;
        try
        {
            string a = _octet0?.Text ?? string.Empty;
            string b = _octet1?.Text ?? string.Empty;
            string c = _octet2?.Text ?? string.Empty;
            string d = _octet3?.Text ?? string.Empty;

            bool allEmpty = string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b) && string.IsNullOrEmpty(c) && string.IsNullOrEmpty(d);

            if (allEmpty)
            {
                SetCurrentValue(IpAddressProperty, string.Empty);
            }
            else
            {
                SetCurrentValue(IpAddressProperty, $"{SafeOctet(a)}.{SafeOctet(b)}.{SafeOctet(c)}.{SafeOctet(d)}");
            }
        }
        finally { _isInternalUpdate = false; }
    }

    private static string SafeOctet(string s)
    {
        if (string.IsNullOrEmpty(s)) return "0";
        if (byte.TryParse(s, out var b)) return b.ToString();
        return "0";
    }
}
