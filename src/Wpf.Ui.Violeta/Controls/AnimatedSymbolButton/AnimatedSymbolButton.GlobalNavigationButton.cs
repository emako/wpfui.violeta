using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Wpf.Ui.Violeta.Controls;

public partial class AnimatedSymbolButton
{
    /// <summary>
    /// <c>AnimatedGlobalNavigationButtonVisualSource</c> — horizontal squash
    /// (NavigationView pane toggle / hamburger).
    /// </summary>
    internal sealed class GlobalNavigationButtonBehavior : AnimatedSymbolBehavior
    {
        public override string DefaultGlyph => "\uF4E1";

        public override Point RenderTransformOrigin => new(0.5, 0.5);

        protected override void OnAttached()
        {
            if (Owner is null || AnimatedVisual is null)
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

            Owner.AddHandler(PreviewMouseLeftButtonDownEvent, (MouseButtonEventHandler)OnPreviewMouseDown, true);
            Owner.AddHandler(PreviewMouseLeftButtonUpEvent, (MouseButtonEventHandler)OnPreviewMouseUp, true);
            Owner.LostMouseCapture += OnLostMouseCapture;
        }

        protected override void OnDetaching()
        {
            if (Owner is null)
            {
                return;
            }

            Owner.RemoveHandler(PreviewMouseLeftButtonDownEvent, (MouseButtonEventHandler)OnPreviewMouseDown);
            Owner.RemoveHandler(PreviewMouseLeftButtonUpEvent, (MouseButtonEventHandler)OnPreviewMouseUp);
            Owner.LostMouseCapture -= OnLostMouseCapture;
            AnimateScaleX(1.0, TimeSpan.FromMilliseconds(80));
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) =>
            AnimateScaleX(0.66, TimeSpan.FromMilliseconds(80));

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e) =>
            AnimateScaleX(1.0, TimeSpan.FromMilliseconds(80));

        private void OnLostMouseCapture(object sender, MouseEventArgs e) =>
            AnimateScaleX(1.0, TimeSpan.FromMilliseconds(80));

        private ScaleTransform? TryGetScale()
        {
            if (AnimatedVisual?.RenderTransform is TransformGroup group
                && group.Children.Count > 0
                && group.Children[0] is ScaleTransform scale)
            {
                return scale;
            }

            return null;
        }

        private void AnimateScaleX(double to, TimeSpan duration)
        {
            TryGetScale()?.BeginAnimation(
                ScaleTransform.ScaleXProperty,
                CreateDoubleAnimation(to, duration));
        }
    }
}
