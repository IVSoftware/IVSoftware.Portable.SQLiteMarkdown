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
                IEnumerable? newItems = null,
                IEnumerable? oldItems = null,
                int newStartingIndex = -1,
                int oldStartingIndex = -1
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
            // This *must* be done LAST.
            Scope = scope;
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