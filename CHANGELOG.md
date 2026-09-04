# Changelog

All notable changes to [WPF-UI.Violeta](https://github.com/emako/wpfui.violeta) are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Release notes are aggregated from [GitHub Releases](https://github.com/emako/wpfui.violeta/releases).

## [4.3.0.12] - 2026-09-04

* Add PopConfirmButton control
* Add TabsTitleControl and gallery sample
* Add FlexPanel control with StretchItems mode and gallery samples
* Add Growl control and gallery integration
* Add Descriptions control and gallery page
* Add Gravatar control and gallery sample
* Add Shield control inspired by HandyControl shields.io badges
* Add BorderBeam control and Spin button mode
* Add ContentWindowDialogControl with BusyMask, button alignment, and gallery sample
* Add attached drag-and-drop framework and gallery sample
* Add theme dictionary registration system
* Add EmptySimpleLogo brush and gallery sample
* Add vertical orientation support to Segmented and TabStrip
* Animate Segmented indicator and selection chrome
* Add configurable tab content transitions
* Animate tabs scrollbar hover visibility
* Add slider AutoToolTip hover, prefix/suffix, and RangeSlider fluent styling
* Add command and shortcut text support to ComboBoxItem
* Add command support to SegmentedItem selection
* Support ToggleComboBox in toggle groups (default non-cancelable)
* Add Width support to SymbolIconExtension and hotfix UTF-32-safe SymbolIcon
* Make FormItem label/field alignment follow content alignment properties
* Refine ColorPicker popup/preview UI and ColorView layout
* Use Segmented for ColorView mode switch
* Fix ColorPicker flyout closing when switching RGB/HSV (nested ComboBox popup)
* Modernize emoji module with theme dictionaries and registration
* Use localized SH resources for MessageBox/PendingBox buttons
* Register Violeta XAML namespaces
* Collapse dialog command area when no buttons are shown
* Keep port binding in sync when cleared
* Fix IPv4Box/IPv4PortBox showing empty when octet is padded to 0
* Fix ToggleComboBox selected item display
* Fix TimePickerPresenter not keeping Time in sync with panels
* Fix NavigationView pill sync on search select
* Fix StackPanel measure throwing on collapsed-only children
* Guard SplitButton/DropDownButton open and release behavior
* Fix exception window button icon styling
* Improve TabStrip accent indicator and button animations
* Refine ToolBar spacing and collapsed layout
* Move required asterisk left of label and reserve placeholder space
* Inline ripple adorner into ThemeSwitchEffect
* Remove ThemeBinding markup extension; refactor theme refresh converter flow
* Rename language manager to locale manager
* Merge pivot tab helpers into PivotHelper
* Make splash settings instance-based options

## [4.3.0.11] - 2026-08-25

* Revert Grid definition DP init for XAML shorthand syntax (fixes PendingBox dialog layout)
* Fix Grid gallery form sample to use standard Grid.ColumnDefinitions syntax
* Align gallery publish output directory

## [4.3.0.10] - 2026-08-25

* Add animated NumberDisplayer control
* Add IconToggleButton control with IsCheckedChanged event
* Add IsFocusOnLoaded attached property to ControlHelper
* Add toolbar style for ui:Button
* Add card styles for Wpf.Ui Border control
* Enable bindable window and grid item titles
* VirtualJoystick: Make dead zone adjustable ([#43](https://github.com/emako/wpfui.violeta/issues/43))
* Switch ColorPicker to Fluent color palette
* Hotfix TabControl TabStripPlacement layouts
* Hotfix PasswordBox width template binding
* Fix disabled DataGrid header gripper artifacts
* Keep notification host alive until app exit
* Fix SplitButton hover/press fills
* Fix button and ToggleButton foreground for rich content states
* Fix Border corner clipping in narrow layout slots
* Fix disabled DropDownButton styling and chevron spacing
* Refactor ToggleComboBox state styling
* Refine toolbar control disabled styling
* Localize GoBackButton default tooltip
* Add Grid layout, Banner color, and ToggleButton gallery samples
* Document drawer background brush usage

## [4.3.0.9] - 2026-08-21

* Add StatusBar gallery sample
* Use theme accent colors for BusyMask / Indicator
* Improve callout tooltip style and samples
* Refine ColorView input row layout

## [4.3.0.8] - 2026-08-19

* Add Wpf.Ui.Emoji library and gallery sample
* Add Segmented control and gallery sample
* Add CardProgress control and gallery samples
* Add NumberComboBox control and gallery samples
* Enhance NumberComboBox item and unit UX
* Add VolumeView control and gallery sample
* Hotfix ProgressBar with WinUI state support
* Unify Violeta theme handling and events
* Align Drawer animation and toggle behavior
* Make exception details selectable in ExceptionReport
* Add Tile press feedback gallery sample
* Remove obsolete Wpf.Ui test demo projects

## [4.3.0.7] - 2026-08-14

* Add Notification helper and gallery demo
* Add CopyButton control and gallery sample
* Add CardCarousel control with seamless circular wrapping
* Add GoBackButton and AnimatedSymbolButton controls
* Add Wpf.Ui.TextEditor library and CodeBox
* Add WinUI-style GridView and gallery page
* Add Pivot TabControl style and gallery sample
* Add reusable Card and CardBorder styles
* Replace SplitToggleButton with ToggleComboBox
* Animate DropDownButton and SplitButton chevrons
* Toggle DropDownButton flyout on click
* Add ToolBarSplitButton and ToolBarDropDownButton styles
* Add toolbar styles for CopyButton and GoBackButton
* Fix toolbar overflow on initial layout
* Add FontIcon markup extension with Width support
* Support string glyph icons for MenuItem
* Add icon font size styling via ControlHelper
* Add drag-move attached property to ControlHelper
* Add inactive appearance toggle for title bar
* Fix custom footer hit testing in TitleBar
* Fix dropdown flyout shadow text blurring
* Fix menu popup blur and submenu layout
* Align MenuItem submenu popup placement
* Enhance SwatchPicker layout and selection UX
* Add tray icon twinkle mode support
* Add TabStrip separator visibility toggle
* Hotfix ListViewItem, DataGrid, and TreeViewItem pill animations
* Align tree and list view chrome with WPF-UI
* Unify ControlHelper and use Border.CornerRadius across controls
* Remove obsolete compat helpers and share Win32 interop

## [4.3.0.6] - 2026-08-10

* Add ComboBox placeholder support and style fix
* Expose caption button state to subclasses
* Use ShellWindow for exception dialog
* Expand Colors page with full palette showcase
* Make color keys selectable in ColorsPage
* Expand icons gallery page
* Add ContentWindow link to caption bar page
* Zip gallery publish output in build script

## [4.3.0.5] - 2026-08-10

* Add VirtualJoystick control
* Refactor ContentWindow with built-in TitleBar
* Expose ContentWindow chrome options
* Add icon/title visibility support to TitleBar
* Add More caption button support
* Fix title bar title character ellipsis
* Align PendingBox dialog title with TitleBar style
* Adjust PendingBox dialog layout and font style
* Add MakeKits packaging for Gallery build
* Add backdrop support to ContentWindow demo
* Add ContentWindow link on TitleBar page

## [4.3.0.4] - 2026-08-07

* Add TeachingTip control
* Add ColorPicker control
* Add SearchBox control
* Add Carousel control
* Add ToggleComboBox control
* Add SwatchPicker control
* Add Win32 CredentialDialog
* Add Win32 OpenFolderDialog support
* Build native Violeta DropDownButton and SplitButton
* Add DWM non-client rendering toggle API
* Add NavigationView search box support
* Align NavigationView pane toggle layout
* Adjust NavigationView back button states
* Support button animation for NavigationView
* Stretch NavigationView content
* Fix usage of TransitioningContentControl
* Add press animations to title bar buttons
* ShellWindow no longer inherits window styles
* Fix taskbar cant send WS_MINIMIZEBOX for ShellWindow
* Unify Win32 interop and fix caption commands
* Expose WINDOWPLACEMENT as public interop type
* Add NumberBox hotfix resource dictionary
* Hotfix ComboBox padding clipping
* Add ToggleButton template hotfix resource
* Fix RadioButtonGroup stack overflow loop
* Apply HarmonyOS font and tooltip hotfix
* Adjust NumericUpDown spinner panel margin
* Fix ButtonSpinner hover inset
* Sync spinner stroke with button state
* Fix form item alignment
* Adjust TagComboBox padding and placeholder color
* Fix TreeComboBox clear button layout spacing
* Handle cascading selection text in code-behind
* Fix PendingBoxDialog title font size
* Add toolbar overflow flyout
* Add toolbar overflow auto-close modes
* Allow configuring overflow auto-close types
* Add toolbar toggle button style resources
* Add toolbar split toggle button style
* Add item-count wrapping to overflow panel
* Add configurable close-to-tray behavior
* Mark SmoothScrollViewer obsolete
* Add multilingual localization to Gallery UI
* Add accent color controls to settings
* Add real tray icon flow to Gallery app
* Add WebView2 sample page to Gallery

## [4.3.0.3] - 2026-07-22

* Fixed NavigationView unable to render Page content
* Refactor toast stacking to per-toast state
* Improve TagComboBox placeholder behavior
* Fix TagComboBox click layering behavior
* Button.xaml: Move Appearance style trigger to Style level for LoadingButton by @Just-Silver 
* Improve that block background scroll in TreeComboBox/DatePicker/TimePicker popup
* Add seconds and AM/PM display to TimePicker
* Animate TabStrip selection indicator
* Add Calendar and DatePicker control styles
* Add CalendarDateTimePicker control
* Add clear button support to CascadingComboBox ([#42](https://github.com/emako/wpfui.violeta/issues/42)) by @huiyadanli 
* Add clear button support to TreeComboBox
* Add TimeBoxPicker and ToggleSwitch control

### Contributors

* @Just-Silver made their first contribution in https://github.com/emako/wpfui.violeta/pull/41

## [4.3.0.2] - 2026-07-07

* New NavigationView by @emako in https://github.com/emako/wpfui.violeta/pull/39
* Fix NavigationView indicator animation [#40](https://github.com/emako/wpfui.violeta/issues/40)
* Add BorderContentAdapter & content clipping
* Fix DatePicker/TimePicker style (repeat buttons and icons)
* Add ValuePicker control with multi-column flyout

### Contributors

* @emako made their first contribution in https://github.com/emako/wpfui.violeta/pull/39

## [4.3.0.1] - 2026-06-16

### Added

#### Controls

- **TreeComboBox** — hierarchical dropdown with level-based indentation
- **PinCode** — PIN entry with multiple modes
- **Skeleton** — skeleton loading placeholder
- **Banner** — informational / warning / error banner
- **QrCode** — QR code control with full encoding pipeline (Reed–Solomon, masking, version tables, etc.)
- **Timeline** — timeline with multiple display modes
- **KeyGestureInput** — keyboard shortcut input with optional clear button
- **Divider** — visual separator
- **Badge** — badge / notification dot
- **AspectRatioLayout** — fixed aspect-ratio layout container
- **LoadingButton** — button with loading state and command simulation
- **FlipView** — paged content view with mouse-wheel navigation and smoother animation
- **FluentPopup** — Fluent-style popup with theme-aware acrylic and dark mode (DWM-backed)
- **FluentScrollViewer** — scroll viewer with configurable scroll physics
- **TagInput** — tag entry with closable tags
- **TagComboBox** — tag-style combo box
- **RangeSlider** — dual-thumb range slider
- **Anchor** — anchor / table-of-contents navigation
- **AcrylicPanel** — acrylic panel (marked as not ready for production)
- **TransitioningContentControl** — animated content transitions
- **TimeBox** — time input control
- **Form**, **FormItem**, **FormGroup** — form layout primitives
- **SelectableTextBlock** — read-only text with selection support
- **NumericUpDown** — numeric stepper (including Double/Decimal variants, prefix/suffix, `RestrictInput`)
- **ButtonSpinner** — spinner with adjacent action buttons
- **BoolStateTextBlock**, **BoolStateContentControl** — boolean-state text and content switching
- **DropShadowChrome** — drop-shadow chrome decorator
- **GridSplitter** — grid splitter (including hidden variant)
- **TabStrip** — tab strip with ghost presenter and ButtonTabStrip styling
- **DatePicker**, **TimePicker**, **DateTimePicker** — date/time pickers with revamped visuals

#### Shapes & Layout

- **Arc** — arc shape
- **Grid** — `HorizontalSpacing` / `VerticalSpacing` attached properties and shorthand Column/Row parsing

#### Styles & Hotfixes

- **Hotfix/Button.xaml** — Button style hotfix
- **Hotfix/ToggleSwitch.xaml** — ToggleSwitch style hotfix
- **TransparentTextBoxStyle** — shared transparent text box style (TagInput, NumericUpDown, etc.)
- **Calendar** — resource dictionary scaffolding (`Calendar.xaml`, `CalendarDatePicker.xaml`)
- **Theme** — extended Dark/Light theme resources

#### Win32 & Low-Level

- **DwmApi** — DWM helpers for acrylic blur, rounded corners, immersive dark mode (used by FluentPopup)
- **Win32 directory reorganization**
  - `Dpi/` — `DpiAware`, `DpiHelper`
  - `NativeTray/` — tray icon, menu, `Win32Icon`, `Win32Image`, etc.
  - `NativeDialog/` — `TaskDialog`
- **AcrylicBrushExtension** — markup extension for acrylic brushes

#### Build, Docs & Tooling

- **build/nuget_push.ps1** — NuGet publish script
- **SHOULDERS.md** — open-source attribution / dependencies list
- **TASK.md** — roadmap and pending work items
- Localization strings for Form across 11 languages

#### Demo

- Expanded **MainWindow** demo (~3,000+ lines of XAML) with section headers and samples for new controls

### Changed

- **FluentScrollViewer** — `IScrollPhysics` members are now public
- **TabStrip** — added `IsSelectedItemBold`; selected state uses accent brushes
- **DateTimePicker** — visual revamp; column order changed to Year–Month–Day; removed fixed TextBlock height
- **TagInput** — simplified null checks and `ICommand` usage; aligned TextBox height with tags
- **GridSplitter** — refactored styles; removed `IsFocused` trigger
- **QrCode** — refactored encoder namespaces
- **Form** — form labels translated to English
- **BitList** — updated implementation
- **CachedImage** — minor adjustments
- **PersonPicture** — `ProfilePicture` now accepts `object` (string or `ImageSource`)
- **CascadingComboBox.xaml** — style tweaks
- **README.md** — updated project description
- Demo **MainWindow** height reduced to 760

### Fixed

- **TaskDialog** — `Log` accepts null parameters
- **PersonPicture** — binding error when a string is bound to `ProfilePicture` ([#38](https://github.com/emako/wpfui.violeta/pull/38))
- **CascadingComboBox** / **TreeComboBox** — toggle overlay display
- **AspectRatioLayout** — control not visible after constructor
- **RunningBlock** — `CornerRadius` binding syntax
- **KeyGestureInput** — clear button margin
- **NumericUpDown** — null-check refactor, placeholder left padding, compiler warnings
- **Banner** — banner ASCII art in NuGet push script

### Removed

- Acrylic demo section from demo app; simplified ScrollViewer demo setup

### Infrastructure

- Version bumped from **4.3.0.0** to **4.3.0.1**
- NuGet push script no longer pushes `.snupkg` symbol packages

## [4.3.0] - 2026-05-11

- No release notes.

## [4.2.1] - 2026-05-11

- No release notes.

## [4.2.0.12] - 2026-05-11

* Add Pagination control
* Add IPv4Box control
* Add TaskDialog control (Experimental)
* Add Hyperlink style
* Add Button.InvokeClick extension via UI Automation
* Fix ExceptionWindow version capture (used Window base class instead of implementation) [#37](https://github.com/emako/wpfui.violeta/issues/37)

## [4.2.0.11] - 2026-04-13

* Sync tray to NativeTray 2.0.3
* Add ThemeSwitchEffect and move ripple adorner

## [4.2.0.10] - 2026-04-08

* Breaking change for CascadingComboBox 


- **CascadingComboBox**

  > `CascadingComboBox` is a multi-level cascading dropdown. Each column displays the children of the selected item in the previous column. The number of visible columns grows automatically as the user navigates. When a leaf node (no children) is selected, the dropdown closes and the value is committed to `SelectedCascadingItem`.

  ```xaml
  <vio:CascadingComboBox
      Width="240"
      HorizontalAlignment="Left"
      ItemsSource="{Binding CascadingComboBoxDemoItems}"
      SelectedCascadingItem="{Binding CascadingComboBoxSelectedValue, Mode=TwoWay}" />
  ```

  ```c#
  
  ```

  `CascadingComboBox` common properties:
  `PlaceholderText` placeholder text when no item is selected.
  `ItemsSource` root-level data source (`IEnumerable<ICascadingItem>`, setting a non-conforming type throws `ArgumentException`).
  `Levels` (read-only) number of columns currently visible in the dropdown.
  `SelectedCascadingItem` the leaf node selected by the user (`ICascadingItem?`, two-way bindable).

## [4.2.0.9] - 2026-04-08

* New CascadingComboBox

## [4.2.0.8] - 2026-04-07

* Fix style for MultiComboBox

## [4.2.0.7] - 2026-04-07

* New MultiComboBox

**MultiComboBox**

  > `MultiComboBox` supports multi-select with built-in select-all and exposes selected values through `MultiSelectedItems`.

  ```xaml
  <StackPanel Orientation="Horizontal">
      <vio:MultiComboBox
          Width="240"
          MaxDropDownHeight="300"
          PlaceholderText="Please select...">
          <vio:MultiComboBoxItem Content="Option A" />
          <vio:MultiComboBoxItem Content="Option B" />
          <vio:MultiComboBoxItem Content="Option C" />
      </vio:MultiComboBox>
  
      <vio:MultiComboBox
          x:Name="MultiComboBoxDemo"
          Width="240"
          Margin="16,0,0,0"
          MaxDropDownHeight="300"
          PlaceholderText="Binding demo (ItemsSource)" />
  </StackPanel>
  ```

  ```c#
  MultiComboBoxDemo.ItemsSource = new[] { "Apple", "Banana", "Cherry", "Durian", "Elderberry" };
  
  MultiComboBoxDemo.MultiSelectedItems.CollectionChanged += (_, _) =>
  {
      string selectedText = MultiComboBoxDemo.MultiSelectedItems.Count == 0
          ? "Selected: (none)"
          : "Selected: " + string.Join(", ", MultiComboBoxDemo.MultiSelectedItems);
  };
  ```

  `MultiComboBox` common properties:
  `PlaceholderText` placeholder text when no item is selected.
  `Separator` text separator used in selected display.
  `SelectAllText` label of the select-all row.
  `IsSelectAllEnabled` whether to show the select-all row.
  `MultiSelectedItems` current selected items collection.

## [4.2.0.6] - 2026-03-31

* New AsyncBox 

```xaml
 <vio:AsyncBox LoadingDelay="1000" LoadingViewType="{x:Type vio:Loading}">
     <TextBlock Text="This is an AsyncBox demo. You can put any content you want in it, and it will automatically show a loading indicator when the content is being loaded asynchronously." />
 </vio:AsyncBox>
```

## [4.2.0.5] - 2026-03-18

* New BusyMask

```xaml
<vio:BusyMask
  Background="Transparent"
  BusyContent="Bar"
  IndicatorType="Bar"
  IsBusy="True">
    <UIElement />
</vio:BusyMask>
```

## [4.2.0.4] - 2026-02-18

* feat: using WPF resource instead of satellite assemblies.

## [4.2.0.3] - 2026-02-12

* feat: migrate RunningBlock from HandyControl

## [4.2.0.2] - 2026-02-11

* feat: add DependencyProperty source generator

## [4.2.0.1] - 2026-01-26

* fix: ExceptionWindow supports multi-subject compatibility. by @huiyadanli in https://github.com/emako/wpfui.violeta/pull/36
* fix: Handle TaskbarCreated message to restore tray icon after Explorer restart by @Copilot in https://github.com/emako/wpfui.violeta/pull/35

## [4.2.0] - 2026-01-16

- No release notes.

## [4.1.0] - 2026-01-16

- No release notes.

## [4.0.3.7] - 2026-01-16

* Add configurable toast stacking feature with position control and static configuration properties by @Copilot in https://github.com/emako/wpfui.violeta/pull/30
* Add support for nested submenus in TrayIconHost by @Copilot in https://github.com/emako/wpfui.violeta/pull/33

### Contributors

* @Copilot made their first contribution in https://github.com/emako/wpfui.violeta/pull/30

## [4.0.3.6] - 2025-09-06

* fix: track system theme changes in demo project [#28](https://github.com/emako/wpfui.violeta/issues/28)

https://www.nuget.org/packages/WPF-UI.Violeta/4.0.3.6

## [4.0.3.5] - 2025-08-22

* fix: safety check Drawer width and height

## [4.0.3.4] - 2025-08-22

* refactor: Drawer control and add XAML template
* feat: support debug symbol in .snupkg

## [4.0.3.3] - 2025-08-21

* feat: Drawer with animated sliding panel
* feat: splash hint [#27](https://github.com/emako/wpfui.violeta/issues/27)
* feat: Add bold style support for tray menu items
* feat: add Tag for TrayIconHost LINQ

## [4.0.3.2] - 2025-07-20

* feat: ShowNotification (aka ShowBalloonTip)

## [4.0.3.1] - 2025-07-08

* feat: add I18N helper for resource localization
* feat: fallback `zh` to `zh-Hans`

## [4.0.3] - 2025-06-30

* Use WPF-UI 4.0.3

## [4.0.2.4] - 2025-06-21

* Fixed style for SmoothScrollViewer by @BlackBooth in https://github.com/emako/wpfui.violeta/pull/24
* Added german translation by @BlackBooth in https://github.com/emako/wpfui.violeta/pull/22
* Fixed loading control for non-english systems by @BlackBooth in https://github.com/emako/wpfui.violeta/pull/23

### Contributors

* @BlackBooth made their first contribution in https://github.com/emako/wpfui.violeta/pull/24

## [4.0.2.3] - 2025-04-20

* feat: TrayIconHost

## [4.0.2.2] - 2025-04-01

fix: add the lost font symbols

## [4.0.2.1] - 2025-03-24

feat: LegacyDataGrid from wpfui3.0.5

> The new DataGrid in version >=4.0.0 is too many bug to use.
> If you want to use the legacy one, you should manually include the `LegacyDataGrid.xaml` using `<ResourceDictionary Source="pack://application:,,,/Wpf.Ui.Violeta;component/Controls/DataGrid/LegacyDataGrid.xaml" />`.

## [4.0.2] - 2025-03-24

Use WPF-UI.4.0.2

## [4.0.0] - 2025-03-13

* WPF-UI 4.0.0 by @huiyadanli in https://github.com/emako/wpfui.violeta/pull/19
* Bump all actions to latest version by @qhy040404 in https://github.com/emako/wpfui.violeta/pull/20

### Contributors

* @qhy040404 made their first contribution in https://github.com/emako/wpfui.violeta/pull/20

## [3.0.5.28] - 2024-12-31

feat: AutoGrid

## [3.0.5.27] - 2024-12-08

* Improve MessageBox design by @KamilDev in https://github.com/emako/wpfui.violeta/pull/18
* feat: SetProcessDpiAwareness param
* feat: SystemMenuThemeManager

### Contributors

* @KamilDev made their first contribution in https://github.com/emako/wpfui.violeta/pull/18

## [3.0.5.26] - 2024-11-20

- feat: IPendingHandler add start time
 - fix: PendingBoxDialog style in win10

## [3.0.5.25] - 2024-11-19

- feat: PendingBox [#11](https://github.com/emako/wpfui.violeta/issues/11)
 - fix: MessageBox not inherit Topmost

## [3.0.5.24] - 2024-11-19

- feat: SmoothScrollViewer
 - feat: ApplicationDispatcher/STAThread
 - feat: FileSizeStringConverter [#3](https://github.com/emako/wpfui.violeta/issues/3)
 - feat: Violeta Fluent Icons
 - fix: ImageViewer support light mode [#12](https://github.com/emako/wpfui.violeta/issues/12)
 - fix: FlyoutService theme not synced [#10](https://github.com/emako/wpfui.violeta/issues/10)

## [3.0.5.23] - 2024-10-27

* feat: handle TreeListView mouse right button

## [3.0.5.22] - 2024-10-27

* feat: DataGrid centered layout style

## [3.0.5.21] - 2024-10-27

* feat: add cache exclude by @huiyadanli in https://github.com/emako/wpfui.violeta/pull/9
* feat: ported CachedImage (BETA)

## [3.0.5.20] - 2024-10-16

feat: BitmapIcon

## [3.0.5.19] - 2024-10-13

feat: new ContentDialog
feat: bindable TreeListView::SelectedItem

## [3.0.5.18] - 2024-10-09

feat: IconManager [#3](https://github.com/emako/wpfui.violeta/issues/3)
fix: TreeListView scroll incompletely [#5](https://github.com/emako/wpfui.violeta/issues/5)
fix: TreeListView FocusVisualStyle error

## [3.0.5.17] - 2024-10-07

fix: ResourcesProvider.GetString

## [3.0.5.16] - 2024-09-30

fix: Height of TreeListViewItem

## [3.0.5.15] - 2024-09-27

fix: duplicated Grid in ContentDialogHostService
feat: ManifestResourceProvider / ResourcesProvider
feat: MenuItemGroup
feat: TreeListView CornerRadius
feat: DpiAware

## [3.0.5.14] - 2024-09-23

breaking change: TreeListView was renamed to TreeModelListView
feat: Neo-TreeListView

## [3.0.5.13] - 2024-09-19

* fix: Splash CornerRadius not effective
* fix: use default ViewportSize for TreeListView
* chore: example of adding a folder for TreeListView by @huiyadanli in https://github.com/emako/wpfui.violeta/pull/2

### Contributors

* @huiyadanli made their first contribution in https://github.com/emako/wpfui.violeta/pull/2

## [3.0.5.12] - 2024-09-10

fix: add try get methods for ExceptionWindow
feat: support ToggleButtonGroup option `IsCanCancel`

## [3.0.5.11] - 2024-09-10

unimportant

## [3.0.5.10] - 2024-09-10

feat: ThemeManager
feat: RadioButtonGroup
feat: ExceptionReport
change: support scrollable for message box

## [3.0.5.9] - 2024-09-08

- No release notes.

## [3.0.5.8] - 2024-09-08

feat: ImageView
feat: TreeListView
feat: ported layout controls from [wpfsuite](https://github.com/OrgEleCho/EleCho.WpfSuite)

## [3.0.5.7] - 2024-09-03

- No release notes.

## [3.0.5.6] - 2024-08-26

feat: Ctrl+C copy content of MessageBox

## [3.0.5.5] - 2024-08-23

feat: PersonPicture

## [3.0.5.4] - 2024-08-22

fix: specify toast foreground

## [3.0.5.3] - 2024-08-20

update: MessageBox use wrapping

## [3.0.5.2] - 2024-08-15

fix: some issues about FlyoutService
feature: ContentDialogHostService
feature: MessageBox

## [3.0.5.1] - 2024-08-15

feature: FlyoutService

## [3.0.5] - 2024-08-14

feature: Toast
