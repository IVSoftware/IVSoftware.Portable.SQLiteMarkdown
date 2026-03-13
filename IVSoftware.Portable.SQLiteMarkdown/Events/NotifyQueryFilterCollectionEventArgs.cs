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

        #region C O N T E X T
        /// <summary>
        /// The changes are taking place inside a QueryResult action
        /// </summary>
        QueryResult = 0x1000,

        ApplyFilter = 0x2000,
        RemoveFilter = 0x4000,
        #endregion C O N T E X T
    }

    [Obsolete("Backward compatibility only. Use MarkdownContext.ModelSettledEventArgs for full capability.")]
    public class NotifyQueryFilterCollectionChangedEventArgs : ModelSettledEventArgs
    {
        public NotifyQueryFilterCollectionChangedEventArgs(NotifyQueryFilterCollectionChangedAction action, IList changedItems)
            : base(
                  reason: (NotifyCollectionChangedReason)((int)action & ~0x7),
                  action: (NotifyCollectionChangedAction)((int)action & 0x07),
                  changedItems: changedItems)
        { }
        public new NotifyQueryFilterCollectionChangedAction Action
            => (NotifyQueryFilterCollectionChangedAction)base.Action;
    }
}
