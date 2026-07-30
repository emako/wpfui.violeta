using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Wpf.Ui.Violeta.Gallery.Pages.OpSystem;

public partial class FilePickerPage : Wpf.Ui.Violeta.Controls.Page
{
    public FilePickerPage()
    {
        InitializeComponent();
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        OpenedFilePathText.Visibility = Visibility.Collapsed;

        OpenFileDialog dialog = new()
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Filter = "All files (*.*)|*.*",
        };

        if (dialog.ShowDialog() != true || !File.Exists(dialog.FileName))
        {
            return;
        }

        OpenedFilePathText.Text = $"已选择：{dialog.FileName}";
        OpenedFilePathText.Visibility = Visibility.Visible;
    }

    private void OpenPicture_Click(object sender, RoutedEventArgs e)
    {
        OpenedPicturePathText.Visibility = Visibility.Collapsed;

        OpenFileDialog dialog = new()
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.png)|*.bmp;*.jpg;*.jpeg;*.png|All files (*.*)|*.*",
        };

        if (dialog.ShowDialog() != true || !File.Exists(dialog.FileName))
        {
            return;
        }

        OpenedPicturePathText.Text = $"已选择：{dialog.FileName}";
        OpenedPicturePathText.Visibility = Visibility.Visible;
    }

    private void OpenMultiple_Click(object sender, RoutedEventArgs e)
    {
        OpenedMultiplePathText.Visibility = Visibility.Collapsed;

        OpenFileDialog dialog = new()
        {
            Multiselect = true,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Filter = "All files (*.*)|*.*",
        };

        if (dialog.ShowDialog() != true || dialog.FileNames.Length == 0)
        {
            return;
        }

        OpenedMultiplePathText.Text = $"已选择 {dialog.FileNames.Length} 个文件：\n{string.Join("\n", dialog.FileNames)}";
        OpenedMultiplePathText.Visibility = Visibility.Visible;
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        OpenedFolderPathText.Visibility = Visibility.Collapsed;

        OpenFolderDialog dialog = new()
        {
            Multiselect = true,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };

        if (dialog.ShowDialog() != true || dialog.FolderNames.Length == 0)
        {
            return;
        }

        OpenedFolderPathText.Text = $"已选择：\n{string.Join("\n", dialog.FolderNames)}";
        OpenedFolderPathText.Visibility = Visibility.Visible;
    }

    private async void SaveFile_Click(object sender, RoutedEventArgs e)
    {
        SavedFileNoticeText.Visibility = Visibility.Collapsed;

        SaveFileDialog dialog = new()
        {
            Filter = "Text Files (*.txt)|*.txt",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };

        string fileName = FileToSaveNameTextBox.Text;
        if (!string.IsNullOrEmpty(fileName))
        {
            char[] invalidChars = Path.GetInvalidFileNameChars()
                .Concat(Path.GetInvalidPathChars())
                .Distinct()
                .ToArray();

            dialog.FileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        if (File.Exists(dialog.FileName))
        {
            return;
        }

        try
        {
            await File.WriteAllTextAsync(dialog.FileName, FileToSaveContentsTextBox.Text);
        }
        catch
        {
            return;
        }

        SavedFileNoticeText.Text = $"已保存：{dialog.FileName}";
        SavedFileNoticeText.Visibility = Visibility.Visible;
    }
}
