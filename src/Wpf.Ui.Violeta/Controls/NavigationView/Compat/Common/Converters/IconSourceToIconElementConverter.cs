using Wpf.Ui.Violeta.Controls.Compat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wpf.Ui.Violeta.Controls.Compat;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls.Compat
{
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
}


