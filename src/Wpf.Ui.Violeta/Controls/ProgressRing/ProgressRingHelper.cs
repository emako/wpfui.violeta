using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Controls.Primitives;

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Attached WinUI ProgressRing behavior used by <c>Hotfixes/ProgressRing.xaml</c>
/// to drive normalized range and visual states on <see cref="ProgressRing"/>.
/// </summary>
public static class ProgressRingHelper
{
    private const string ActiveStateName = "Active";
    private const string DeterminateActiveStateName = "DeterminateActive";
    private const string InactiveStateName = "Inactive";

    #region IsEnabled

    public static bool GetIsEnabled(ProgressRing element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(ProgressRing element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ProgressRingHelper),
            new PropertyMetadata(false, OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ProgressRing progressRing)
        {
            return;
        }

        if (Equals(e.OldValue, e.NewValue))
        {
            return;
        }

        if (e.NewValue is true)
        {
            Attach(progressRing);
        }
        else
        {
            Detach(progressRing);
        }
    }

    #endregion

    #region IsActive

    public static bool GetIsActive(ProgressRing element) =>
        (bool)element.GetValue(IsActiveProperty);

    public static void SetIsActive(ProgressRing element, bool value) =>
        element.SetValue(IsActiveProperty, value);

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.RegisterAttached(
            "IsActive",
            typeof(bool),
            typeof(ProgressRingHelper),
            new PropertyMetadata(true, OnIsActiveChanged));

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        GetController(d as ProgressRing)?.OnIsActiveChanged();
    }

    #endregion

    #region StrokeThickness

    public static double GetStrokeThickness(ProgressRing element) =>
        (double)element.GetValue(StrokeThicknessProperty);

    public static void SetStrokeThickness(ProgressRing element, double value) =>
        element.SetValue(StrokeThicknessProperty, value);

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.RegisterAttached(
            "StrokeThickness",
            typeof(double),
            typeof(ProgressRingHelper),
            new PropertyMetadata(3.0));

    #endregion

    #region TemplateSettings

    private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "TemplateSettings",
            typeof(ProgressRingTemplateSettings),
            typeof(ProgressRingHelper),
            null);

    public static readonly DependencyProperty TemplateSettingsProperty =
        TemplateSettingsPropertyKey.DependencyProperty;

    public static ProgressRingTemplateSettings GetTemplateSettings(ProgressRing element) =>
        (ProgressRingTemplateSettings)element.GetValue(TemplateSettingsProperty);

    private static void SetTemplateSettings(ProgressRing element, ProgressRingTemplateSettings? value) =>
        element.SetValue(TemplateSettingsPropertyKey, value);

    #endregion

    #region Controller

    private static readonly DependencyProperty ControllerProperty =
        DependencyProperty.RegisterAttached(
            "Controller",
            typeof(ProgressRingController),
            typeof(ProgressRingHelper));

    private static ProgressRingController? GetController(ProgressRing? progressRing) =>
        progressRing?.GetValue(ControllerProperty) as ProgressRingController;

    private static void Attach(ProgressRing progressRing)
    {
        if (GetController(progressRing) != null)
        {
            return;
        }

        SetTemplateSettings(progressRing, new ProgressRingTemplateSettings());
        var controller = new ProgressRingController(progressRing);
        progressRing.SetValue(ControllerProperty, controller);
        controller.Attach();
    }

    private static void Detach(ProgressRing progressRing)
    {
        var controller = GetController(progressRing);
        if (controller != null)
        {
            controller.Detach();
            progressRing.ClearValue(ControllerProperty);
        }

        progressRing.ClearValue(TemplateSettingsPropertyKey);
    }

    #endregion

    private sealed class ProgressRingController
    {
        private readonly ProgressRing _owner;
        private readonly EventHandler _onProgressChanged;
        private readonly EventHandler _onIsIndeterminateChanged;
        private readonly EventHandler _onTemplateChanged;
        private readonly DependencyPropertyDescriptor _progressDescriptor;
        private readonly DependencyPropertyDescriptor _isIndeterminateDescriptor;
        private readonly DependencyPropertyDescriptor _templateDescriptor;
        private bool _attached;

        public ProgressRingController(ProgressRing owner)
        {
            _owner = owner;
            _onProgressChanged = (_, _) => UpdateRange();
            _onIsIndeterminateChanged = (_, _) => ChangeVisualState();
            _onTemplateChanged = (_, _) => OnTemplateChanged();

            _progressDescriptor = DependencyPropertyDescriptor.FromProperty(
                ProgressRing.ProgressProperty, typeof(ProgressRing));
            _isIndeterminateDescriptor = DependencyPropertyDescriptor.FromProperty(
                ProgressRing.IsIndeterminateProperty, typeof(ProgressRing));
            _templateDescriptor = DependencyPropertyDescriptor.FromProperty(
                Control.TemplateProperty, typeof(ProgressRing));
        }

        public void Attach()
        {
            if (_attached)
            {
                return;
            }

            _attached = true;
            _owner.Loaded += OnLoaded;
            _progressDescriptor.AddValueChanged(_owner, _onProgressChanged);
            _isIndeterminateDescriptor.AddValueChanged(_owner, _onIsIndeterminateChanged);
            _templateDescriptor.AddValueChanged(_owner, _onTemplateChanged);

            UpdateRange();
            if (_owner.IsLoaded)
            {
                ChangeVisualState();
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
            _progressDescriptor.RemoveValueChanged(_owner, _onProgressChanged);
            _isIndeterminateDescriptor.RemoveValueChanged(_owner, _onIsIndeterminateChanged);
            _templateDescriptor.RemoveValueChanged(_owner, _onTemplateChanged);
        }

        public void OnIsActiveChanged() => ChangeVisualState();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateRange();
            ChangeVisualState();
        }

        private void OnTemplateChanged()
        {
            _owner.Dispatcher.BeginInvoke(
                (Action)(() =>
                {
                    UpdateRange();
                    ChangeVisualState();
                }),
                DispatcherPriority.Loaded);
        }

        private void UpdateRange()
        {
            var templateSettings = GetTemplateSettings(_owner);
            if (templateSettings == null)
            {
                return;
            }

            double progress = _owner.Progress;
            if (progress > 100)
            {
                progress = 100;
            }

            if (progress < 0)
            {
                progress = 0;
            }

            templateSettings.NormalizedRange = progress / 100.0;
        }

        private void ChangeVisualState()
        {
            bool isActive = GetIsActive(_owner);
            string state = isActive
                ? (_owner.IsIndeterminate ? ActiveStateName : DeterminateActiveStateName)
                : InactiveStateName;

            VisualStateManager.GoToState(_owner, state, true);
        }
    }
}
