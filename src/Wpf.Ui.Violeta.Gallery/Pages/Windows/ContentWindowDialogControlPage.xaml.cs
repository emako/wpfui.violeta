using System.Windows;
using System.Windows.Controls;
using LiteObservableLanguages;
using Wpf.Ui.Violeta.Controls;
using Wpf.Ui.Violeta.Gallery.Globalization;

namespace Wpf.Ui.Violeta.Gallery.Pages.Windows;

public partial class ContentWindowDialogControlPage : Wpf.Ui.Violeta.Controls.Page
{
    public ContentWindowDialogControlPage()
    {
        InitializeComponent();
    }

    private void OpenDefaultDialogWindow_Click(object sender, RoutedEventArgs e)
    {
        ShowDialogWindow(new ContentWindowDialogControl
        {
            Title = LangKeys.Sample_9e5ffa068e.Tr(),
            PrimaryButtonText = LangKeys.ContentWindowDialogControl_ButtonOK.Tr(),
            SecondaryButtonText = LangKeys.ContentWindowDialogControl_ButtonCancel.Tr(),
            DefaultButton = ContentDialogButton.Primary,
            Content = CreateDemoForm(),
        });
    }

    private void OpenConfirmOnlyDialogWindow_Click(object sender, RoutedEventArgs e)
    {
        ShowDialogWindow(new ContentWindowDialogControl
        {
            Title = LangKeys.Sample_9e5ffa068e.Tr(),
            PrimaryButtonText = LangKeys.ContentWindowDialogControl_ButtonOK.Tr(),
            DefaultButton = ContentDialogButton.Primary,
            Content = CreateDemoForm(),
        });
    }

    private void OpenCloseAlignedDialogWindow_Click(object sender, RoutedEventArgs e)
    {
        ShowDialogWindow(new ContentWindowDialogControl
        {
            Title = LangKeys.Sample_9e5ffa068e.Tr(),
            PrimaryButtonText = LangKeys.ContentWindowDialogControl_ButtonOK.Tr(),
            SecondaryButtonText = LangKeys.ContentWindowDialogControl_ButtonCancel.Tr(),
            CloseButtonText = LangKeys.ContentWindowDialogControl_ButtonClose.Tr(),
            IsSecondaryButtonEnabled = false,
            DefaultButton = ContentDialogButton.Primary,
            ButtonAlignment = ContentWindowButtonAlignment.Right,
            Content = CreateDemoForm(),
        });
    }

    private void ShowDialogWindow(ContentWindowDialogControl content)
    {
        var dialog = ContentWindow.Create(content);
        dialog.Owner = Window.GetWindow(App.Current.MainWindow);
        _ = dialog.ShowDialog();
        ResultText.Text = LangKeys.Format_Result.Tr(dialog.Result);
    }

    private static Form CreateDemoForm()
    {
        var form = new Form
        {
            LabelPosition = FormLabelPosition.Left,
            LabelWidth = new GridLength(1, GridUnitType.Star),
        };

        form.Items.Add(new FormItem
        {
            IsRequired = true,
            Label = LangKeys.Sample_60d0458ac6.Tr(),
            Content = new Wpf.Ui.Controls.TextBox { Width = 220, PlaceholderText = LangKeys.Sample_8093e3921d.Tr() },
        });
        form.Items.Add(new FormItem
        {
            Label = LangKeys.Sample_1e1459eeed.Tr(),
            Content = BuildDepartmentComboBox(),
        });
        form.Items.Add(new FormItem
        {
            Label = LangKeys.Sample_4717f4f110.Tr(),
            Content = new CheckBox { Content = LangKeys.Sample_19b083f7cf.Tr(), IsChecked = true },
        });
        form.Items.Add(new FormItem
        {
            Label = LangKeys.Sample_18d1485cc2.Tr(),
            Content = new Wpf.Ui.Controls.ToggleSwitch { IsChecked = true },
        });

        return form;
    }

    private static ComboBox BuildDepartmentComboBox()
    {
        var combo = new ComboBox { Width = 220 };
        combo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = LangKeys.Sample_9176a628cc.Tr() });
        combo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = LangKeys.Sample_829ec9c321.Tr() });
        combo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = LangKeys.Sample_c5d34b60ac.Tr() });
        combo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = LangKeys.Sample_b890b34994.Tr() });
        combo.SelectedIndex = 0;
        return combo;
    }
}
