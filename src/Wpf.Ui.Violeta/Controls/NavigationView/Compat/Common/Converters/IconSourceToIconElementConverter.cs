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
