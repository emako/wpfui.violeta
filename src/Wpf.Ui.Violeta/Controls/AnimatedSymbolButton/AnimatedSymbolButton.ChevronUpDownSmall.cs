using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

public partial class AnimatedSymbolButton
{
    /// <summary>
    /// <c>AnimatedChevronUpDownSmallVisualSource</c> — 0°↔180° rotate (ComboBox chevron).
    /// Driven by <see cref="AnimatedSymbolButton.IsExpanded"/>.
    /// </summary>
    internal sealed class ChevronUpDownSmallBehavior : AnimatedSymbolBehavior
    {
        public override string DefaultGlyph => "\uE70D";

        public override Point RenderTransformOrigin => new(0.5, 0.5);

        protected override void OnAttached()
        {
            AnimatedVisual?.RenderTransform = new TransformGroup
            {
                Children =
                {
                    new ScaleTransform(1, 1),
                    new TranslateTransform(),
                    new RotateTransform(),
                },
            };

            ApplyExpanded(Owner?.IsExpanded == true, animate: false);
        }

        protected override void OnExpandedChanged(bool isExpanded)
        {
            ApplyExpanded(isExpanded, animate: true);
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

        private void ApplyExpanded(bool isExpanded, bool animate)
        {
            if (TryGetRotate() is not { } rotate)
            {
                return;
            }

            var to = isExpanded ? 180.0 : 0.0;
            if (!animate)
            {
                rotate.BeginAnimation(RotateTransform.AngleProperty, null);
                rotate.Angle = to;
                return;
            }

            rotate.BeginAnimation(
                RotateTransform.AngleProperty,
                new DoubleAnimation
                {
                    To = to,
                    Duration = TimeSpan.FromMilliseconds(167),
                    FillBehavior = FillBehavior.HoldEnd,
                });
        }
    }
}
