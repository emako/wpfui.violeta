using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Wpf.Ui.Violeta.Controls;

public partial class AnimatedSymbolButton
{
    /// <summary>
    /// WinUI Gallery <c>CopyToClipboardSuccessAnimation</c> — copy → Accept morph (<see cref="CopyButton"/>).
    /// Storyboard lives in <c>AnimatedSymbolButton.xaml</c> (<c>CopyToClipboardSuccessAnimation</c>).
    /// </summary>
    internal sealed class CopyToClipboardBehavior : AnimatedSymbolBehavior
    {
        private Storyboard? _successAnimation;

        public override string DefaultGlyph => "\uE8C8";

        public override Point RenderTransformOrigin => new(0.5, 0.5);

        public bool IsAnimating { get; private set; }

        protected override void OnAttached()
        {
            ResolveStoryboard();
        }

        protected override void OnDetaching()
        {
            if (_successAnimation is not null)
            {
                _successAnimation.Completed -= OnSuccessAnimationCompleted;
                _successAnimation.Stop();
                _successAnimation = null;
            }

            IsAnimating = false;
            AnimatedVisual?.Opacity = 1;
            SuccessGlyph?.Opacity = 0;
        }

        public override void OnClick()
        {
            PlaySuccessAnimation();
        }

        private void ResolveStoryboard()
        {
            _successAnimation?.Completed -= OnSuccessAnimationCompleted;
            _successAnimation = null;

            if (RootGrid?.Resources["CopyToClipboardSuccessAnimation"] is Storyboard storyboard)
            {
                _successAnimation = storyboard.IsFrozen ? storyboard.Clone() : storyboard;
                _successAnimation.Completed += OnSuccessAnimationCompleted;
            }
        }

        private void PlaySuccessAnimation()
        {
            if (RootGrid is null || _successAnimation is null || IsAnimating)
            {
                return;
            }

            IsAnimating = true;
            _successAnimation.Begin(RootGrid, true);
        }

        private void OnSuccessAnimationCompleted(object? sender, EventArgs e)
        {
            IsAnimating = false;
        }
    }
}
