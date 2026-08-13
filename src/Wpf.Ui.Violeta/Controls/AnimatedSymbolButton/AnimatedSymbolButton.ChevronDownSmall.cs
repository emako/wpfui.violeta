using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

public partial class AnimatedSymbolButton
{
    /// <summary>
    /// <c>AnimatedChevronDownSmallVisualSource</c> — clipped translate bounce (DropDownButton).
    /// </summary>
    internal sealed class ChevronDownSmallBehavior : AnimatedSymbolBehavior
    {
        private const double PressDepthRatio = 0.18;
        private const double OvershootRatio = 0.10;

        public override string DefaultGlyph => "\uE70D";

        public override Point RenderTransformOrigin => new(0.5, 0.5);

        public override bool ClipToBounds => true;

        public override Size ClipViewport => new(14, 12);

        protected override void OnAttached()
        {
            if (Owner is null || AnimatedVisual is null)
            {
                return;
            }

            // Template Freezables are immutable — install a fresh transform group.
            var scale = new ScaleTransform(1, 1);
            var translate = new TranslateTransform();
            var rotate = new RotateTransform();
            AnimatedVisual.RenderTransform = new TransformGroup
            {
                Children = { scale, translate, rotate },
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

            if (AnimatedVisual?.RenderTransform is TransformGroup group
                && group.Children.Count > 1
                && group.Children[1] is TranslateTransform translate)
            {
                translate.BeginAnimation(TranslateTransform.YProperty, null);
                translate.Y = 0;
            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) => BeginPress();

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e) => BeginRelease();

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (TryGetTranslate() is { } translate && Math.Abs(translate.Y) > 0.01)
            {
                BeginRelease();
            }
        }

        private double GetViewportHeight()
        {
            if (ClipHost?.ActualHeight > 0)
            {
                return ClipHost.ActualHeight;
            }

            return 12.0;
        }

        private TranslateTransform? TryGetTranslate()
        {
            if (AnimatedVisual?.RenderTransform is TransformGroup group
                && group.Children.Count > 1
                && group.Children[1] is TranslateTransform translate)
            {
                return translate;
            }

            return null;
        }

        private void BeginPress()
        {
            if (TryGetTranslate() is not { } translate)
            {
                return;
            }

            var depth = GetViewportHeight() * PressDepthRatio;
            var animation = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.HoldEnd };
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame(depth, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150)))
                {
                    KeySpline = new KeySpline(0.167, 0.167, 0.65, 1.0),
                });
            translate.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private void BeginRelease()
        {
            if (TryGetTranslate() is not { } translate)
            {
                return;
            }

            var overshoot = -(GetViewportHeight() * OvershootRatio);
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame(overshoot, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(83)))
                {
                    KeySpline = new KeySpline(0.55, 0.0, 0.75, 1.0),
                });
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(317)))
                {
                    KeySpline = new KeySpline(0.35, 0.0, 0.0, 1.0),
                });
            translate.BeginAnimation(TranslateTransform.YProperty, animation);
        }
    }
}
