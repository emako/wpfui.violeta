using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

public partial class AnimatedSymbolButton
{
    /// <summary>
    /// <c>AnimatedSettingsVisualSource</c> — wind back on press, full turn on release
    /// (NavigationView settings item).
    /// </summary>
    internal sealed class SettingsBehavior : AnimatedSymbolBehavior
    {
        private Storyboard? _storyboard;

        public override string DefaultGlyph => "\uE713";

        public override Point RenderTransformOrigin => new(0.5, 0.5);

        protected override void OnAttached()
        {
            if (Owner is null)
            {
                return;
            }

            EnsureFreshRotateTransform();
            Owner.AddHandler(PreviewMouseLeftButtonDownEvent, (MouseButtonEventHandler)OnPreviewMouseDown, true);
            Owner.AddHandler(PreviewMouseLeftButtonUpEvent, (MouseButtonEventHandler)OnPreviewMouseUp, true);
        }

        protected override void OnDetaching()
        {
            if (Owner is null)
            {
                return;
            }

            Owner.RemoveHandler(PreviewMouseLeftButtonDownEvent, (MouseButtonEventHandler)OnPreviewMouseDown);
            Owner.RemoveHandler(PreviewMouseLeftButtonUpEvent, (MouseButtonEventHandler)OnPreviewMouseUp);
            _storyboard?.Stop();
            _storyboard = null;
            if (TryGetRotate() is { } rotate)
            {
                rotate.BeginAnimation(RotateTransform.AngleProperty, null);
                rotate.Angle = 0;
            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) =>
            BeginRotation(toAngle: -22.5, durationSeconds: 0.1, EasingMode.EaseIn, resetAfter: false);

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e) =>
            BeginRotation(toAngle: 360, durationSeconds: 0.5, EasingMode.EaseOut, resetAfter: true);

        private void EnsureFreshRotateTransform()
        {
            if (AnimatedVisual is null)
            {
                return;
            }

            AnimatedVisual.RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(1, 1),
                    new TranslateTransform(),
                    new RotateTransform(),
                },
            };
        }

        private RotateTransform? TryGetRotate()
        {
            if (AnimatedVisual?.RenderTransform is TransformGroup group
                && group.Children.Count > 2
                && group.Children[2] is RotateTransform rotate)
            {
                return rotate;
            }

            return null;
        }

        private void BeginRotation(double toAngle, double durationSeconds, EasingMode easingMode, bool resetAfter)
        {
            if (TryGetRotate() is not { } rotate)
            {
                return;
            }

            _storyboard?.Stop();

            var animation = new DoubleAnimation
            {
                To = toAngle,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = new CircleEase { EasingMode = easingMode },
                FillBehavior = FillBehavior.HoldEnd,
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, rotate);
            Storyboard.SetTargetProperty(animation, new PropertyPath(RotateTransform.AngleProperty));
            storyboard.Children.Add(animation);

            if (resetAfter)
            {
                var reset = new DoubleAnimation
                {
                    To = 0,
                    BeginTime = TimeSpan.FromSeconds(durationSeconds),
                    Duration = TimeSpan.Zero,
                };
                Storyboard.SetTarget(reset, rotate);
                Storyboard.SetTargetProperty(reset, new PropertyPath(RotateTransform.AngleProperty));
                storyboard.Children.Add(reset);
            }

            _storyboard = storyboard;
            storyboard.Begin();
        }
    }
}
