using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;


namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal sealed class NotifyCollectionChangingEventArgs : CancelEventArgs
    {
        public NotifyCollectionChangingEventArgs(
                NotifyCollectionChangeAction action,
                NotifyCollectionChangeReason reason,
                NotifyCollectionChangeScope scope = NotifyCollectionChangeScope.ReadOnly,
                IEnumerable? newItems = null,
                IEnumerable? oldItems = null,
                int newStartingIndex = -1,
                int oldStartingIndex = -1
            )
        {
            NewItems = MakeOPC(newItems);
            OldItems = MakeOPC(oldItems);
            NewItems.CollectionChanging += (sender, e) =>
            {
                if(Scope != NotifyCollectionChangeScope.FullControl)
                {
                    this.ThrowHard<InvalidOperationException>(SCOPE_POLICY_VIOLATION_MESSAGE);
                    e.Cancel = true;
                }
            };
            OldItems.CollectionChanging += (sender, e) =>
            {
                if (Scope != NotifyCollectionChangeScope.FullControl)
                {
                    this.ThrowHard<InvalidOperationException>(SCOPE_POLICY_VIOLATION_MESSAGE);
                    e.Cancel = true;
                }
            };
            _eBCL = MakeBCL();

            #region L o c a l F x
            #endregion L o c a l F x
        }
        private ObservablePreviewCollection<object> MakeOPC(IEnumerable? items)
        {
            ObservablePreviewCollection<object> opc = new();
            if(items is not null)
            {
                foreach (var item in items)
                {
                    opc.Add(item);
                }
            }
            return opc;
        }

        private NotifyCollectionChangedEventArgs MakeBCL()
        {
            switch (Action)
            {
                case NotifyCollectionChangeAction.Add:
                    if (NewStartingIndex < -1)
                    {
                        return MakeReset(true);
                    }
                    switch (NewItems.Count)
                    {
                        case 0: 
                            return MakeReset(true);
                        case 1:
                            if(NewStartingIndex == -1)
                            { 
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Add,
                                    changedItem: NewItems[0]);
                            }
                            else
                            {
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Add,
                                    changedItem: NewItems[0],
                                    index: NewStartingIndex);
                            }
                        default:
                            if (NewStartingIndex == -1)
                            {
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Add,
                                    changedItems: NewItems);
                            }
                            else
                            {
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Add,
                                    changedItems: NewItems,
                                    startingIndex: NewStartingIndex);
                            }
                    }
                case NotifyCollectionChangeAction.Remove:
                    if (OldStartingIndex < -1)
                    {
                        return MakeReset(true);
                    }
                    switch (OldItems.Count)
                    {
                        case 0:
                            return MakeReset(true);
                        case 1:
                            if (OldStartingIndex == -1)
                            {
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Remove,
                                    changedItem: OldItems[0]);
                            }
                            else
                            {
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Remove,
                                    changedItem: OldItems[0],
                                    index: OldStartingIndex);
                            }
                        default:
                            if (OldStartingIndex == -1)
                            {
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Remove,
                                    changedItems: OldItems);
                            }
                            else
                            {
                                return new NotifyCollectionChangedEventArgs(
                                    action: NotifyCollectionChangedAction.Remove,
                                    changedItems: OldItems,
                                    startingIndex: OldStartingIndex);
                            }
                    }
                case NotifyCollectionChangeAction.Replace:
                    break;
                case NotifyCollectionChangeAction.Move:
                    break;
                case NotifyCollectionChangeAction.Reset:
                    return new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset);
                default:
                    break;
            }
            if (Action == NotifyCollectionChangeAction.Reset)
            {
            }
            throw new NotImplementedException("ToDo");
        }

        private NotifyCollectionChangedEventArgs MakeReset(bool ieException)
        {
            if (ieException)
            {
                this.ThrowHard<InvalidOperationException>(CONFIGURATION_INVALID_MESSAGE);
                Reason |= NotifyCollectionChangeReason.Exception;
            }
            return new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset);
        }

        public NotifyCollectionChangeAction Action
        {
            get => _action;
            set
            {
                _action = value;
            }
        }
        NotifyCollectionChangeAction _action = default;
        public NotifyCollectionChangeReason Reason { get; private set; }
        public NotifyCollectionChangeScope Scope{ get; }
        public bool IsBclCompatible { get; private set; } = true;

        private NotifyCollectionChangedEventArgs _eBCL;

        public ObservablePreviewCollection<object> NewItems { get; }

        public ObservablePreviewCollection<object> OldItems { get; }

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