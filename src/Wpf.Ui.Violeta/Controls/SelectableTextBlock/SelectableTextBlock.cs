using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Violeta.Resources.Localization;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// A read-only, selectable text display control.
/// Mirrors the behavior of Avalonia's <c>SelectableTextBlock</c>:
/// supports text selection via mouse/keyboard, clipboard copy, and exposes
/// Avalonia-compatible <see cref="SelectionEnd"/>, <see cref="CanCopy"/>,
/// <see cref="SelectionForegroundBrush"/>, and <see cref="Copy"/> API.
/// </summary>
public class SelectableTextBlock : TextBox
{
    static SelectableTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SelectableTextBlock),
            new FrameworkPropertyMetadata(typeof(SelectableTextBlock)));

        IsReadOnlyProperty.OverrideMetadata(
            typeof(SelectableTextBlock),
            new FrameworkPropertyMetadata(true));

        IsTabStopProperty.OverrideMetadata(
            typeof(SelectableTextBlock),
            new FrameworkPropertyMetadata(false));

        CursorProperty.OverrideMetadata(
            typeof(SelectableTextBlock),
            new FrameworkPropertyMetadata(Cursors.IBeam));
    }

    public SelectableTextBlock()
    {
        InitializeContextMenu();
    }

    private void InitializeContextMenu()
    {
        var copyItem = new MenuItem
        {
            Command = ApplicationCommands.Copy,
            Header = SH.ButtonCopy,
            InputGestureText = "Ctrl+C",
        };

        ContextMenu = new ContextMenu();
        ContextMenu.Items.Add(copyItem);
    }

    #region SelectionEnd

    /// <summary>
    /// Gets or sets the character index marking the end of the current selection.
    /// Mirrors <c>SelectableTextBlock.SelectionEnd</c> in Avalonia.
    /// </summary>
    public int SelectionEnd
    {
        get => SelectionStart + SelectionLength;
        set => Select(SelectionStart, Math.Max(0, value - SelectionStart));
    }

    #endregion SelectionEnd

    #region CanCopy

    /// <summary>
    /// Gets whether there is a non-empty selection that can be copied to the clipboard.
    /// Mirrors <c>SelectableTextBlock.CanCopy</c> in Avalonia.
    /// </summary>
    public bool CanCopy => SelectionLength > 0;

    #endregion CanCopy

    #region SelectionForegroundBrush

    public static readonly DependencyProperty SelectionForegroundBrushProperty =
        DependencyProperty.Register(
            nameof(SelectionForegroundBrush),
            typeof(Brush),
            typeof(SelectableTextBlock),
            new PropertyMetadata(null, OnSelectionForegroundBrushChanged));

    private static void OnSelectionForegroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectableTextBlock self)
        {
#if NET48_OR_GREATER || NET5_0_OR_GREATER
            self.SelectionTextBrush = e.NewValue as Brush;
#endif
        }
    }

    /// <summary>
    /// The brush used to render the foreground colour of selected text.
    /// Mirrors <c>SelectableTextBlock.SelectionForegroundBrush</c> in Avalonia.
    /// Internally maps to <see cref="TextBox.SelectionTextBrush"/>.
    /// </summary>
    public Brush? SelectionForegroundBrush
    {
        get => (Brush?)GetValue(SelectionForegroundBrushProperty);
        set => SetValue(SelectionForegroundBrushProperty, value);
    }

    #endregion SelectionForegroundBrush

    /// <summary>
    /// Copies the currently selected text to the clipboard.
    /// Mirrors <c>SelectableTextBlock.Copy()</c> in Avalonia.
    /// </summary>
    public new void Copy() => ApplicationCommands.Copy.Execute(null, this);
}
