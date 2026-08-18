using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Attached WinUI ProgressBar behavior used by <c>Hotfix/ProgressBar.xaml</c>
/// to drive indicator width and visual states on <see cref="ProgressBar"/>.
/// </summary>
public static class ProgressBarHelper
{
    #region IsEnabled

    public static bool GetIsEnabled(ProgressBar element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(ProgressBar element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ProgressBarHelper),
            new PropertyMetadata(false, OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ProgressBar progressBar)
        {
            return;
        }

        if (Equals(e.OldValue, e.NewValue))
        {
            return;
        }

        if (e.NewValue is true)
        {
            Attach(progressBar);
        }
        else
        {
            Detach(progressBar);
        }
    }

    #endregion

    #region ShowError

    public static bool GetShowError(ProgressBar element) =>
        (bool)element.GetValue(ShowErrorProperty);

    public static void SetShowError(ProgressBar element, bool value) =>
        element.SetValue(ShowErrorProperty, value);

    public static readonly DependencyProperty ShowErrorProperty =
        DependencyProperty.RegisterAttached(
            "ShowError",
            typeof(bool),
            typeof(ProgressBarHelper),
            new PropertyMetadata(false, OnShowErrorChanged));

    private static void OnShowErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        GetController(d as ProgressBar)?.OnShowErrorChanged();
    }

    #endregion

    #region ShowPaused

    public static bool GetShowPaused(ProgressBar element) =>
        (bool)element.GetValue(ShowPausedProperty);

    public static void SetShowPaused(ProgressBar element, bool value) =>
        element.SetValue(ShowPausedProperty, value);

    public static readonly DependencyProperty ShowPausedProperty =
        DependencyProperty.RegisterAttached(
            "ShowPaused",
            typeof(bool),
            typeof(ProgressBarHelper),
            new PropertyMetadata(false, OnShowPausedChanged));

    private static void OnShowPausedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        GetController(d as ProgressBar)?.OnShowPausedChanged();
    }

    #endregion

    #region TemplateSettings

    private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "TemplateSettings",
            typeof(ProgressBarTemplateSettings),
            typeof(ProgressBarHelper),
            null);

    public static readonly DependencyProperty TemplateSettingsProperty =
        TemplateSettingsPropertyKey.DependencyProperty;

    public static ProgressBarTemplateSettings GetTemplateSettings(ProgressBar element) =>
        (ProgressBarTemplateSettings)element.GetValue(TemplateSettingsProperty);

    private static void SetTemplateSettings(ProgressBar element, ProgressBarTemplateSettings? value) =>
        element.SetValue(TemplateSettingsPropertyKey, value);

    #endregion

    #region Controller

    private static readonly DependencyProperty ControllerProperty =
        DependencyProperty.RegisterAttached(
            "Controller",
            typeof(ProgressBarController),
            typeof(ProgressBarHelper));

    private static ProgressBarController? GetController(ProgressBar? progressBar) =>
        progressBar?.GetValue(ControllerProperty) as ProgressBarController;

    private static void Attach(ProgressBar progressBar)
    {
        if (GetController(progressBar) != null)
        {
            return;
        }

        SetTemplateSettings(progressBar, new ProgressBarTemplateSettings());
        var controller = new ProgressBarController(progressBar);
        progressBar.SetValue(ControllerProperty, controller);
        controller.Attach();
    }

    private static void Detach(ProgressBar progressBar)
    {
        var controller = GetController(progressBar);
        if (controller != null)
        {
            controller.Detach();
            progressBar.ClearValue(ControllerProperty);
        }

        progressBar.ClearValue(TemplateSettingsPropertyKey);
    }

    #endregion

    private sealed class ProgressBarController
    {
        private const string LayoutRootName = "LayoutRoot";
        private const string DeterminateProgressBarIndicatorName = "DeterminateProgressBarIndicator";
        private const string IndeterminateProgressBarIndicatorName = "IndeterminateProgressBarIndicator";
        private const string IndeterminateProgressBarIndicator2Name = "IndeterminateProgressBarIndicator2";
        private const string ErrorStateName = "Error";
        private const string PausedStateName = "Paused";
        private const string IndeterminateStateName = "Indeterminate";
        private const string IndeterminateErrorStateName = "IndeterminateError";
        private const string IndeterminatePausedStateName = "IndeterminatePaused";
        private const string DeterminateStateName = "Determinate";
        private const string UpdatingStateName = "Updating";
        private const string UpdatingWithErrorStateName = "UpdatingError";

        private readonly ProgressBar _owner;
        private readonly EventHandler _onIsIndeterminateChanged;
        private readonly EventHandler _onMinimumChanged;
        private readonly EventHandler _onMaximumChanged;
        private readonly EventHandler _onPaddingChanged;
        private readonly EventHandler _onForegroundChanged;
        private readonly EventHandler _onTemplateChanged;
        private readonly DependencyPropertyDescriptor _isIndeterminateDescriptor;
        private readonly DependencyPropertyDescriptor _minimumDescriptor;
        private readonly DependencyPropertyDescriptor _maximumDescriptor;
        private readonly DependencyPropertyDescriptor _paddingDescriptor;
        private readonly DependencyPropertyDescriptor _foregroundDescriptor;
        private readonly DependencyPropertyDescriptor _templateDescriptor;

        private Grid? _layoutRoot;
        private Rectangle? _determinateProgressBarIndicator;
        private Rectangle? _indeterminateProgressBarIndicator;
        private Rectangle? _indeterminateProgressBarIndicator2;
        private bool _attached;

        public ProgressBarController(ProgressBar owner)
        {
            _owner = owner;
            _onIsIndeterminateChanged = (_, _) => OnIsIndeterminatePropertyChanged();
            _onMinimumChanged = (_, _) => OnIndicatorWidthComponentChanged();
            _onMaximumChanged = (_, _) => OnIndicatorWidthComponentChanged();
            _onPaddingChanged = (_, _) => OnIndicatorWidthComponentChanged();
            _onForegroundChanged = (_, _) => ApplyIndicatorBrushes();
            _onTemplateChanged = (_, _) => ApplyTemplateParts();

            _isIndeterminateDescriptor = DependencyPropertyDescriptor.FromProperty(ProgressBar.IsIndeterminateProperty, typeof(ProgressBar));
            _minimumDescriptor = DependencyPropertyDescriptor.FromProperty(RangeBase.MinimumProperty, typeof(ProgressBar));
            _maximumDescriptor = DependencyPropertyDescriptor.FromProperty(RangeBase.MaximumProperty, typeof(ProgressBar));
            _paddingDescriptor = DependencyPropertyDescriptor.FromProperty(Control.PaddingProperty, typeof(ProgressBar));
            _foregroundDescriptor = DependencyPropertyDescriptor.FromProperty(Control.ForegroundProperty, typeof(ProgressBar));
            _templateDescriptor = DependencyPropertyDescriptor.FromProperty(Control.TemplateProperty, typeof(ProgressBar));
        }

        public void Attach()
        {
            if (_attached)
            {
                return;
            }

            _attached = true;
            _owner.Loaded += OnLoaded;
            _owner.SizeChanged += OnSizeChanged;
            _owner.ValueChanged += OnValueChanged;
            _isIndeterminateDescriptor.AddValueChanged(_owner, _onIsIndeterminateChanged);
            _minimumDescriptor.AddValueChanged(_owner, _onMinimumChanged);
            _maximumDescriptor.AddValueChanged(_owner, _onMaximumChanged);
            _paddingDescriptor.AddValueChanged(_owner, _onPaddingChanged);
            _foregroundDescriptor.AddValueChanged(_owner, _onForegroundChanged);
            _templateDescriptor.AddValueChanged(_owner, _onTemplateChanged);
            ApplicationThemeManager.Changed += OnApplicationThemeChanged;

            if (_owner.IsLoaded)
            {
                ApplyTemplateParts();
            }
        }

        public void Detach()
        {
            if (!_attached)
            {
                return;
            }

            _attached = false;
            _owner.Loaded -= OnLoaded;
            _owner.SizeChanged -= OnSizeChanged;
            _owner.ValueChanged -= OnValueChanged;
            _isIndeterminateDescriptor.RemoveValueChanged(_owner, _onIsIndeterminateChanged);
            _minimumDescriptor.RemoveValueChanged(_owner, _onMinimumChanged);
            _maximumDescriptor.RemoveValueChanged(_owner, _onMaximumChanged);
            _paddingDescriptor.RemoveValueChanged(_owner, _onPaddingChanged);
            _foregroundDescriptor.RemoveValueChanged(_owner, _onForegroundChanged);
            _templateDescriptor.RemoveValueChanged(_owner, _onTemplateChanged);
            ApplicationThemeManager.Changed -= OnApplicationThemeChanged;
            _layoutRoot = null;
            _determinateProgressBarIndicator = null;
            _indeterminateProgressBarIndicator = null;
            _indeterminateProgressBarIndicator2 = null;
        }

        public void OnShowErrorChanged() => UpdateStates();

        public void OnShowPausedChanged() => UpdateStates();

        private void OnLoaded(object sender, RoutedEventArgs e) => ApplyTemplateParts();

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetProgressBarIndicatorWidth();
            UpdateWidthBasedTemplateSettings();
            ReapplyIndeterminateStoryboard();
        }

        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            OnIndicatorWidthComponentChanged();

        private void OnIsIndeterminatePropertyChanged()
        {
            SetProgressBarIndicatorWidth();
            UpdateStates();
            ReapplyIndeterminateStoryboard();
        }

        private void OnIndicatorWidthComponentChanged() => SetProgressBarIndicatorWidth();

        private void OnApplicationThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
        {
            ApplyIndicatorBrushes();
            _owner.Dispatcher.BeginInvoke(RefreshStates, DispatcherPriority.Render);
        }

        private void ApplyTemplateParts()
        {
            _owner.ApplyTemplate();
            _layoutRoot = FindName<Grid>(LayoutRootName);
            _determinateProgressBarIndicator = FindName<Rectangle>(DeterminateProgressBarIndicatorName);
            _indeterminateProgressBarIndicator = FindName<Rectangle>(IndeterminateProgressBarIndicatorName);
            _indeterminateProgressBarIndicator2 = FindName<Rectangle>(IndeterminateProgressBarIndicator2Name);
            ApplyIndicatorBrushes();
            SetProgressBarIndicatorWidth();
            UpdateWidthBasedTemplateSettings();
            UpdateStates();
            ReapplyIndeterminateStoryboard();
        }

        private T? FindName<T>(string name) where T : FrameworkElement
        {
            if (_owner.Template?.FindName(name, _owner) is T child)
            {
                return child;
            }

            return null;
        }

        private void ApplyIndicatorBrushes()
        {
            if (_owner.Foreground is not SolidColorBrush source)
            {
                return;
            }

            ApplyBrush(_determinateProgressBarIndicator, source);
            ApplyBrush(_indeterminateProgressBarIndicator, source);
            ApplyBrush(_indeterminateProgressBarIndicator2, source);
        }

        private static void ApplyBrush(Rectangle? rectangle, SolidColorBrush source)
        {
            if (rectangle == null)
            {
                return;
            }

            rectangle.Fill = source.CloneCurrentValue();
        }

        private void UpdateStates(bool useTransitions = true)
        {
            if (_owner.IsIndeterminate)
            {
                if (GetShowError(_owner))
                {
                    VisualStateManager.GoToState(_owner, IndeterminateErrorStateName, useTransitions);
                }
                else if (GetShowPaused(_owner))
                {
                    VisualStateManager.GoToState(_owner, IndeterminatePausedStateName, useTransitions);
                }
                else
                {
                    VisualStateManager.GoToState(_owner, IndeterminateStateName, useTransitions);
                }

                UpdateWidthBasedTemplateSettings();
            }
            else if (GetShowError(_owner))
            {
                VisualStateManager.GoToState(_owner, ErrorStateName, useTransitions);
            }
            else if (GetShowPaused(_owner))
            {
                VisualStateManager.GoToState(_owner, PausedStateName, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(_owner, DeterminateStateName, useTransitions);
            }
        }

        private void SetProgressBarIndicatorWidth()
        {
            var templateSettings = GetTemplateSettings(_owner);
            if (templateSettings == null || _layoutRoot == null || _determinateProgressBarIndicator == null)
            {
                return;
            }

            double progressBarWidth = _layoutRoot.ActualWidth;
            double prevIndicatorWidth = _determinateProgressBarIndicator.ActualWidth;
            double maximum = _owner.Maximum;
            double minimum = _owner.Minimum;
            var padding = _owner.Padding;

            if (GetShowError(_owner))
            {
                VisualStateManager.GoToState(_owner, UpdatingWithErrorStateName, true);
            }
            else
            {
                VisualStateManager.GoToState(_owner, UpdatingStateName, true);
            }

            if (_owner.IsIndeterminate)
            {
                _determinateProgressBarIndicator.Width = 0;

                if (_indeterminateProgressBarIndicator != null)
                {
                    _indeterminateProgressBarIndicator.Width = progressBarWidth * 0.4;
                }

                if (_indeterminateProgressBarIndicator2 != null)
                {
                    _indeterminateProgressBarIndicator2.Width = progressBarWidth * 0.6;
                }
            }
            else if (Math.Abs(maximum - minimum) > double.Epsilon)
            {
                double maxIndicatorWidth = progressBarWidth - (padding.Left + padding.Right);
                double increment = maxIndicatorWidth / (maximum - minimum);
                double indicatorWidth = increment * (_owner.Value - minimum);
                double widthDelta = indicatorWidth - prevIndicatorWidth;
                templateSettings.IndicatorLengthDelta = -widthDelta;
                _determinateProgressBarIndicator.Width = indicatorWidth;
            }
            else
            {
                _determinateProgressBarIndicator.Width = 0;
            }

            UpdateStates();
        }

        private void UpdateWidthBasedTemplateSettings()
        {
            var templateSettings = GetTemplateSettings(_owner);
            if (templateSettings == null)
            {
                return;
            }

            double width;
            double height;
            if (_layoutRoot != null)
            {
                width = _layoutRoot.ActualWidth;
                height = _layoutRoot.ActualHeight;
            }
            else
            {
                width = 0;
                height = 0;
            }

            double indeterminateProgressBarIndicatorWidth = width * 0.4;
            double indeterminateProgressBarIndicatorWidth2 = width * 0.6;

            templateSettings.ContainerAnimationStartPosition = indeterminateProgressBarIndicatorWidth * -1.0;
            templateSettings.ContainerAnimationEndPosition = indeterminateProgressBarIndicatorWidth * 3.0;
            templateSettings.Container2AnimationStartPosition = indeterminateProgressBarIndicatorWidth2 * -1.5;
            templateSettings.Container2AnimationEndPosition = indeterminateProgressBarIndicatorWidth2 * 1.66;
            templateSettings.ContainerAnimationMidPosition = width * 0.2;

            var padding = _owner.Padding;
            var rectangle = new RectangleGeometry(
                new Rect(
                    padding.Left,
                    padding.Top,
                    Math.Max(0, width - (padding.Right + padding.Left)),
                    Math.Max(0, height - (padding.Bottom + padding.Top))));

            if (_indeterminateProgressBarIndicator != null)
            {
                rectangle.RadiusX = _indeterminateProgressBarIndicator.RadiusX;
                rectangle.RadiusY = _indeterminateProgressBarIndicator.RadiusY;
            }

            templateSettings.ClipRect = rectangle;
            templateSettings.EllipseAnimationEndPosition = (1.0 / 3.0) * width;
            templateSettings.EllipseAnimationWellPosition = (2.0 / 3.0) * width;

            if (width <= 180.0)
            {
                templateSettings.EllipseDiameter = 4.0;
                templateSettings.EllipseOffset = 4.0;
            }
            else if (width <= 280.0)
            {
                templateSettings.EllipseDiameter = 5.0;
                templateSettings.EllipseOffset = 7.0;
            }
            else
            {
                templateSettings.EllipseDiameter = 6.0;
                templateSettings.EllipseOffset = 9.0;
            }
        }

        private void RefreshStates()
        {
            VisualStateManager.GoToState(_owner, UpdatingStateName, false);
            UpdateStates(false);
        }

        private void ReapplyIndeterminateStoryboard()
        {
            _owner.Dispatcher.BeginInvoke(
                (Action)(() =>
                {
                    if (_owner.IsIndeterminate)
                    {
                        RefreshStates();
                    }
                }),
                DispatcherPriority.Render);
        }

    }
}
