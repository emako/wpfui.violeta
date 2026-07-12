#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618, CS8619, CS8625

namespace Wpf.Ui.Violeta.Controls.Compat;

/// <summary>
/// Represents a method that handles general events.
/// </summary>
/// <typeparam name="TSender"></typeparam>
/// <typeparam name="TResult"></typeparam>
/// <param name="sender">The event source.</param>
/// <param name="args">The event data. If there is no event data, this parameter will be null.</param>
public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);
