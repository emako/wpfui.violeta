using System.Collections.Generic;
using System.Windows;

namespace Wpf.Ui.Violeta.Controls;

public class PinCodeCompletedEventArgs(IList<string> code, RoutedEvent? @event) : RoutedEventArgs(@event)
{
    public IList<string> Code { get; } = code;
}
