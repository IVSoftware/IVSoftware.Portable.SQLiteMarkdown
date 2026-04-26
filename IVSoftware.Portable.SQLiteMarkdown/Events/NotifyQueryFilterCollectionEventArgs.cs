using IVSoftware.Portable.Collections;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// Legacy change-action enum with BCL-aligned core values.
    /// </summary>
    /// <remarks>
    /// <see cref="Add"/>, <see cref="Remove"/>, <see cref="Replace"/>,
    /// <see cref="Move"/>, and <see cref="Reset"/> remain numerically aligned
    /// with <see cref="System.Collections.Specialized.NotifyCollectionChangedAction"/>.
    ///
    /// Use <see cref="IVSoftware.Portable.Collections.NotifyCollectionChangeAction"/>
    /// for the current generalized contract. Legacy-only extensions are
    /// <see cref="QueryResult"/>, <see cref="ApplyFilter"/>, and
    /// <see cref="RemoveFilter"/>.
    /// </remarks>
    [Flags]
    [Obsolete($"Backward compatibility only; use {nameof(IVSoftware.Portable.Collections.NotifyCollectionChangeAction)}")]
    public enum NotifyQueryFilterCollectionChangedAction
    {
        Add = NotifyCollectionChangedAction.Add,
        Remove = NotifyCollectionChangedAction.Remove,
        Replace = NotifyCollectionChangedAction.Replace,
        Move = NotifyCollectionChangedAction.Move,
        Reset = NotifyCollectionChangedAction.Reset,

        /// <summary>
        /// These items (old and new) represent a new canonical recordset.
        /// </summary>
        QueryResult = 0x1000,

        /// <summary>
        /// These items (old and new) represent a narrower subset.
        /// </summary>
        ApplyFilter = NotifyCollectionChangeReason.ApplyFilter,

        /// <summary>
        /// These items (old and new) represent a wider subset.
        /// </summary>
        RemoveFilter = NotifyCollectionChangeReason.RemoveFilter,
    }

    [Obsolete("Backward compatibility only. Use MarkdownContext.ModelSettledEventArgs for full capability.")]
    public class NotifyQueryFilterCollectionChangedEventArgs
    {
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList changedItems)
        {
            Action = action;
            Reason = ((int)action & ~0x7);
        }
        public NotifyQueryFilterCollectionChangedAction Action { get; }

        public int Reason { get; }
        internal NotifyCollectionChangeReason ReasonInternal => (NotifyCollectionChangeReason)Reason;
    }
}
