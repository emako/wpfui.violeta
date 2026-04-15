#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

namespace Wpf.Ui.Violeta.Controls.Compat;

public class IconSourceToIconElementConverter : AdvancedValueConverterBase<IconSource, IconElement>
{
    public override IconElement DoConvert(IconSource from)
    {
        return from.CreateIconElement();
    }

    public override IconSource DoConvertBack(IconElement to)
    {
        return to.CreateIconSource();
    }
}
