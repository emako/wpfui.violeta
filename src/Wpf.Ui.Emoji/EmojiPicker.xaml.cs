//
//  Wpf.Ui.Emoji — Emoji support for WPF
//
//  Copyright © 2017–2021 Sam Hocevar <sam@hocevar.net>
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Wpf.Ui.Emoji;

/// <summary>
/// Interaction logic for Picker.xaml
/// </summary>
public partial class EmojiPicker : UserControl
{
    public EmojiPicker()
    {
        InitializeComponent();
    }

    //// Backwards compatibility for when the backend was a TextBlock.
    //public double FontSize
    //{
    //    get => Image.Height * 0.75;
    //    set => Image.Height = value / 0.75;
    //}

    public event PropertyChangedEventHandler SelectionChanged;

    private static void OnSelectionPropertyChanged(DependencyObject source,
                                                   DependencyPropertyChangedEventArgs e)
    {
        (source as EmojiPicker)?.OnSelectionChanged(e.NewValue as string);
    }

    public string Selection
    {
        get => (string)GetValue(SelectionProperty);
        set => SetValue(SelectionProperty, value);
    }

    private void OnSelectionChanged(string s)
    {
        var is_disabled = string.IsNullOrEmpty(s);
        Image.SetValue(EmojiImage.SourceProperty, is_disabled ? "???" : s);
        Image.Opacity = is_disabled ? 0.3 : 1.0;
        SelectionChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selection)));
    }

    public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register(
        nameof(Selection), typeof(string), typeof(EmojiPicker),
            new FrameworkPropertyMetadata("☺", OnSelectionPropertyChanged));

    private void OnPopupKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && sender is Popup popup)
        {
            popup.IsOpen = false;
            e.Handled = true;
        }
    }

    private void OnPopupLoaded(object sender, RoutedEventArgs e)
    {
        if (!(sender is Popup popup))
            return;

        var child = popup.Child;
        IInputElement old_focus = null;
        child.Focusable = true;
        child.IsVisibleChanged += (o, ea) =>
        {
            if (child.IsVisible)
            {
                old_focus = Keyboard.FocusedElement;
                Keyboard.Focus(child);
            }
        };

        popup.Closed += (o, ea) => Keyboard.Focus(old_focus);
    }

    public static readonly DependencyProperty PopupBorderStyleProperty = TabbedEmojiList.PopupBorderStyleProperty.AddOwner(typeof(EmojiPicker));

    public Style PopupBorderStyle
    {
        get { return (Style)GetValue(PopupBorderStyleProperty); }
        set { SetValue(PopupBorderStyleProperty, value); }
    }

    public static readonly DependencyProperty EmojiItemSizeProperty = TabbedEmojiList.EmojiItemSizeProperty.AddOwner(typeof(EmojiPicker));

    public double EmojiItemSize
    {
        get { return (double)GetValue(EmojiItemSizeProperty); }
        set { SetValue(EmojiItemSizeProperty, value); }
    }

    public static readonly DependencyProperty PickerEmojiItemSizeProperty = DependencyProperty.Register(nameof(PickerEmojiItemSize), typeof(double), typeof(EmojiPicker), new PropertyMetadata(double.NaN));

    public double PickerEmojiItemSize
    {
        get { return (double)GetValue(PickerEmojiItemSizeProperty); }
        set { SetValue(PickerEmojiItemSizeProperty, value); }
    }

    public static readonly DependencyProperty EmojiToggleButtonStyleProperty = TabbedEmojiList.EmojiToggleButtonStyleProperty.AddOwner(typeof(EmojiPicker));

    public Style EmojiToggleButtonStyle
    {
        get { return (Style)GetValue(EmojiToggleButtonStyleProperty); }
        set { SetValue(EmojiToggleButtonStyleProperty, value); }
    }

    public static readonly RoutedEvent EmojiPickedEvent = TabbedEmojiList.EmojiPickedEvent.AddOwner(typeof(EmojiPicker));

    public event EmojiPickedEventHandler EmojiPicked
    {
        add { this.AddHandler(EmojiPickedEvent, value); }
        remove { this.RemoveHandler(EmojiPickedEvent, value); }
    }

    private void TabbedEmojiList_EmojiList_EmojiPicked(object sender, EmojiPickedEventArgs e)
    {
        Selection = e.Emoji;
        Button_INTERNAL.IsChecked = false;
        e.Handled = true;
        this.RaiseEvent(new EmojiPickedEventArgs(Selection));
    }
}
