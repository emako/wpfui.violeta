//
//  Wpf.Ui.Emoji — Emoji support for WPF
//
//  Copyright © 2017–2021 Sam Hocevar <sam@hocevar.net>
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Wpf.Ui.Emoji
{
    internal static class Extensions
    {
        /// <summary>
        /// Advance a TextPointer to the nth character
        /// </summary>
        public static TextPointer GetPositionAtCharOffset(this TextPointer p, int offset)
        {
            var fallback = offset > 0 ? p.DocumentEnd : p.DocumentStart;
            while (offset != 0 && p != null)
            {
                var dir = offset > 0 ? LogicalDirection.Forward : LogicalDirection.Backward;
                if (p.GetPointerContext(dir) == TextPointerContext.Text)
                {
                    var text = p.GetTextInRun(dir);
                    if (text.Length >= Math.Abs(offset))
                        return p.GetPositionAtOffset(offset);
                    offset -= Math.Sign(offset) * text.Length;
                }
                p = p.GetNextContextPosition(dir);
            }
            return p ?? fallback;
        }

        public static SolidColorBrush ToBrush(this Color c, bool freeze = true)
        {
            var brush = new SolidColorBrush(c);

            if (freeze && brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }
    }
}
