using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
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
        public static implicit operator NotifyCollectionChangedEventArgs(NotifyCollectionChangingEventArgs @this)
            => @this.EventArgsBCL;

        public static implicit operator NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs @this)
            => new NotifyCollectionChangingEventArgs(@this);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs eBCL)
        : this(
              action: (NotifyCollectionChangeAction)eBCL.Action,
              newItems: eBCL.NewItems,
              oldItems: eBCL.OldItems,
              newStartingIndex: eBCL.NewStartingIndex,
              oldStartingIndex: eBCL.OldStartingIndex)
        { }

        /// <summary>
        /// Constructor designed to encourage named args.
        /// </summary>
        public NotifyCollectionChangingEventArgs(
                NotifyCollectionChangeAction action,
                NotifyCollectionChangeReason reason = NotifyCollectionChangeReason.None,
                NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly,
                IEnumerable? newItems = null,
                IEnumerable? oldItems = null,
                int newStartingIndex = -1,
                int oldStartingIndex = -1,
                NotifyCollectionChangedEventArgs? eBcl = null
            )
        {
            Action = action;
            Reason = reason;
            if (newItems is IEnumerable @new)
            {
                foreach (var item in @new)
                {
                    NewItems.Add(item);
                }
            }
            NewItemsReadOnly = NewItems.ToArray();

            if (oldItems is IEnumerable old)
            {
                foreach (var item in old)
                {
                    OldItems.Add(item);
                }
            }
            OldItemsReadOnly = OldItems.ToArray();

            NewItems.CollectionChanged += (sender, e) =>
            {
                if (!_isReverting)
                {
                    if (Scope != NotifyCollectionChangeScope.FullControl)
                    {
                        try
                        {
                            _isReverting = true;
                            NewItems.Clear();
                            foreach (var item in NewItemsReadOnly)
                            {
                                NewItems.Add(item);
                            }
                        }
                        finally
                        {
                            _isReverting = false;
                        }
                        this.ThrowHard<InvalidOperationException>(SCOPE_POLICY_VIOLATION_MESSAGE);
                    }
                }
            };
            OldItems.CollectionChanged += (sender, e) =>
            {
                if (!_isReverting)
                {
                    if (Scope != NotifyCollectionChangeScope.FullControl)
                    {
                        try
                        {
                            _isReverting = true;
                            OldItems.Clear();
                            foreach (var item in OldItemsReadOnly)
                            {
                                OldItems.Add(item);
                            }
                        }
                        finally
                        {
                            _isReverting = false;
                        }
                        this.ThrowHard<InvalidOperationException>(SCOPE_POLICY_VIOLATION_MESSAGE);
                    }
                }
            };
            MakeBCL(eBcl);

            // This *must* be done LAST.
            Scope = scope;
        }
        bool _isReverting = false;

        private void MakeBCL(NotifyCollectionChangedEventArgs? eBcl)
        {
            if (eBcl is not null)
            {
                EventArgsBCL = eBcl;
            }
            else
            {
                bool hasIndex;
                // Screen for batch ops that involve EventArgs classes as "new items".
                if (NewItems.Count > 0 && NewItems[0] is EventArgs)
                {
                    IsBclCompatible = false;
                    MakeReset(false);
                }
                else
                {
                    switch (Action)
                    {
                        case NotifyCollectionChangeAction.Add:
                            if (NewStartingIndex < -1)
                            {
                                MakeReset(true);
                                return;
                            }
                            if (NewItems.Count == 0)
                            {
                                MakeReset(true);
                                return;
                            }

                            hasIndex = NewStartingIndex >= 0;

                            if (NewItems.Count == 1)
                            {
                                if (hasIndex)
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Add,
                                        changedItem: NewItems[0],
                                        index: NewStartingIndex);
                                }
                                else
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Add,
                                        changedItem: NewItems[0]);
                                }
                            }
                            else
                            {
                                EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                    NotifyCollectionChangedAction.Add,
                                    changedItems: NewItems,
                                    startingIndex: hasIndex ? NewStartingIndex : -1);
                            }
                            break;

                        case NotifyCollectionChangeAction.Remove:
                            if (OldStartingIndex < -1)
                            {
                                MakeReset(true);
                                return;
                            }
                            if (OldItems.Count == 0)
                            {
                                MakeReset(true);
                                return;
                            }

                            hasIndex = OldStartingIndex >= 0;

                            if (OldItems.Count == 1)
                            {
                                if (hasIndex)
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Remove,
                                        changedItem: OldItems[0],
                                        index: OldStartingIndex);
                                }
                                else
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Remove,
                                        changedItem: OldItems[0]);
                                }
                            }
                            else
                            {
                                EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                    NotifyCollectionChangedAction.Remove,
                                    changedItems: OldItems,
                                    startingIndex: hasIndex ? OldStartingIndex : -1);
                            }
                            break;

                        case NotifyCollectionChangeAction.Replace:
                            if (NewItems.Count != OldItems.Count)
                            {
                                MakeReset(true);
                                return;
                            }

                            hasIndex = NewStartingIndex >= 0 && OldStartingIndex >= 0;

                            if (NewItems.Count == 1)
                            {
                                if (hasIndex)
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Replace,
                                        newItem: NewItems[0],
                                        oldItem: OldItems[0],
                                        index: NewStartingIndex);
                                }
                                else
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Replace,
                                        newItem: NewItems[0],
                                        oldItem: OldItems[0]);
                                }
                            }
                            else
                            {
                                if (hasIndex)
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Replace,
                                        newItems: NewItems,
                                        oldItems: OldItems,
                                        startingIndex: NewStartingIndex);
                                }
                                else
                                {
                                    EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                        NotifyCollectionChangedAction.Replace,
                                        newItems: NewItems,
                                        oldItems: OldItems);
                                }
                            }
                            break;
                        case NotifyCollectionChangeAction.Move:
                            if (NewItems.Count != OldItems.Count)
                            {
                                MakeReset(true);
                                return;
                            }
                            if (NewStartingIndex < 0 || OldStartingIndex < 0)
                            {
                                MakeReset(true);
                                return;
                            }
                            if (NewItems.Count == 0)
                            {
                                MakeReset(true);
                                return;
                            }

                            if (NewItems.Count == 1)
                            {
                                EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                    NotifyCollectionChangedAction.Move,
                                    changedItem: NewItems[0],
                                    index: NewStartingIndex,
                                    oldIndex: OldStartingIndex);
                            }
                            else
                            {
                                EventArgsBCL = new NotifyCollectionChangedEventArgs(
                                    NotifyCollectionChangedAction.Move,
                                    changedItems: NewItems,
                                    index: NewStartingIndex,
                                    oldIndex: OldStartingIndex);
                            }
                            break;
                        case NotifyCollectionChangeAction.Reset:
                            MakeReset(false);
                            break;
                    }
                }
            }
        }

        private NotifyCollectionChangedEventArgs EventArgsBCL = null!;
        private void MakeReset(bool ieException)
        {
            if (ieException)
            {
                this.ThrowHard<InvalidOperationException>(CONFIGURATION_INVALID_MESSAGE);
                Reason |= NotifyCollectionChangeReason.Exception;
            }
            EventArgsBCL = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset);
        }

        public NotifyCollectionChangeAction Action { get; }
        public NotifyCollectionChangeReason Reason { get; private set; } = NotifyCollectionChangeReason.None;

        /// <summary>
        /// Initialize to FullControl to allow construction of the event.
        /// </summary>
        /// <remarks>
        /// The final value of Scope is set before the CTor exits.
        /// </remarks>
        public NotifyCollectionChangeScope Scope { get; private set; } = NotifyCollectionChangeScope.FullControl;
        public bool IsBclCompatible { get; private set; } = true;

        public ObservableCollection<object?> NewItems { get; } = new();

        private object?[] NewItemsReadOnly { get; } = [];

        public ObservableCollection<object?> OldItems { get; } = new();
        public object?[] OldItemsReadOnly { get; } = [];

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
    }
}