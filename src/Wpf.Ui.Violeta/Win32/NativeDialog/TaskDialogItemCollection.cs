using System;
using System.Collections.ObjectModel;

namespace Wpf.Ui.Violeta.Win32;

/// <summary>
/// Represents a list of <see cref="TaskDialogItem"/> objects.
/// </summary>
/// <typeparam name="T">The type of the task dialog item.</typeparam>
/// <threadsafety instance="false" static="true" />
public class TaskDialogItemCollection<T> : Collection<T> where T : TaskDialogItem
{
    private readonly TaskDialog _owner;

    internal TaskDialogItemCollection(TaskDialog owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// Overrides the <see cref="Collection{T}.ClearItems"/> method.
    /// </summary>
    protected override void ClearItems()
    {
        foreach (T item in this)
        {
            item.Owner = null;
        }
        base.ClearItems();
        _owner.UpdateDialog();
    }

    /// <summary>
    /// Overrides the <see cref="Collection{T}.InsertItem"/> method.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The object to insert. May not be <see langword="null" />.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> is already associated with a different task dialog.</exception>
    /// <exception cref="InvalidOperationException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> has a duplicate id or button type.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para>
    ///   <paramref name="index"/> is less than zero.
    /// </para>
    /// <para>
    ///   -or-
    /// </para>
    /// <para>
    ///   <paramref name="index" /> is equal to or greater than <see cref="Collection{T}.Count"/>.
    /// </para>
    /// </exception>
    protected override void InsertItem(int index, T item)
    {
#pragma warning disable IDE0011 // Suppress IDE0011: Remove "add braces" style suggestion temporarily
#pragma warning disable CA1510 // Suppress CA1510: Disable maintainability rule for manual null check
        if (item == null)
            throw new ArgumentNullException(nameof(item));
#pragma warning restore IDE0011 // Restore IDE0011 brace style check for subsequent code
#pragma warning restore CA1510 // Restore CA1510 null argument helper rule

        if (item.Owner != null)
            throw new ArgumentException(TaskDialogResources.TaskDialogItemHasOwnerError);

        item.Owner = _owner;
        try
        {
            item.CheckDuplicate(null!);
        }
        catch (InvalidOperationException)
        {
            item.Owner = null;
            throw;
        }
        base.InsertItem(index, item);
        _owner.UpdateDialog();
    }

    /// <summary>
    /// Overrides the <see cref="Collection{T}.RemoveItem"/> method.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para>
    ///   <paramref name="index"/> is less than zero.
    /// </para>
    /// <para>
    ///   -or-
    /// </para>
    /// <para>
    ///   <paramref name="index" /> is equal to or greater than <see cref="Collection{T}.Count"/>.
    /// </para>
    /// </exception>
    protected override void RemoveItem(int index)
    {
        base[index].Owner = null;
        base.RemoveItem(index);
        _owner.UpdateDialog();
    }

    /// <summary>
    /// Overrides the <see cref="Collection{T}.SetItem"/> method.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The new value for the element at the specified index. May not be <see langword="null" />.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> is already associated with a different task dialog.</exception>
    /// <exception cref="InvalidOperationException">The <see cref="TaskDialogItem"/> specified in <paramref name="item" /> has a duplicate id or button type.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para>
    ///   <paramref name="index"/> is less than zero.
    /// </para>
    /// <para>
    ///   -or-
    /// </para>
    /// <para>
    ///   <paramref name="index" /> is equal to or greater than <see cref="Collection{T}.Count"/>.
    /// </para>
    /// </exception>
    protected override void SetItem(int index, T item)
    {
#pragma warning disable IDE0011 // Suppress IDE0011: Remove "add braces" style suggestion temporarily
#pragma warning disable CA1510 // Suppress CA1510: Disable maintainability rule for manual null check
        if (item == null)
            throw new ArgumentNullException(nameof(item));
#pragma warning restore IDE0011 // Restore IDE0011 brace style check for subsequent code
#pragma warning restore CA1510 // Restore CA1510 null argument helper rule

        if (base[index] != item)
        {
            if (item.Owner != null)
                throw new ArgumentException(TaskDialogResources.TaskDialogItemHasOwnerError);
            item.Owner = _owner;
            try
            {
                item.CheckDuplicate(base[index]);
            }
            catch (InvalidOperationException)
            {
                item.Owner = null;
                throw;
            }
            base[index].Owner = null;
            base.SetItem(index, item);
            _owner.UpdateDialog();
        }
    }
}
