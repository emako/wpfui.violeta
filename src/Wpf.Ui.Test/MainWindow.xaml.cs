using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Appearance;
using Wpf.Ui.Violeta.Controls;
using ContentDialog = Wpf.Ui.Violeta.Controls.ContentDialog;
using ContentDialogButton = Wpf.Ui.Violeta.Controls.ContentDialogButton;

namespace Wpf.Ui.Test;

[ObservableObject]
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();

        Thread.Sleep(600);
        Splash.CloseOnLoaded(this, minimumMilliseconds: 1800);

        ScrollViewer.ScrollToEnd();

        InitNode1Value();
    }

    [ObservableProperty]
    private int themeIndex = (int)ApplicationTheme.Dark;

    partial void OnThemeIndexChanged(int value)
    {
        ThemeManager.Apply((ApplicationTheme)value);
    }

    [RelayCommand]
    private void ShowToast(Button self)
    {
        string message = "This is a toast message";
        ToastLocation toastLocation = (ToastLocation)Enum.Parse(typeof(ToastLocation), self.Content.ToString()!);

        if (self.Tag.ToString() == "Information")
        {
            Toast.Information(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Error")
        {
            Toast.Error(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Success")
        {
            Toast.Success(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Warning")
        {
            Toast.Warning(message, toastLocation);
        }
        else if (self.Tag.ToString() == "Question")
        {
            Toast.Question(message, toastLocation);
        }
        else if (self.Tag.ToString() == "None")
        {
            Toast.Show(null!, message, new ToastConfig()
            {
                Location = toastLocation,
            });
        }
    }

    [RelayCommand]
    private void ShowFlyoutInline()
    {
        Toast.Success("The cake is a lie!");
    }

    [RelayCommand]
    private async Task ShowContentDialogAsync()
    {
        ContentDialog dialog = new()
        {
            Title = "My sample dialog",
            Content = "Content of the dialog",
            CloseButtonText = "Close button",
            PrimaryButtonText = "Primary button",
            SecondaryButtonText = "Secondary button",
            DefaultButton = ContentDialogButton.Primary,
        };

        _ = await dialog.ShowAsync();
    }

    [RelayCommand]
    private async Task ShowWithContentPresenterForDialogs()
    {
        Wpf.Ui.Controls.ContentDialog dialog =
            new()
            {
                Title = "My sample dialog",
                Content = "Content of the dialog",
                CloseButtonText = "Close button",
                PrimaryButtonText = "Primary button",
                SecondaryButtonText = "Secondary button"
            };

        // Setting the dialog container
        dialog.DialogHost = ContentDialogHostService.ContentPresenterForDialogs;

        // Showing the dialog
        _ = await dialog.ShowAsync(CancellationToken.None);
    }

    [RelayCommand]
    private void ShowMessageBox(Button self)
    {
        if (self.Content.ToString() == "Information")
        {
            _ = MessageBox.Information("This is a information message");
        }
        else if (self.Content.ToString() == "Warning")
        {
            _ = MessageBox.Warning("This is a warning message");
        }
        else if (self.Content.ToString() == "Question")
        {
            _ = MessageBox.Question("This is a question and do you want to click OK?");
        }
        else if (self.Content.ToString() == "Error")
        {
            _ = MessageBox.Error(
                """
                Dummy exception from Violeta:
                   at Violeta.View.MainWindow.OnNotifyIconLeftDoubleClick(NotifyIcon sender, RoutedEventArgs e) in D:\GitHub\Violeta\View\MainWindow.xaml.cs:line 53
                   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
                   at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)
                """
            );
        }
    }

    [RelayCommand]
    private async Task AsyncShowMessageBoxAsync(Button self)
    {
        if (self.Content.ToString() == "Information")
        {
            _ = await MessageBox.InformationAsync("This is a information message");
        }
        else if (self.Content.ToString() == "Warning")
        {
            _ = await MessageBox.WarningAsync("This is a warning message");
        }
        else if (self.Content.ToString() == "Question")
        {
            _ = await MessageBox.QuestionAsync("This is a question and do you want to click OK?");
        }
        else if (self.Content.ToString() == "Error")
        {
            _ = await MessageBox.ErrorAsync("This is a error message");
        }
    }

    [ObservableProperty]
    private RegistryModel treeRegistryModel = new();

    [ObservableProperty]
    private FileModel treeFileModel = new();

    [ObservableProperty]
    private TreeModelCollection<TreeTestModel> treeTestModel = CreateTestModel();

    [RelayCommand]
    private void AddTreeTestModel()
    {
        TreeTestModel.Add(new TreeTestModel()
        {
            Column1 = "Test Added " + DateTime.Now,
            Column2 = "Test Added " + DateTime.Now,
            Column3 = "Test Added " + DateTime.Now,
        });

        TreeTestModel.Children[0].Children.Add(new TreeTestModel()
        {
            Column1 = "Test Added " + DateTime.Now,
            Column2 = "Test Added " + DateTime.Now,
            Column3 = "Test Added " + DateTime.Now,
        });

        TreeTestModel.Children[0].Children[0].Children.Add(new TreeTestModel()
        {
            Column1 = "Test Added " + DateTime.Now,
            Column2 = "Test Added " + DateTime.Now,
            Column3 = "Test Added " + DateTime.Now,
        });
    }

    [RelayCommand]
    private void RemoveTreeTestModel()
    {
        if (TreeTestModel.Count > 0)
        {
            TreeTestModel.RemoveAt(0);
        }
    }

    [RelayCommand]
    private void ChangeTreeTestModel()
    {
        if (TreeTestModel.FirstOrDefault() is TreeTestModel model)
        {
            model.Column1 = "Test Changed " + DateTime.Now;
            model.Column2 = "Test Changed " + DateTime.Now;
            model.Column3 = "Test Changed " + DateTime.Now;
        }
    }

    [RelayCommand]
    private void ClearTreeTestModel()
    {
        TreeTestModel.Clear();
    }

    public static TreeModelCollection<TreeTestModel> CreateTestModel()
    {
        return new TreeModelCollection<TreeTestModel>()
        {
            Children = new(
            [
                new()
                {
                    Column1 = "Test 1",
                    Column2 = "Test 1",
                    Column3 = "Test 1",
                    Children = new(
                    [
                        new()
                        {
                            Column1 = "Test 1.1",
                            Column2 = "Test 1.1",
                            Column3 = "Test 1.1",
                            Children = new(
                            [
                                new()
                                {
                                    Column1 = "Test 1.2",
                                    Column2 = "Test 1.2",
                                    Column3 = "Test 1.2",
                                },
                            ]),
                        },
                    ]),
                },
                new()
                {
                    Column1 = "Test 2",
                    Column2 = "Test 2",
                    Column3 = "Test 2",
                }
            ]),
        };
    }

    public static TreeModelCollection<TreeTestModel> CreateTestModel(int count1, int count2, int count3)
    {
        TreeModelCollection<TreeTestModel> model = [];

        for (int i = 0; i < count1; i++)
        {
            TreeTestModel p = new()
            {
                Column1 = "Person A " + i.ToString(),
                Column2 = "Column2 A",
                Column3 = "Column3 A",
            };
            model.Children.Add(p);

            for (int n = 0; n < count2; n++)
            {
                TreeTestModel p2 = new()
                {
                    Column1 = "Person B" + n.ToString(),
                    Column2 = "Column2 B",
                    Column3 = "Column3 B",
                };
                p.Children.Add(p2);

                for (int k = 0; k < count3; k++)
                {
                    p2.Children.Add(new TreeTestModel()
                    {
                        Column1 = "Person C" + k.ToString(),
                        Column2 = "Column2 C",
                        Column3 = "Column3 C",
                    });
                }
            }
        }
        return model;
    }

    [ObservableProperty]
    private ObservableCollection<Staff> staffList = [];

    [ObservableProperty]
    private Staff selectedStaffItem = null!;

    private void InitNode1Value()
    {
        Staff staff = new Staff()
        {
            Name = "Alice",
            Age = 30,
            Sex = "Male",
            Duty = "Manager",
            IsExpanded = true,
            Path = @"C:\Program Files\nodejs\",
        };
        Staff staff2 = new Staff()
        {
            Name = "Alice1",
            Age = 21,
            Sex = "Male",
            Duty = "Normal",
            IsExpanded = true,
            Path = @"C:\Program Files\nodejs\node.exe",
        };
        Staff staff3 = new Staff()
        {
            Name = "Alice11",
            Age = 21,
            Sex = "Male",
            Duty = "Normal",
            Path = @"D:\UI_MonsterSmallIcon_Eremite_Male_Standard_Crossbow.png",
        };
        staff2.StaffList.Add(staff3);
        staff3 = new Staff()
        {
            Name = "Alice22",
            Age = 21,
            Sex = "Female",
            Duty = "Normal",
            Path = @"C:\Program Files\ASUS\ARMOURY CRATE Service\GameVisualPlugin\GameVisual2.ico",
        };
        staff2.StaffList.Add(staff3);
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Alice2",
            Age = 22,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Alice3",
            Age = 23,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        StaffList.Add(staff);

        staff = new Staff()
        {
            Name = "Bob",
            Age = 31,
            Sex = "Male",
            Duty = "CEO"
        };
        staff2 = new Staff()
        {
            Name = "Bob1",
            Age = 24,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Bob2",
            Age = 25,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Bob3",
            Age = 26,
            Sex = "Male",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        StaffList.Add(staff);

        staff = new Staff()
        {
            Name = "Cyber",
            Age = 32,
            Sex = "Female",
            Duty = "Leader"
        };
        staff2 = new Staff()
        {
            Name = "Cyber1",
            Age = 27,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        staff2 = new Staff()
        {
            Name = "Cyber2",
            Age = 28,
            Sex = "Female",
            Duty = "Normal"
        };
        staff.StaffList.Add(staff2);
        StaffList.Add(staff);
    }

    [RelayCommand]
    private void AddNode1Value()
    {
        Staff staff = new()
        {
            Name = "Alice",
            Age = 30,
            Sex = "Male",
            Duty = "Manager",
            IsExpanded = true
        };
        Staff staff2 = new()
        {
            Name = "Alice1",
            Age = 21,
            Sex = "Male",
            Duty = "Normal",
            IsExpanded = true
        };
        Staff staff3 = new()
        {
            Name = "Alice11",
            Age = 21,
            Sex = "Male",
            Duty = "Normal"
        };
        staff2.StaffList.Add(staff3);
        staff.StaffList.Add(staff2);
        StaffList.Add(staff2);
    }

    [RelayCommand]
    private void ChangeNode1Value()
    {
        foreach (Staff staff in StaffList)
        {
            staff.Age += 1;
            staff.Sex = staff.Sex == "Male" ? "Female" : "Male";
        }
    }

    [RelayCommand]
    private void ChangeNode2Value()
    {
        foreach (Staff staff in StaffList)
        {
            foreach (Staff staff2 in staff.StaffList)
            {
                staff2.Age += 1;
                staff2.Sex = staff2.Sex == "Male" ? "Female" : "Male";
            }
        }
    }

    [RelayCommand]
    private void OpenStaff()
    {
        // Use `TreeListView::SelectedItem` here ...
        _ = SelectedStaffItem;
    }

    [RelayCommand]
    private void ShowReport()
    {
        ExceptionReport.Show(new SystemException(
            """
            A critical system error occurred while attempting to perform the requested operation.
            The system entered an unexpected state, possibly due to resource exhaustion,
            incompatible configuration settings, or an internal logic flaw.
            Immediate investigation is required to diagnose and rectify the issue.
            Please check system logs, review recent changes,
            and ensure that the environment meets all necessary requirements.
            If the issue cannot be resolved, escalate to the technical team for further analysis.
            """
            ));
    }

    [RelayCommand]
    private void ThrowException()
    {
        throw new InvalidOperationException("The operation could not be completed because the system encountered an unexpected state. This might be due to incorrect usage of the API or an internal error. Please ensure that all prerequisites are met and the operation is performed under the correct conditions. If the problem persists, consult the documentation or contact support for further assistance.");
    }

    [RelayCommand]
    private async Task ShowPendingBoxAsync()
    {
        using IPendingHandler pending = PendingBox.Show();
        await Task.Delay(3000);
    }

    [RelayCommand]
    private async Task ShowPendingBoxWithCancelAsync()
    {
        using IPendingHandler pending = PendingBox.Show("Doing something", "I'm a title", isShowCancel: true);
        await Task.Delay(3000);
    }
}

public partial class RegistryModel : ITreeModel
{
    public IEnumerable GetChildren(object parent)
    {
        if (parent == null)
        {
            yield return Registry.ClassesRoot;
            yield return Registry.CurrentUser;
            yield return Registry.LocalMachine;
            yield return Registry.Users;
            yield return Registry.CurrentConfig;
        }
        else if (parent is RegistryKey key)
        {
            foreach (var name in key.GetSubKeyNames())
            {
                RegistryKey? subKey = null;
                try
                {
                    subKey = key.OpenSubKey(name);
                }
                catch
                {
                }

                if (subKey != null)
                {
                    yield return subKey;
                }
            }

            foreach (var name in key.GetValueNames())
            {
                yield return new RegValue()
                {
                    Name = name,
                    Data = key.GetValue(name),
                    Kind = key.GetValueKind(name)
                };
            }
        }
    }

    public bool HasChildren(object parent)
    {
        return parent is RegistryKey;
    }
}

public class FileModel : ITreeModel
{
    public IEnumerable GetChildren(object? parent)
    {
        if (parent == null)
        {
            // Return the root directory
            yield return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        }
        else if (parent is DirectoryInfo directory)
        {
            // Return subdirectories
            foreach (var dir in directory.GetDirectories())
            {
                yield return dir;
            }

            // Return files
            foreach (var file in directory.GetFiles())
            {
                yield return file;
            }
        }
    }

    public bool HasChildren(object parent)
    {
        if (parent is DirectoryInfo directory)
        {
            try
            {
                // Check if there are subdirectories or files
                return directory.GetDirectories().Any() || directory.GetFiles().Any();
            }
            catch (UnauthorizedAccessException)
            {
                // Catch access denied exception and return false
                return false;
            }
            catch (Exception ex)
            {
                // Handle other possible exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
        return false;
    }
}

public struct RegValue
{
    public string Name { get; set; }
    public object? Data { get; set; }
    public RegistryValueKind Kind { get; set; }
}

[ObservableObject]
public partial class TreeTestModel : TreeModelObject<TreeTestModel>
{
    [ObservableProperty]
    private string? column1;

    [ObservableProperty]
    private string? column2;

    [ObservableProperty]
    private string? column3;

    [ObservableProperty]
    private bool isChecked = false;
}

public partial class Staff : ObservableObject
{
    [ObservableProperty]
    private string name = null!;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private string sex = null!;

    [ObservableProperty]
    private string duty = null!;

    [ObservableProperty]
    private bool isChecked = true;

    [ObservableProperty]
    private bool isSelected = false;

    [ObservableProperty]
    private bool isExpanded = false;

    [ObservableProperty]
    private string path = null!;

    [ObservableProperty]
    private ObservableCollection<Staff> staffList = [];

    [RelayCommand]
    private void OnClick()
    {
    }
}
