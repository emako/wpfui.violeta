namespace Wpf.Ui.Violeta.Controls;

public partial class AnimatedSymbolButton
{
    /// <summary>
    /// <see cref="AnimatedSymbolKind.Spin"/> — spinning arc in the symbol slot when
    /// <see cref="IsLoading"/> is <c>true</c> (visual matches <see cref="LoadingButton"/>).
    /// Spinner visibility and rotation are driven by the control template triggers.
    /// </summary>
    internal sealed class SpinBehavior : AnimatedSymbolBehavior
    {
        public override string DefaultGlyph => string.Empty;
    }
}
