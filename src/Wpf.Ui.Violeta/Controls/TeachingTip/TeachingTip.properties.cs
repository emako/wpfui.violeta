using System.Windows;
using System.Windows.Input;

using Wpf.Ui.Violeta.Controls.Compat;

namespace Wpf.Ui.Violeta.Controls;

public partial class TeachingTip
{
    #region ActionButtonCommand

    public static readonly DependencyProperty ActionButtonCommandProperty =
        DependencyProperty.Register(
            nameof(ActionButtonCommand),
            typeof(ICommand),
            typeof(TeachingTip),
            new PropertyMetadata(OnActionButtonCommandPropertyChanged));

    public ICommand ActionButtonCommand
    {
        get => (ICommand)GetValue(ActionButtonCommandProperty);
        set => SetValue(ActionButtonCommandProperty, value);
    }

    private static void OnActionButtonCommandPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion ActionButtonCommand

    #region ActionButtonCommandParameter

    public static readonly DependencyProperty ActionButtonCommandParameterProperty =
        DependencyProperty.Register(
            nameof(ActionButtonCommandParameter),
            typeof(object),
            typeof(TeachingTip),
            new PropertyMetadata(OnActionButtonCommandParameterPropertyChanged));

    public object ActionButtonCommandParameter
    {
        get => GetValue(ActionButtonCommandParameterProperty);
        set => SetValue(ActionButtonCommandParameterProperty, value);
    }

    private static void OnActionButtonCommandParameterPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion ActionButtonCommandParameter

    #region ActionButtonContent

    public static readonly DependencyProperty ActionButtonContentProperty =
        DependencyProperty.Register(
            nameof(ActionButtonContent),
            typeof(object),
            typeof(TeachingTip),
            new PropertyMetadata(OnActionButtonContentPropertyChanged));

    public object ActionButtonContent
    {
        get => (object)GetValue(ActionButtonContentProperty);
        set => SetValue(ActionButtonContentProperty, value);
    }

    private static void OnActionButtonContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion ActionButtonContent

    #region ActionButtonStyle

    public static readonly DependencyProperty ActionButtonStyleProperty =
        DependencyProperty.Register(
            nameof(ActionButtonStyle),
            typeof(Style),
            typeof(TeachingTip),
            new PropertyMetadata(OnActionButtonStylePropertyChanged));

    public Style ActionButtonStyle
    {
        get => (Style)GetValue(ActionButtonStyleProperty);
        set => SetValue(ActionButtonStyleProperty, value);
    }

    private static void OnActionButtonStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion ActionButtonStyle

    #region CloseButtonCommand

    public static readonly DependencyProperty CloseButtonCommandProperty =
        DependencyProperty.Register(
            nameof(CloseButtonCommand),
            typeof(ICommand),
            typeof(TeachingTip),
            new PropertyMetadata(OnCloseButtonCommandPropertyChanged));

