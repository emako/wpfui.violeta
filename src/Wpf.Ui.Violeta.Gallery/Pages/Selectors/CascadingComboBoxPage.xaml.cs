using System;
using System.ComponentModel;
using System.Windows;
using Wpf.Ui.Violeta.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Selectors;

public partial class CascadingComboBoxPage : Wpf.Ui.Violeta.Controls.Page
{
    public CascadingComboBoxPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CascadingComboBoxDemo.ItemsSource = new ICascadingItem[]
        {
            new CascadingItem(LangKeys.Sample_d4912425b4.Tr(),
            [
                new CascadingItem(LangKeys.Sample_7e040aa9cb.Tr()),
                new CascadingItem(LangKeys.Sample_7a399889b9.Tr()),
                new CascadingItem(LangKeys.Sample_027110256c.Tr()),
                new CascadingItem(LangKeys.Sample_852861b891.Tr()),
            ]),
            new CascadingItem(LangKeys.Sample_a44dc3df64.Tr(),
            [
                new CascadingItem(LangKeys.Sample_69d6beffab.Tr()),
                new CascadingItem(LangKeys.Sample_ed5a4dc733.Tr()),
                new CascadingItem(LangKeys.Sample_71f38f399f.Tr()),
                new CascadingItem(LangKeys.Sample_572fd7fd9c.Tr()),
            ]),
            new CascadingItem(LangKeys.Sample_2428ebeae7.Tr(),
            [
                new CascadingItem(LangKeys.Sample_ad827c5906.Tr()),
                new CascadingItem(LangKeys.Sample_995882b996.Tr()),
                new CascadingItem(LangKeys.Sample_cc6b473b7e.Tr()),
                new CascadingItem(LangKeys.Sample_880490aef6.Tr()),
            ]),
        };

        DependencyPropertyDescriptor
            .FromProperty(CascadingComboBox.SelectedCascadingItemProperty, typeof(CascadingComboBox))
            ?.AddValueChanged(CascadingComboBoxDemo, OnSelectedCascadingItemChanged);
    }

    private void OnSelectedCascadingItemChanged(object? sender, EventArgs e)
    {
        CascadingComboBoxResultText.Text = CascadingComboBoxDemo.SelectedCascadingItem is { Label: { } label }
            ? LangKeys.Sample_af69d6c47f.Tr(label)
            : LangKeys.Sample_b5c92782c9.Tr();
    }
}
