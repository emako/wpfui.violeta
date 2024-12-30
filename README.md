![WPF UI Banner Dark](https://user-images.githubusercontent.com/13592821/174165081-9c62d188-ecb6-4200-abd8-419afbaf32c2.png#gh-dark-mode-only)

![WPF UI Banner Light](https://user-images.githubusercontent.com/13592821/174165388-921c4745-90ed-4396-9a4b-9c86478f7447.png#gh-light-mode-only)

# WPF UI Violeta

[![GitHub license](https://img.shields.io/github/license/emako/wpfui.violeta)](https://github.com/emako/wpfui.violeta/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/v/WPF-UI.Violeta.svg)](https://nuget.org/packages/WPF-UI.Violeta) [![VS 2022 Downloads](https://img.shields.io/visual-studio-marketplace/i/lepo.WPF-UI?label=vs-2022)](https://marketplace.visualstudio.com/items?itemName=lepo.WPF-UI) [![Actions](https://github.com/emako/wpfui.violeta/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/emako/wpfui.violeta/actions/workflows/library.nuget.yml) [![Platform](https://img.shields.io/badge/platform-Windows-blue?logo=windowsxp&color=1E9BFA)](https://dotnet.microsoft.com/zh-cn/download/dotnet/latest/runtime)

WPF UI Violeta is based on [WPF UI](https://github.com/lepoco/wpfui), and provides the Fluent experience in your known and loved WPF framework. Some new immersive controls like `Toast`, `Flyout`, `ContentDialog`, `MessageBox` and etc.

Some idea or codes are ported from [ModernWpf](https://github.com/Kinnara/ModernWpf) and [Fischless](https://github.com/GenshinMatrix/Fischless).

When I decided to create this project I was listening to the song `Violeta`.

### 🚀 Getting started

Similar to WPF UI.

```xaml
<Application
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
    xmlns:vio="http://schemas.lepo.co/wpfui/2022/xaml/violeta">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ui:ThemesDictionary Theme="Dark" />
                <ui:ControlsDictionary />
                <vio:ThemesDictionary Theme="Dark" />
                <vio:ControlsDictionary />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### 👋Examples

[Wpf.Ui.Test](https://github.com/emako/wpfui.violeta/tree/master/src/Wpf.Ui.Test)

- **Toast**

  > `Toast` is an independent popup notification that automatically disappears after a specified time.

  ```c#
  Toast.Information("I am information message");
  Toast.Error("I am error message");
  Toast.Success("I am success message");
  Toast.Warning("I am warning message");
  Toast.Show(owner: null!, "I am any message",  new ToastConfig());
  ```

- **Flyout**
  
  > The `FlyoutService` enables you to attach `Flyout` menus or tooltips to various controls such as `Button`, providing a flexible and intuitive way to display additional content or options.
  
  ```xaml
  <ui:Button Content="Show Flyout">
      <ui:FlyoutService.Flyout>
          <ui:Flyout Placement="Bottom">
              <StackPanel>
                  <TextBlock
                      HorizontalAlignment="Left"
                      Text="Show the flyout message here" />
                   <Button
                     Command="{Binding GotItCommand}"
                     Content="Got it" />
              </StackPanel>
          </ui:Flyout>
      </ui:FlyoutService.Flyout>
  </ui:Button>
  ```
  
- **ContentDialogHostService**

  > The `ContentDialogHostService` simplifies the creation and management of `ContentDialog` instances in your application.

  ```c#
  Wpf.Ui.Controls.ContentDialog dialog = new()
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
  await dialog.ShowAsync(CancellationToken.None);
  ```
  
- **ContentDialog**

  > The new `ContentDialog` is easy to use with smooth transitions.

  ```c#
  global using ContentDialog = Wpf.Ui.Violeta.Controls.ContentDialog;
  global using ContentDialogButton = Wpf.Ui.Violeta.Controls.ContentDialogButton;
  
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
  ```
  

​	If you want to inherit `Wpf.Ui.Violeta.Controls.ContentDialog` to implement a custom dialog just using `Style="{StaticResource DefaultVioletaContentDialogStyle}"`.

- **MessageBox**

  > To utilize Win32's classic `MessageBox` methods while supporting modern UI themes like Mica and dark mode.

  ```c#
  global using MessageBox = Wpf.Ui.Violeta.Controls.MessageBox;
  
  // Sync methods
  _ = MessageBox.Information("This is a information message");
  _ = MessageBox.Warning("This is a warning message");
  _ = MessageBox.Error("This is a error message");
  MessageBoxResult result =  MessageBox.Question("This is a question and do you want to click OK?");
  
  // Async methods
  _ = await MessageBox.InformationAsync("This is a information message");
  _ = await MessageBox.WarningAsync("This is a warning message");
  _ = await MessageBox.ErrorAsync("This is a error message");
  MessageBoxResult result = await MessageBox.QuestionAsync("This is a question and do you want to click OK?");
  ```

- **PendingBox**

  > Keep displaying 'Loading' until released.

  ```c#
  // Default style.
  using IPendingHandler pending = PendingBox.Show();
  
  // Show with title and cancel button.
  using IPendingHandler pending = PendingBox.Show("Doing something", "I'm a title", isShowCancel: true);
  ```


- **ToggleButtonGroup** / **RadioButtonGroup** / **MenuItemGroup**

  > Turn the ToggleButton and RadioButton under the same Group into a radio button.

  ```xaml
  <StackPanel Orientation="Horizontal">
      <StackPanel.Resources>
          <vio:ToggleButtonGroup x:Key="ToggleButtonGroup" />
      </StackPanel.Resources>
      <ToggleButton
          vio:ToggleButtonGroup.Group="{DynamicResource ToggleButtonGroup}"
          Content="1st"
          IsChecked="True" />
      <ToggleButton
          vio:ToggleButtonGroup.Group="{DynamicResource ToggleButtonGroup}"
          Content="2nd" />
  </StackPanel>
  ```
  ```xaml
  <StackPanel Orientation="Horizontal">
      <StackPanel.Resources>
          <vio:RadioButtonGroup x:Key="RadioButtonGroup" />
      </StackPanel.Resources>
      <RadioButton
                   vio:RadioButtonGroup.Group="{DynamicResource RadioButtonGroup}"
                   Content="1st"
                   IsChecked="True" />
      <Grid>
          <RadioButton
                       Margin="8,0,0,0"
                       vio:RadioButtonGroup.Group="{DynamicResource RadioButtonGroup}"
                       Content="2nd" />
      </Grid>
  </StackPanel>
  ```
  
- **Splash**

  > Show the Splash Screen in another UI thread.

  ```c#
  public App()
  {
      Splash.ShowAsync("pack://application:,,,/Wpf.Ui.Test;component/wpfui.png");
      InitializeComponent();
  }
  
  public MainWindow()
  {
      InitializeComponent();
      Splash.CloseOnLoaded(this, minimumMilliseconds: 1800);
  }
  ```

- **TreeListView**

  > TreeListView is a better way to display hierarchical data.

  ```xaml
  <ui:TreeListView ItemsSource="{Binding StaffList}">
      <ui:TreeListView.Columns>
          <GridViewColumnCollection>
              <ui:GridViewColumn Width="400" Header="Name">
                  <ui:GridViewColumn.CellTemplate>
                      <DataTemplate>
                          <ui:TreeRowExpander Content="{Binding Name}" />
                      </DataTemplate>
                  </ui:GridViewColumn.CellTemplate>
              </ui:GridViewColumn>
              <ui:GridViewColumn
                                 Width="80"
                                 DisplayMemberBinding="{Binding Age}"
                                 Header="Age" />
              <ui:GridViewColumn
                                 Width="80"
                                 DisplayMemberBinding="{Binding Sex}"
                                 Header="Sex" />
              <ui:GridViewColumn
                                 Width="100"
                                 DisplayMemberBinding="{Binding Duty}"
                                 Header="Duty" />
              <ui:GridViewColumn Width="250" Header="IsChecked">
                  <ui:GridViewColumn.CellTemplate>
                      <DataTemplate>
                          <ui:ToggleSwitch IsChecked="{Binding IsChecked}" />
                      </DataTemplate>
                  </ui:GridViewColumn.CellTemplate>
              </ui:GridViewColumn>
          </GridViewColumnCollection>
      </ui:TreeListView.Columns>
      <ui:TreeListView.ItemTemplate>
          <HierarchicalDataTemplate ItemsSource="{Binding StaffList}" />
      </ui:TreeListView.ItemTemplate>
  </ui:TreeListView>
  ```

  ```c#
  public partial class ViewModel : ObservableObject
  {
      [ObservableProperty]
      private ObservableCollection<Staff> staffList = [];
      
      public void InitNode1Value()
      {
          Staff staff = new Staff()
          {
              Name = "Alice",
              Age = 30,
              Sex = "Male",
              Duty = "Manager",
              IsExpanded = true
          };
          Staff staff2 = new Staff()
          {
              Name = "Alice1",
              Age = 21,
              Sex = "Male",
              Duty = "Normal",
              IsExpanded = true
          };
          Staff staff3 = new Staff()
          {
              Name = "Alice11",
              Age = 21,
              Sex = "Male",
              Duty = "Normal"
          };
          staff2.StaffList.Add(staff3);
          staff3 = new Staff()
          {
              Name = "Alice22",
              Age = 21,
              Sex = "Female",
              Duty = "Normal"
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
      private ObservableCollection<Staff> staffList = [];
  }
  ```
  
- **TreeModelListView**

  > TreeModelListView is a fast tree list view used `IEnumerable` and `ITreeModel` to display data but CURD is not fully supported.

  ```xaml
  <ui:TreeModelListView Model="{Binding TreeTestModel}">
   <ui:GridView>
       <ui:GridView.Columns>
           <ui:GridViewColumn Width="400" Header="Column1">
               <ui:GridViewColumn.CellTemplate>
                   <DataTemplate>
                       <ui:TreeModelRowExpander Content="{Binding Column1}" />
                   </DataTemplate>
               </ui:GridViewColumn.CellTemplate>
           </ui:GridViewColumn>
           <ui:GridViewColumn
               Width="250"
               DisplayMemberBinding="{Binding Column2, Mode=TwoWay}"
               Header="Column2" />
           <ui:GridViewColumn
               Width="250"
               DisplayMemberBinding="{Binding Column3, Mode=TwoWay}"
               Header="Column3" />
           <ui:GridViewColumn Width="250" Header="IsChecked">
               <ui:GridViewColumn.CellTemplate>
                   <DataTemplate>
                       <ui:ToggleSwitch IsChecked="{Binding IsChecked}" />
                   </DataTemplate>
               </ui:GridViewColumn.CellTemplate>
           </ui:GridViewColumn>
       </ui:GridView.Columns>
   </ui:GridView>
  </ui:TreeModelListView>
  ```

  ```c#
  public partial class ViewModel : ObservableObject
  {
      [ObservableProperty]
      private TreeCollection<TreeTestModel> treeTestModel = CreateTestModel();
      
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
  ```

- **ImageView**

  > Provides a scalable image control.

  ```xaml
  <vio:ImageView Source="/wpfui.png" />
  ```

- **ExceptionReport**

  > Show a dialog to handle the `DispatcherUnhandledException` from Application.

   ```c#
  public partial class App : Application
  {
      public App()
      {
          InitializeComponent();
  
          DispatcherUnhandledException += (object s, DispatcherUnhandledExceptionEventArgs e) =>
          {
              e.Handled = true;
              ExceptionReport.Show(e.Exception);
          };
      }
  }
   ```

- **BitmapIcon**

  > Supports to show monochrome image that match the theme color.

   ```xaml
  <ui:BitmapIcon
                 ShowAsMonochrome="False"
                 UriSource="pack://application:,,,/Wpf.Ui.Test;component/Resources/Images/Tiara.png" />
  <ui:BitmapIcon
                 ShowAsMonochrome="True"
                 UriSource="pack://application:,,,/Wpf.Ui.Test;component/Resources/Images/Tiara.png" />
   ```



### 📷 Screenshots

Under construction

### Thanks

- [🔗 WPF-UI](https://github.com/lepoco/wpfui)
- [🔗 Fischless](https://github.com/GenshinMatrix/Fischless)
- [🔗 ModernWpf](https://github.com/Kinnara/ModernWpf)
- [🔗 TreeListView](https://www.codeproject.com/Articles/30721/WPF-TreeListView-Control)
- [🔗 CachedImage](https://github.com/floydpink/CachedImage)
- [🔗 WpfAutoGrid.Core](https://github.com/budul100/WpfAutoGrid.Core)

