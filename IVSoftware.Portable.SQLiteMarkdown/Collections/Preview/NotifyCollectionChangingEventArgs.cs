using IVSoftware.Portable.Common.Exceptions;
using System;
using System.Collections;
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
            NewItems = localMakeOPC(newItems);
            OldItems = localMakeOPC(oldItems);
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
            ObservablePreviewCollection<object> localMakeOPC(IEnumerable? items)
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
        public NotifyCollectionChangeReason Reason { get; }
        public NotifyCollectionChangeScope Scope{ get; }

        public bool IsBclCompatible { get; private set; }

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

        const string SCOPE_POLICY_VIOLATION_MESSAGE = "TODO: Basically, you can't do this";
    }
}