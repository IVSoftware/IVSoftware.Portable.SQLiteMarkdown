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
        public static implicit operator NotifyCollectionChangedEventArgs(NotifyCollectionChangingEventArgs @this)
            => @this.EventArgsBCL;

        public static implicit operator NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs @this)
            => new NotifyCollectionChangingEventArgs(@this);

        public NotifyCollectionChangingEventArgs(NotifyCollectionChangedEventArgs eBCL)
        {
            NewItems = MakeOPC(eBCL.NewItems);
            OldItems = MakeOPC(eBCL.OldItems);

            NewStartingIndex = eBCL.NewStartingIndex;
            OldStartingIndex = eBCL.OldStartingIndex;
            EventArgsBCL = eBCL;
        }
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
            MakeBCL();
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

        private void MakeBCL()
        {
            bool hasIndex;
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
                        EventArgsBCL = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset);
                        break;
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
        public NotifyCollectionChangeScope Scope { get; } = NotifyCollectionChangeScope.ReadOnly;
        public bool IsBclCompatible { get; private set; } = true;

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