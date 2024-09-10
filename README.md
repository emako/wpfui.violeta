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
  
- **ContentDialog**

  > The `ContentDialogHostService` simplifies the creation and management of `ContentDialog` instances in your application.

  ```c#
  ContentDialog dialog = new()
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

- **ToggleButtonGroup** / **RadioButtonGroup**

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
  <ui:TreeListView Model="{Binding TreeTestModel}">
   <ui:GridView>
       <ui:GridView.Columns>
           <ui:GridViewColumn Width="400" Header="Column1">
               <ui:GridViewColumn.CellTemplate>
                   <DataTemplate>
                       <ui:TreeRowExpander Content="{Binding Column1}" />
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
  </ui:TreeListView>
  ```

  ```c#
  public partial class ViewModel : ObservableObject
  {
      [ObservableProperty]
      private TreeCollection<TreeTestModel> treeTestModel = CreateTestModel();
      
      public static TreeCollection<TreeTestModel> CreateTestModel()
      {
          return new TreeCollection<TreeTestModel>()
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
  public partial class TreeTestModel : TreeObject<TreeTestModel>
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



### 📷 Screenshots

Under construction

### Thanks

- [🔗 WPF-UI](https://github.com/lepoco/wpfui)
- [🔗 Fischless](https://github.com/GenshinMatrix/Fischless)
- [🔗 ModernWpf](https://github.com/Kinnara/ModernWpf)
- [🔗 TreeListView](https://www.codeproject.com/Articles/30721/WPF-TreeListView-Control)

