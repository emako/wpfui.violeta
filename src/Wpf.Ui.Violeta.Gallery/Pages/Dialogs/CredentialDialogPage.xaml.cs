using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Wpf.Ui.Violeta.Win32.NativeDialog;
using Wpf.Ui.Violeta.Gallery.Globalization;
using LiteObservableLanguages;

namespace Wpf.Ui.Violeta.Gallery.Pages.Dialogs;

public partial class CredentialDialogPage : Wpf.Ui.Violeta.Controls.Page
{
    public CredentialDialogPage()
    {
        InitializeComponent();
    }

    private nint OwnerHandle
    {
        get
        {
            Window? window = Window.GetWindow(this);
            return window is null ? 0 : new WindowInteropHelper(window).Handle;
        }
    }

    private string Target => string.IsNullOrWhiteSpace(TargetTextBox.Text)
        ? "Wpf.Ui.Violeta.Gallery.Sample"
        : TargetTextBox.Text.Trim();

    private void ShowCredentialDialog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
        {
            return;
        }

        var tag = btn.Tag?.ToString();
        nint owner = OwnerHandle;

        try
        {
            switch (tag)
            {
                case "Prompt":
                    PromptAndReport(owner, showSaveCheckBox: false, useAppCache: false);
                    return;

                case "SaveChecked":
                    PromptAndReport(owner, showSaveCheckBox: true, useAppCache: false);
                    return;

                case "AppCache":
                    PromptAndReport(owner, showSaveCheckBox: true, useAppCache: true);
                    return;

                case "Retrieve":
                    RetrieveAndReport();
                    return;

                case "Delete":
                    DeleteAndReport();
                    return;
            }
        }
        catch (Exception ex)
        {
            CredentialDialogResultText.Text = LangKeys.Format_Exception.Tr(ex.Message);
        }
    }

    private void PromptAndReport(nint owner, bool showSaveCheckBox, bool useAppCache)
    {
        var dialog = new CredentialDialog
        {
            Target = Target,
            MainInstruction = LangKeys.Sample_4b151dd64a.Tr(),
            Content = LangKeys.Sample_84063d73bf.Tr(),
            ShowSaveCheckBox = showSaveCheckBox,
            UseApplicationInstanceCredentialCache = useAppCache,
        };

        bool accepted = dialog.ShowDialog(owner);
        if (!accepted)
        {
            CredentialDialogResultText.Text = LangKeys.Sample_6457052c6e.Tr();
            return;
        }

        dialog.ConfirmCredentials(confirm: true);

        CredentialDialogResultText.Text = dialog.IsStoredCredential
            ? LangKeys.Format_CachedCredential.Tr(dialog.UserName)
            : LangKeys.Format_UserSave.Tr(dialog.UserName, dialog.IsSaveChecked);
    }

    private void RetrieveAndReport()
    {
        NetworkCredential? credential = CredentialDialog.RetrieveCredential(Target);
        CredentialDialogResultText.Text = credential is null
            ? LangKeys.Sample_015d3e62d9.Tr()
            : LangKeys.Format_SavedCredential.Tr(credential.UserName);
    }

    private void DeleteAndReport()
    {
        bool removed = CredentialDialog.DeleteCredential(Target);
        CredentialDialogResultText.Text = removed
            ? LangKeys.Sample_2c6ba0e39a.Tr()
            : LangKeys.Sample_76a6900819.Tr();
    }
}
