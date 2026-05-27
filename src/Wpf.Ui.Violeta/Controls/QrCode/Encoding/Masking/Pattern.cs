using System;

namespace Wpf.Ui.Violeta.Controls.Encoding.Masking;

internal abstract class Pattern : BitMatrix
{
    public override int Width => throw new NotSupportedException();
    public override int Height => throw new NotSupportedException();

    public override bool[,] InternalArray => throw new NotImplementedException();

    public abstract MaskPatternType MaskPatternType { get; }
}
