using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal sealed class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        NotifyCollectionChangedEventArgs? EventArgsBCL = null;

        public static implicit operator NotifyCollectionChangedEventArgs(NotifyCollectionChangingEventArgs @this)
            => @this.EventArgsBCL ?? @this.MakeBCL();

        public static implicit operator NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs @this)
            => new NotifyCollectionChangingEventArgs(@this);

        public NotifyCollectionChangingEventArgs(
            NotifyCollectionChangedEventArgs eBCL,
            NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None,
            NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly)
        : this(
              action: (NotifyCollectionChangeAction)eBCL.Action,
              reason: reason,
              newItems: eBCL.NewItems,
              oldItems: eBCL.OldItems,
              newStartingIndex: eBCL.NewStartingIndex,
              oldStartingIndex: eBCL.OldStartingIndex)
        {
            EventArgsBCL = eBCL;
        }

        /// <summary>
        /// Constructor designed to encourage named args.
        /// </summary>
        public NotifyCollectionChangingEventArgs(
                NotifyCollectionChangeAction action,
                NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None,
                NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly,
                IList? newItems = null,
                IList? oldItems = null,
                int newStartingIndex = -1,
                int oldStartingIndex = -1
            )
        {
            Action = action;
            Reason = reason;
            Scope = scope;
            NewItems = new MutationPreviewCollection(newItems, scope);
            OldItems = new MutationPreviewCollection(oldItems, scope);
        }
        bool _isReverting = false;

        private NotifyCollectionChangedEventArgs MakeBCL()
        {
            NotifyCollectionChangedEventArgs eBcl =
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset); // sentinel

            bool makeReset = false;
            bool ieException = false;

            if (NewItems.Count > 0 && NewItems[0] is EventArgs)
            {
                IsBclCompatible = false;
                makeReset = true;
            }
            else
            {
                switch (Action)
                {
                    case NotifyCollectionChangeAction.Add:
                        {
                            if (NewStartingIndex < -1 || NewItems.Count == 0)
                            {
                                makeReset = true;
                                ieException = true;
                            }
                            else
                            {
                                bool hasIndex = NewStartingIndex >= 0;

                                eBcl = NewItems.Count == 1
                                    ? hasIndex
                                        ? new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Add,
                                            NewItems[0],
                                            NewStartingIndex)
                                        : new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Add,
                                            NewItems[0])
                                    : new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Add,
                                        NewItems,
                                        hasIndex ? NewStartingIndex : -1);
                            }
                            break;
                        }

                    case NotifyCollectionChangeAction.Remove:
                        {
                            if (OldStartingIndex < -1 || OldItems.Count == 0)
                            {
                                makeReset = true;
                                ieException = true;
                            }
                            else
                            {
                                bool hasIndex = OldStartingIndex >= 0;

                                eBcl = OldItems.Count == 1
                                    ? hasIndex
                                        ? new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Remove,
                                            OldItems[0],
                                            OldStartingIndex)
                                        : new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Remove,
                                            OldItems[0])
                                    : new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Remove,
                                        OldItems,
                                        hasIndex ? OldStartingIndex : -1);
                            }
                            break;
                        }

                    case NotifyCollectionChangeAction.Replace:
                        {
                            if (NewItems.Count != OldItems.Count)
                            {
                                makeReset = true;
                                ieException = true;
                            }
                            else
                            {
                                bool hasIndex = NewStartingIndex >= 0 && OldStartingIndex >= 0;

                                eBcl = NewItems.Count == 1
                                    ? hasIndex
                                        ? new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Replace,
                                            NewItems[0],
                                            OldItems[0],
                                            NewStartingIndex)
                                        : new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Replace,
                                            NewItems[0],
                                            OldItems[0])
                                    : hasIndex
                                        ? new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Replace,
                                            NewItems,
                                            OldItems,
                                            NewStartingIndex)
                                        : new NotifyCollectionChangedEventArgs(
                                            NotifyCollectionChangedAction.Replace,
                                            NewItems,
                                            OldItems);
                            }
                            break;
                        }

                    case NotifyCollectionChangeAction.Move:
                        {
                            if (NewItems.Count != OldItems.Count ||
                                NewStartingIndex < 0 ||
                                OldStartingIndex < 0 ||
                                NewItems.Count == 0)
                            {
                                makeReset = true;
                                ieException = true;
                            }
                            else
                            {
                                eBcl = NewItems.Count == 1
                                    ? new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Move,
                                        NewItems[0],
                                        NewStartingIndex,
                                        OldStartingIndex)
                                    : new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Move,
                                        NewItems,
                                        NewStartingIndex,
                                        OldStartingIndex);
                            }
                            break;
                        }

                    case NotifyCollectionChangeAction.Reset:
                        makeReset = true;
                        break;
                }
            }

            if (makeReset)
            {
                if (ieException)
                {
                    this.ThrowHard<InvalidOperationException>(CONFIGURATION_INVALID_MESSAGE);
                    Reason |= NotifyCollectionChangeReason.Exception;
                }

                return new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset);
            }
            else 
            {
                return eBcl;
            }
        }

        public NotifyCollectionChangeAction Action { get; }
        public NotifyCollectionChangeReason Reason { get; private set; } = NotifyCollectionChangeReason.None;
        public NotifyCollectionChangeScope Scope { get; }
        public bool IsBclCompatible { get; private set; } = true;

        public IList NewItems { get; }

        public IList OldItems { get; }

        public int NewStartingIndex
        {
            get => _newStartingIndex;
            set
            {
                if (!Equals(_newStartingIndex, value))
                {
                    if (Scope == NotifyCollectionChangeScope.FullControl)
                    {
                        _newStartingIndex = value;
                    }
                    else
                    {
                        this.ThrowHard<InvalidOperationException>(SCOPE_POLICY_VIOLATION_MESSAGE);
                    }
                }
            }
        }
        int _newStartingIndex = default;

        public int OldStartingIndex
        {
            get => _oldStartingIndex;
            set
            {
                if (!Equals(_oldStartingIndex, value))
                {
                    if (Scope == NotifyCollectionChangeScope.FullControl)
                    {
                        _oldStartingIndex = value;
                    }
                    else
                    {
                        this.ThrowHard<InvalidOperationException>(SCOPE_POLICY_VIOLATION_MESSAGE);
                    }
                }
            }
        }
        int _oldStartingIndex = default;
        string SCOPE_POLICY_VIOLATION_MESSAGE =>    
            $"This operation is not permitted: {nameof(NotifyCollectionChangeScope)}={Scope.ToFullKey()}," +
            $"Always check Scope before attempting to modify the change proposal.";

        string CONFIGURATION_INVALID_MESSAGE =>
            $"This configuration is not permitted as specified: Check related properties and ensure the combination is valid.";

        /// <summary>
        /// Provides a scope-enforced preview surface for collection mutation proposals.
        /// </summary>
        /// <remarks>
        /// This collection is used exclusively within <see cref="NotifyCollectionChangingEventArgs"/>
        /// to expose <c>NewItems</c> and <c>OldItems</c> as mutable lists prior to BCL emission.
        ///
        /// Construction occurs under temporary <c>FullControl</c> in order to materialize a complete
        /// snapshot of the proposed items. Once populated, the declared <see cref="NotifyCollectionChangeScope"/>
        /// is restored and enforced for all subsequent mutations.
        ///
        /// Consumers may observe, cancel, or rewrite the proposal depending on scope:
        /// - <c>ReadOnly</c>: No mutation permitted.
        /// - <c>CancelOnly</c>: Mutation blocked, but proposal may be canceled.
        /// - <c>FullControl</c>: Full mutation permitted.
        ///
        /// All mutation entry points are intercepted to enforce scope policy. Violations are
        /// routed through <c>ThrowHard</c>, allowing escalation or advisory handling.
        /// </remarks>
        class MutationPreviewCollection : ObservableCollection<object>
        {
            const string SCOPE_POLICY_VIOLATION_MESSAGE =
                "Collection mutation requires FullControl scope.";

            public MutationPreviewCollection(IList? items, NotifyCollectionChangeScope scope)
            {
                Scope = NotifyCollectionChangeScope.FullControl;
                foreach (var item in items ?? Array.Empty<object>())
                {
                    Add(item);
                }
                Scope = scope;
            }

            public NotifyCollectionChangeScope Scope { get; }

            bool EnforceMutationPolicy()
            {
                if (Scope.HasFlag(NotifyCollectionChangeScope.FullControl))
                {
                    return true;
                }
                else
                {
                    this.ThrowHard<InvalidOperationException>(SCOPE_POLICY_VIOLATION_MESSAGE);
                    return false;
                }
            }

            protected override void ClearItems()
            {
                if (EnforceMutationPolicy())
                {
                    base.ClearItems();
                }
            }

            protected override void InsertItem(int index, object item)
            {
                if (EnforceMutationPolicy())
                {
                    base.InsertItem(index, item);
                }
            }
            protected override void SetItem(int index, object item)
            {
                if (EnforceMutationPolicy())
                {
                    base.SetItem(index, item);
                }
            }

            protected override void RemoveItem(int index)
            {
                if (EnforceMutationPolicy())
                {
                    base.RemoveItem(index);
                }
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                if (EnforceMutationPolicy())
                {
                    base.MoveItem(oldIndex, newIndex);
                }
            }
        }
    }
}