using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

public partial class AnimatedSymbolButton
{
    /// <summary>
    /// <c>AnimatedSettingsVisualSource</c> — 1:1 port of
    /// <c>NavigationView</c> settings gear rotation
    /// (press → -22.5°, release → 360° then snap to 0).
    /// </summary>
    internal sealed class SettingsBehavior : AnimatedSymbolBehavior
    {
        private Storyboard? _storyboard;
        private FrameworkElement? _iconElement;

        public override string DefaultGlyph => "\uE713";

        public override Point RenderTransformOrigin => new(0.5, 0.5);

        protected override void OnAttached()
        {
            if (Owner is null)
            {
                return;
            }

            // Prefer the glyph TextBlock (same role as NavigationViewItem.Icon).
            _iconElement =
                Owner.GetPart(DefaultGlyphPart) as FrameworkElement
                ?? AnimatedVisual;

            EnsureIconRotateTransform();

            Owner.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            Owner.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            if (Owner is null)
            {
                return;
            }

            Owner.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            Owner.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;

            _storyboard?.Stop(Owner);
            _storyboard?.Remove(Owner);
            _storyboard = null;

            if (_iconElement?.RenderTransform is RotateTransform rotate)
            {
                rotate.BeginAnimation(RotateTransform.AngleProperty, null);
                rotate.Angle = 0;
            }

            _iconElement = null;
        }

        // NavigationView.OnSettingsItemPreviewMouseLeftButtonDown
        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Match settings gear: wind back to -22.5° in 0.1s (CircleEase EaseIn)
            BeginSettingsIconRotation(toAngle: -22.5, durationSeconds: 0.1, easingMode: EasingMode.EaseIn, resetAfter: false);
        }

        // NavigationView.OnSettingsItemPreviewMouseLeftButtonUp
        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Match settings gear: spin to 360° in 0.5s (CircleEase EaseOut), then snap angle back to 0
            BeginSettingsIconRotation(toAngle: 360, durationSeconds: 0.5, easingMode: EasingMode.EaseOut, resetAfter: true);
        }

        // NavigationView.TryGetSettingsIconElement
        private FrameworkElement? TryGetSettingsIconElement()
        {
            if (_iconElement is null)
            {
                _iconElement =
                    Owner?.GetPart(DefaultGlyphPart) as FrameworkElement
                    ?? AnimatedVisual;
            }

            return EnsureIconRotateTransform();
        }

        private FrameworkElement? EnsureIconRotateTransform()
        {
            if (_iconElement is null)
            {
                return null;
            }

            if (!(_iconElement.RenderTransform is RotateTransform))
            {
                _iconElement.RenderTransformOrigin = new Point(0.5, 0.5);
                _iconElement.RenderTransform = new RotateTransform { Angle = 0 };
            }

            return _iconElement;
        }

        // NavigationView.BeginSettingsIconRotation — line-for-line
        private void BeginSettingsIconRotation(
            double toAngle,
            double durationSeconds,
            EasingMode easingMode,
            bool resetAfter)
        {
            var iconElement = TryGetSettingsIconElement();
            if (iconElement is null || Owner is null)
            {
                return;
            }

            _storyboard?.Stop(Owner);
            _storyboard?.Remove(Owner);

            // Animate via the icon element path (same idea as settings gear).
            var anglePath = new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)");

            var animation = new DoubleAnimation
            {
                To = toAngle,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = new CircleEase { EasingMode = easingMode },
                FillBehavior = FillBehavior.HoldEnd,
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, iconElement);
            Storyboard.SetTargetProperty(animation, anglePath);
            storyboard.Children.Add(animation);

            if (resetAfter)
            {
                var reset = new DoubleAnimation
                {
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(durationSeconds),
                    Duration = TimeSpan.Zero,
                    FillBehavior = FillBehavior.HoldEnd,
                };
                Storyboard.SetTarget(reset, iconElement);
                Storyboard.SetTargetProperty(reset, anglePath);
                storyboard.Children.Add(reset);
            }

            _storyboard = storyboard;
            // Controllable clock so Stop/Remove on the next press/release works (Button captures mouse).
            storyboard.Begin(Owner, isControllable: true);
        }
    }
}
