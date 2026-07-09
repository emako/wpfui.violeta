using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#pragma warning disable SYSLIB1045

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// IPv4 address with optional port entry control (e.g. 192.168.0.10:8080).
/// Supports dot-advance between octets, colon/dot jump to port, paste and validation.
/// </summary>
public class IPv4PortBox : Control
{
    private TextBox? _octet0;
    private TextBox? _octet1;
    private TextBox? _octet2;
    private TextBox? _octet3;
    private TextBox? _port;

    private bool _isInternalUpdate;

    static IPv4PortBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IPv4PortBox), new FrameworkPropertyMetadata(typeof(IPv4PortBox)));
    }

    public static readonly DependencyProperty EndpointProperty = DependencyProperty.Register(
        nameof(Endpoint),
        typeof(string),
        typeof(IPv4PortBox),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEndpointChanged)
    );

    public string Endpoint
    {
        get => (string)GetValue(EndpointProperty);
        set => SetValue(EndpointProperty, value);
    }

    private static void OnEndpointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IPv4PortBox box)
        {
            box.UpdateTextBoxesFromEndpoint();
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _octet0 = GetTemplateChild("PART_Octet0") as TextBox;
        _octet1 = GetTemplateChild("PART_Octet1") as TextBox;
        _octet2 = GetTemplateChild("PART_Octet2") as TextBox;
        _octet3 = GetTemplateChild("PART_Octet3") as TextBox;
        _port = GetTemplateChild("PART_Port") as TextBox;

        if (_octet0 != null) AttachOctetHandlers(_octet0);
        if (_octet1 != null) AttachOctetHandlers(_octet1);
        if (_octet2 != null) AttachOctetHandlers(_octet2);
        if (_octet3 != null) AttachOctetHandlers(_octet3);
        if (_port != null) AttachPortHandlers(_port);

        UpdateTextBoxesFromEndpoint();
    }

    private void AttachOctetHandlers(TextBox tb)
    {
        tb.PreviewTextInput += Octet_PreviewTextInput;
        tb.PreviewKeyDown += Octet_PreviewKeyDown;
        tb.TextChanged += Octet_TextChanged;
        DataObject.AddPastingHandler(tb, OnPaste);
    }

    private void AttachPortHandlers(TextBox tb)
    {
        tb.PreviewTextInput += Port_PreviewTextInput;
        tb.PreviewKeyDown += Port_PreviewKeyDown;
        tb.TextChanged += Port_TextChanged;
        DataObject.AddPastingHandler(tb, OnPaste);
    }

    private void Octet_PreviewTextInput(object? sender, TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        if (e.Text == ":")
        {
            MoveFocusToPort();
            e.Handled = true;
            return;
        }

        if (e.Text == ".")
        {
            MoveFocusFromOctet(sender as TextBox);
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
            string newText = tb.SelectionLength > 0
                ? tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text)
                : tb.Text.Insert(tb.CaretIndex, e.Text);

            if (newText.Length > 3)
            {
                e.Handled = true;
                return;
            }

            if (int.TryParse(newText, out var val) && val > 255)
            {
                e.Handled = true;
            }
        }
    }

    private void Octet_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb)
        {
            return;
        }

        if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
        {
            MoveFocusFromOctet(tb);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Left)
        {
            if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
            {
                MoveFocusToPreviousOctet(tb);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Right)
        {
            if (tb.CaretIndex == tb.Text.Length)
            {
                MoveFocusFromOctet(tb);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Back)
        {
            if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
            {
                MoveFocusToPreviousOctet(tb);
                e.Handled = true;
            }
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
        {
            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text) && TrySetFromClipboard(text))
            {
                e.Handled = true;
            }
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
        {
            if (Keyboard.FocusedElement is TextBox octet
                && string.IsNullOrEmpty(octet.SelectedText))
            {
                CopyEndpointToClipboard();
            }
        }
    }

    private void Octet_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isInternalUpdate)
        {
            return;
        }

        if (sender is not TextBox tb)
        {
            return;
        }

        string cleaned = Regex.Replace(tb.Text ?? string.Empty, "[^0-9]", string.Empty);
        if (cleaned != tb.Text)
        {
            int sel = tb.SelectionStart;
            tb.Text = cleaned;
            tb.SelectionStart = Math.Min(sel, tb.Text.Length);
        }

        if (int.TryParse(tb.Text, out int v) && v > 255)
        {
            tb.Text = "255";
            tb.SelectionStart = tb.Text.Length;
        }

        if (tb.Text.Length >= 3)
        {
            MoveFocusFromOctet(tb);
        }

        UpdateEndpointFromTextBoxes();
    }

    private void Port_PreviewTextInput(object? sender, TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text) || !char.IsDigit(e.Text, 0))
        {
            e.Handled = true;
            return;
        }

        if (sender is not TextBox tb)
        {
            return;
        }

        string newText = tb.SelectionLength > 0
            ? tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text)
            : tb.Text.Insert(tb.CaretIndex, e.Text);

        if (newText.Length > 5)
        {
            e.Handled = true;
            return;
        }

        if (int.TryParse(newText, out var val) && val > 65535)
        {
            e.Handled = true;
        }
    }

    private void Port_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb)
        {
            return;
        }

        if (e.Key == Key.Left)
        {
            if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
            {
                _octet3?.Focus();
                _octet3?.SelectAll();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Back)
        {
            if (tb.CaretIndex == 0 && tb.SelectionLength == 0)
            {
                _octet3?.Focus();
                _octet3?.SelectAll();
                e.Handled = true;
            }
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
        {
            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text) && TrySetFromClipboard(text))
            {
                e.Handled = true;
            }
        }
        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
        {
            if (string.IsNullOrEmpty(tb.SelectedText))
            {
                CopyEndpointToClipboard();
            }
        }
    }

    private void Port_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isInternalUpdate)
        {
            return;
        }

        if (sender is not TextBox tb)
        {
            return;
        }

        string cleaned = Regex.Replace(tb.Text ?? string.Empty, "[^0-9]", string.Empty);
        if (cleaned != tb.Text)
        {
            int sel = tb.SelectionStart;
            tb.Text = cleaned;
            tb.SelectionStart = Math.Min(sel, tb.Text.Length);
        }

        if (int.TryParse(tb.Text, out int v) && v > 65535)
        {
            tb.Text = "65535";
            tb.SelectionStart = tb.Text.Length;
        }

        UpdateEndpointFromTextBoxes();
    }

    private void OnPaste(object? sender, DataObjectPastingEventArgs e)
    {
        if (e.SourceDataObject.GetDataPresent(DataFormats.Text))
        {
            string? text = e.SourceDataObject.GetData(DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(text) && TrySetFromClipboard(text))
            {
                e.CancelCommand();
            }
        }
    }

    private void CopyEndpointToClipboard()
    {
        try
        {
            Clipboard.SetText(Endpoint);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private bool TrySetFromClipboard(string? text)
    {
        if (text is null)
        {
            return false;
        }

        text = text.Trim();
        if (!TryParseEndpoint(text, out var octets, out var port))
        {
            return false;
        }

        _isInternalUpdate = true;
        try
        {
            _octet0!.Text = octets[0].ToString();
            _octet1?.Text = octets[1].ToString();
            _octet2?.Text = octets[2].ToString();
            _octet3?.Text = octets[3].ToString();
            _port?.Text = port >= 0 ? port.ToString() : string.Empty;
            UpdateEndpointFromTextBoxes();
        }
        finally
        {
            _isInternalUpdate = false;
        }

        return true;
    }

    private static bool TryParseEndpoint(string text, out byte[] octets, out int port)
    {
        octets = new byte[4];
        port = -1;

        string ipPart;
        int colonIndex = text.LastIndexOf(':');
        if (colonIndex >= 0)
        {
            ipPart = text.Substring(0, colonIndex);
            string portPart = text.Substring(colonIndex + 1);
            if (portPart.Length == 0 || !int.TryParse(portPart, out port) || port < 0 || port > 65535)
            {
                return false;
            }
        }
        else
        {
            ipPart = text;
        }

        var parts = ipPart.Split('.');
        if (parts.Length != 4)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (!byte.TryParse(parts[i], out octets[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void MoveFocusFromOctet(TextBox? tb)
    {
        if (tb == null)
        {
            return;
        }

        if (tb == _octet0)
        {
            _octet1?.Focus();
            _octet1?.SelectAll();
        }
        else if (tb == _octet1)
        {
            _octet2?.Focus();
            _octet2?.SelectAll();
        }
        else if (tb == _octet2)
        {
            _octet3?.Focus();
            _octet3?.SelectAll();
        }
        else
        {
            MoveFocusToPort();
        }
    }

    private void MoveFocusToPort()
    {
        _port?.Focus();
        _port?.SelectAll();
    }

    private void MoveFocusToPreviousOctet(TextBox? tb)
    {
        if (tb == null)
        {
            return;
        }

        if (tb == _octet3)
        {
            _octet2?.Focus();
            _octet2?.SelectAll();
        }
        else if (tb == _octet2)
        {
            _octet1?.Focus();
            _octet1?.SelectAll();
        }
        else if (tb == _octet1)
        {
            _octet0?.Focus();
            _octet0?.SelectAll();
        }
    }

    private void UpdateTextBoxesFromEndpoint()
    {
        if (_isInternalUpdate)
        {
            return;
        }

        _isInternalUpdate = true;
        try
        {
            if (string.IsNullOrWhiteSpace(Endpoint))
            {
                _octet0?.Text = string.Empty;
                _octet1?.Text = string.Empty;
                _octet2?.Text = string.Empty;
                _octet3?.Text = string.Empty;
                _port?.Text = string.Empty;
                return;
            }

            if (TryParseEndpoint(Endpoint.Trim(), out var octets, out var port))
            {
                _octet0?.Text = octets[0].ToString();
                _octet1?.Text = octets[1].ToString();
                _octet2?.Text = octets[2].ToString();
                _octet3?.Text = octets[3].ToString();
                _port?.Text = port >= 0 ? port.ToString() : string.Empty;
            }
            else
            {
                var ipParts = Endpoint.Trim().Split(':')[0].Split('.');
                _octet0?.Text = ipParts.Length > 0 ? ipParts[0] : string.Empty;
                _octet1?.Text = ipParts.Length > 1 ? ipParts[1] : string.Empty;
                _octet2?.Text = ipParts.Length > 2 ? ipParts[2] : string.Empty;
                _octet3?.Text = ipParts.Length > 3 ? ipParts[3] : string.Empty;
                _port?.Text = string.Empty;
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private void UpdateEndpointFromTextBoxes()
    {
        if (_isInternalUpdate)
        {
            return;
        }

        _isInternalUpdate = true;
        try
        {
            string a = _octet0?.Text ?? string.Empty;
            string b = _octet1?.Text ?? string.Empty;
            string c = _octet2?.Text ?? string.Empty;
            string d = _octet3?.Text ?? string.Empty;
            string p = _port?.Text ?? string.Empty;

            bool allEmpty = string.IsNullOrEmpty(a)
                && string.IsNullOrEmpty(b)
                && string.IsNullOrEmpty(c)
                && string.IsNullOrEmpty(d)
                && string.IsNullOrEmpty(p);

            if (allEmpty)
            {
                SetCurrentValue(EndpointProperty, string.Empty);
                return;
            }

            string ip = $"{SafeOctet(a)}.{SafeOctet(b)}.{SafeOctet(c)}.{SafeOctet(d)}";
            if (string.IsNullOrEmpty(p))
            {
                SetCurrentValue(EndpointProperty, ip);
            }
            else
            {
                SetCurrentValue(EndpointProperty, $"{ip}:{SafePort(p)}");
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private static string SafeOctet(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return "0";
        }

        return byte.TryParse(s, out var b) ? b.ToString() : "0";
    }

    private static string SafePort(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return "0";
        }

        if (int.TryParse(s, out var port))
        {
            return Math.Max(0, Math.Min(65535, port)).ToString();
        }

        return "0";
    }
}