    public ICommand CloseButtonCommand
    {
        get => (ICommand)GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    private static void OnCloseButtonCommandPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion CloseButtonCommand

    #region CloseButtonCommandParameter

    public static readonly DependencyProperty CloseButtonCommandParameterProperty =
        DependencyProperty.Register(
            nameof(CloseButtonCommandParameter),
            typeof(object),
            typeof(TeachingTip),
            new PropertyMetadata(OnCloseButtonCommandParameterPropertyChanged));

    public object CloseButtonCommandParameter
    {
        get => (object)GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }

    private static void OnCloseButtonCommandParameterPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion CloseButtonCommandParameter

    #region CloseButtonContent

    public static readonly DependencyProperty CloseButtonContentProperty =
        DependencyProperty.Register(
            nameof(CloseButtonContent),
            typeof(object),
            typeof(TeachingTip),
            new PropertyMetadata(OnCloseButtonContentPropertyChanged));

    public object CloseButtonContent
    {
        get => (object)GetValue(CloseButtonContentProperty);
        set => SetValue(CloseButtonContentProperty, value);
    }

    private static void OnCloseButtonContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion CloseButtonContent

    #region CloseButtonStyle

    public static readonly DependencyProperty CloseButtonStyleProperty =
        DependencyProperty.Register(
            nameof(CloseButtonStyle),
            typeof(Style),
            typeof(TeachingTip),
            new PropertyMetadata(OnCloseButtonStylePropertyChanged));

    public Style CloseButtonStyle
    {
        get => (Style)GetValue(CloseButtonStyleProperty);
        set => SetValue(CloseButtonStyleProperty, value);
    }

    private static void OnCloseButtonStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion CloseButtonStyle

    #region HeroContent

    public static readonly DependencyProperty HeroContentProperty =
        DependencyProperty.Register(
            nameof(HeroContent),
            typeof(UIElement),
            typeof(TeachingTip),
            new PropertyMetadata(OnHeroContentPropertyChanged));

    public UIElement HeroContent
    {
        get => (UIElement)GetValue(HeroContentProperty);
        set => SetValue(HeroContentProperty, value);
    }

    private static void OnHeroContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion HeroContent

    #region HeroContentPlacement

    public static readonly DependencyProperty HeroContentPlacementProperty =
        DependencyProperty.Register(
            nameof(HeroContentPlacement),
            typeof(TeachingTipHeroContentPlacementMode),
            typeof(TeachingTip),
            new PropertyMetadata(TeachingTipHeroContentPlacementMode.Auto, OnHeroContentPlacementPropertyChanged));

    public TeachingTipHeroContentPlacementMode HeroContentPlacement
    {
        get => (TeachingTipHeroContentPlacementMode)GetValue(HeroContentPlacementProperty);
        set => SetValue(HeroContentPlacementProperty, value);
    }

    private static void OnHeroContentPlacementPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion HeroContentPlacement

    #region IconSource

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(
            nameof(IconSource),
            typeof(IconSource),
            typeof(TeachingTip),
            new PropertyMetadata(OnIconSourcePropertyChanged));

    public IconSource IconSource
    {
        get => (IconSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    private static void OnIconSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion IconSource

    #region IsLightDismissEnabled

    public static readonly DependencyProperty IsLightDismissEnabledProperty =
        DependencyProperty.Register(
            nameof(IsLightDismissEnabled),
            typeof(bool),
            typeof(TeachingTip),
            new PropertyMetadata(false, OnIsLightDismissEnabledPropertyChanged));

    public bool IsLightDismissEnabled
    {
        get => (bool)GetValue(IsLightDismissEnabledProperty);
        set => SetValue(IsLightDismissEnabledProperty, value);
    }

    private static void OnIsLightDismissEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion IsLightDismissEnabled

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(TeachingTip),
            new PropertyMetadata(false, OnIsOpenPropertyChanged));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion IsOpen

    #region PlacementMargin

    public static readonly DependencyProperty PlacementMarginProperty =
        DependencyProperty.Register(
            nameof(PlacementMargin),
            typeof(Thickness),
            typeof(TeachingTip),
            new PropertyMetadata(default(Thickness), OnPlacementMarginPropertyChanged));

    public Thickness PlacementMargin
    {
        get => (Thickness)GetValue(PlacementMarginProperty);
        set => SetValue(PlacementMarginProperty, value);
    }

    private static void OnPlacementMarginPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion PlacementMargin

    #region PreferredPlacement

    public static readonly DependencyProperty PreferredPlacementProperty =
        DependencyProperty.Register(
            nameof(PreferredPlacement),
            typeof(TeachingTipPlacementMode),
            typeof(TeachingTip),
            new PropertyMetadata(TeachingTipPlacementMode.Auto, OnPreferredPlacementPropertyChanged));

    public TeachingTipPlacementMode PreferredPlacement
    {
        get => (TeachingTipPlacementMode)GetValue(PreferredPlacementProperty);
        set => SetValue(PreferredPlacementProperty, value);
    }

    private static void OnPreferredPlacementPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion PreferredPlacement

    #region ShouldConstrainToRootBounds

    public static readonly DependencyProperty ShouldConstrainToRootBoundsProperty =
        DependencyProperty.Register(
            nameof(ShouldConstrainToRootBounds),
            typeof(bool),
            typeof(TeachingTip),
            new PropertyMetadata(true, OnShouldConstrainToRootBoundsPropertyChanged));

    public bool ShouldConstrainToRootBounds
    {
        get => (bool)GetValue(ShouldConstrainToRootBoundsProperty);
        set => SetValue(ShouldConstrainToRootBoundsProperty, value);
    }

    private static void OnShouldConstrainToRootBoundsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion ShouldConstrainToRootBounds

    #region Subtitle

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(TeachingTip),
            new PropertyMetadata(OnSubtitlePropertyChanged));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    private static void OnSubtitlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion Subtitle

    #region TailVisibility

    public static readonly DependencyProperty TailVisibilityProperty =
        DependencyProperty.Register(
            nameof(TailVisibility),
            typeof(TeachingTipTailVisibility),
            typeof(TeachingTip),
            new PropertyMetadata(TeachingTipTailVisibility.Auto, OnTailVisibilityPropertyChanged));

    public TeachingTipTailVisibility TailVisibility
    {
        get => (TeachingTipTailVisibility)GetValue(TailVisibilityProperty);
        set => SetValue(TailVisibilityProperty, value);
    }

    private static void OnTailVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion TailVisibility

    #region Target

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(
            nameof(Target),
            typeof(FrameworkElement),
            typeof(TeachingTip),
            new PropertyMetadata(OnTargetPropertyChanged));

    public FrameworkElement Target
    {
        get => (FrameworkElement)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    private static void OnTargetPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion Target

    #region TemplateSettings

    public static readonly DependencyProperty TemplateSettingsProperty =
        DependencyProperty.Register(
            nameof(TemplateSettings),
            typeof(TeachingTipTemplateSettings),
            typeof(TeachingTip),
            new PropertyMetadata(OnTemplateSettingsPropertyChanged));

    public TeachingTipTemplateSettings TemplateSettings
    {
        get => (TeachingTipTemplateSettings)GetValue(TemplateSettingsProperty);
        set => SetValue(TemplateSettingsProperty, value);
    }

    private static void OnTemplateSettingsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion TemplateSettings

    #region Title

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(TeachingTip),
            new PropertyMetadata(OnTitlePropertyChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnTitlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (TeachingTip)sender;
        owner.OnPropertyChanged(args);
    }

    #endregion Title

    #region CornerRadius

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TeachingTip), new PropertyMetadata(default(CornerRadius)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion CornerRadius

    public event TypedEventHandler<TeachingTip, object>? ActionButtonClick;

    public event TypedEventHandler<TeachingTip, object>? CloseButtonClick;

    public event TypedEventHandler<TeachingTip, TeachingTipClosingEventArgs>? Closing;

    public event TypedEventHandler<TeachingTip, TeachingTipClosedEventArgs>? Closed;
}
