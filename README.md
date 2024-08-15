![WPF UI Banner Dark](https://user-images.githubusercontent.com/13592821/174165081-9c62d188-ecb6-4200-abd8-419afbaf32c2.png#gh-dark-mode-only)

![WPF UI Banner Light](https://user-images.githubusercontent.com/13592821/174165388-921c4745-90ed-4396-9a4b-9c86478f7447.png#gh-light-mode-only)

# WPF UI Violeta

[![GitHub license](https://img.shields.io/github/license/emako/wpfui.violeta)](https://github.com/emako/wpfui.violeta/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/v/WPF-UI.Violeta.svg)](https://nuget.org/packages/WPF-UI.Violeta) [![VS 2022 Downloads](https://img.shields.io/visual-studio-marketplace/i/lepo.WPF-UI?label=vs-2022)](https://marketplace.visualstudio.com/items?itemName=lepo.WPF-UI) [![Actions](https://github.com/emako/wpfui.violeta/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/emako/wpfui.violeta/actions/workflows/library.nuget.yml) [![Platform](https://img.shields.io/badge/platform-Windows-blue?logo=windowsxp&color=1E9BFA)](https://dotnet.microsoft.com/zh-cn/download/dotnet/latest/runtime)

WPF UI Violeta is based on [WPF UI](https://github.com/lepoco/wpfui), and provides the Fluent experience in your known and loved WPF framework. Some new immersive controls like `Toast`.

When I decided to create this project I was listening to the song `Violeta`.

### 🚀 Getting started

The same as WPF UI.

```xaml
xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
xmlns:vio="http://schemas.lepo.co/wpfui/2022/xaml/violeta"
```

### 📷 Screenshots

Under construction

### 👋Sample

[Wpf.Ui.Test](https://github.com/emako/wpfui.violeta/tree/master/src/Wpf.Ui.Test)

- Toast

  ```c#
  Toast.Information("I am information message");
  Toast.Error("I am error message");
  Toast.Success("I am success message");
  Toast.Warning("I am warning message");
  Toast.Show(owner: null!, "I am any message",  new ToastConfig());
  ```

- Flyout
  ```xaml
  <ui:Button Content="Show Flyout">
      <ui:FlyoutService.Flyout>
          <ui:Flyout Placement="Bottom">
              <ui:Card>
                  <StackPanel>
                      <TextBlock
                          HorizontalAlignment="Left"
                          Text="Show the flyout message here" />
                      <Button
                         Command="{Binding GotItCommand}"
                         Content="Got it" />
                  </StackPanel>
              </ui:Card>
          </ui:Flyout>
      </ui:FlyoutService.Flyout>
  </ui:Button>
  ```


### Thanks

- [🔗 Fischless](https://github.com/GenshinMatrix/Fischless)
- [🔗 ModernWpf](https://github.com/Kinnara/ModernWpf)

