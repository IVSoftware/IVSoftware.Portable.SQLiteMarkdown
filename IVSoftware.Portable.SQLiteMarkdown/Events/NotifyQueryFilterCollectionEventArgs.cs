using IVSoftware.Portable.SQLiteMarkdown.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace IVSoftware.Portable.SQLiteMarkdown.Events
{
    [Flags]
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
        QueryResult = NotifyCollectionChangeReason.QueryResult,

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
    public class NotifyQueryFilterCollectionChangedEventArgs : ModelSettledEventArgs
    {
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList changedItems)
            : base(
                  reason: (NotifyCollectionChangeReason)((int)action & ~0x7),
                  action: (NotifyCollectionChangedAction)((int)action & 0x07),
                  changedItems: changedItems)
        { }
        public new NotifyQueryFilterCollectionChangedAction Action
            => (NotifyQueryFilterCollectionChangedAction)base.Action;
    }
}
