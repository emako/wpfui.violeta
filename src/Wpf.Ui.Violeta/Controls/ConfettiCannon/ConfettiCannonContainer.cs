using System.Windows.Documents;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Local host for <see cref="ConfettiCannon"/> particle rendering (AdornerDecorator).
/// Place this in a page region and pass the same Token to <see cref="ConfettiCannon.Options.Token"/>.
/// </summary>
public class ConfettiCannonContainer : AdornerDecorator;
