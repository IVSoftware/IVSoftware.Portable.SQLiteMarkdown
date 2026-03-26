using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;
using IVSoftware.Portable.Common.Exceptions;
using System.Collections.Generic;
using System.Collections.ObjectModel;


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

        public IList? NewItems 
        {
            get => 
                Scope == NotifyCollectionChangeScope.FullControl 
                ? _newItems 
                : (IList)NewItemsReadOnly; 
        }
        private readonly ObservableCollection<object> _newItems;

        public IReadOnlyList<object> NewItemsReadOnly
        {
            get
            {
                if (_newItemsReadOnly is null)
                {
                    _newItemsReadOnly = new ReadOnlyCollection<object>(null);
                }
                return _newItemsReadOnly;
            }
        }
        IReadOnlyList<object>? _newItemsReadOnly = null;

        public IList? OldItems
        {
            get =>
                Scope == NotifyCollectionChangeScope.FullControl
                ? _oldItems
                : (IList)OldItemsReadOnly;
        }
        private readonly ObservableCollection<object> _oldItems;

        public IReadOnlyList<object> OldItemsReadOnly
        {
            get
            {
                if (_oldItemsReadOnly is null)
                {
                    _oldItemsReadOnly = new ReadOnlyCollection<object>(null);
                }
                return _oldItemsReadOnly;
            }
        }
        IReadOnlyList<object>? _oldItemsReadOnly = null;

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
                    _oldStartingIndex = value;
                }
            }
        }
        int _oldStartingIndex = default;

        const string SCOPE_POLICY_VIOLATION_MESSAGE = "TODO: Basically, you can't do this";
    }
}