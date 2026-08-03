![WPF UI Banner Dark](https://user-images.githubusercontent.com/13592821/174165081-9c62d188-ecb6-4200-abd8-419afbaf32c2.png#gh-dark-mode-only)

![WPF UI Banner Light](https://user-images.githubusercontent.com/13592821/174165388-921c4745-90ed-4396-9a4b-9c86478f7447.png#gh-light-mode-only)

# WPF UI Violeta

[![GitHub license](https://img.shields.io/github/license/emako/wpfui.violeta)](https://github.com/emako/wpfui.violeta/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/v/WPF-UI.Violeta.svg)](https://nuget.org/packages/WPF-UI.Violeta) [![VS 2022 Downloads](https://img.shields.io/visual-studio-marketplace/i/lepo.WPF-UI?label=vs-2022)](https://marketplace.visualstudio.com/items?itemName=lepo.WPF-UI) [![Actions](https://github.com/emako/wpfui.violeta/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/emako/wpfui.violeta/actions/workflows/library.nuget.yml) [![Platform](https://img.shields.io/badge/platform-Windows-blue?logo=windowsxp&color=1E9BFA)](https://dotnet.microsoft.com/zh-cn/download/dotnet/latest/runtime)

WPF UI Violeta is based on [WPF UI](https://github.com/lepoco/wpfui), and provides the Fluent experience in your known and loved WPF framework. Some new immersive controls like `Toast`, `Flyout`, `ContentDialog`, `MessageBox` and etc.

Some idea or codes are ported from [Special Thanks](https://github.com/emako/wpfui.violeta#special-thanks).

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

See the examples from `Wpf.Ui.Violeta.Gallery`.

### 💞Special Thanks

WPF-UI.Violeta is standing on shoulders of several open-source libraries, see [SHOULDERS](SHOULDERS.md).

