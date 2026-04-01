using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class SuppressibleObservableCollection<T> 
        : ObservableCollection<T>
        , IEnumerable
        , IList
    {        
        public SuppressibleObservableCollection(NotifyCollectionChangeScope eventScope = NotifyCollectionChangeScope.CancelOnly)
        {
            EventScope = eventScope;
        }
        public NotifyCollectionChangeScope EventScope { get; }

        protected override void ClearItems()
        {
            base.ClearItems();
        }
#if false
        protected override void InsertItem(int index, T item)
        {
            Func<NotifyCollectionChangingEventArgs> action = () =>
                new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Add,
                        scope: EventScope,
                        newItems: new[] { item },
                        newStartingIndex: index);

           base.InsertItem(index, item);

            switch (DHostCoalesce.TryAppend(action))
            {
                case SuppressionPhase.None:
                    break;
                case SuppressionPhase.Preview:
                    break;
                case SuppressionPhase.Commit:
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void SetItem(int index, T item)
        {
            Func<NotifyCollectionChangingEventArgs> action = () =>
                new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Replace,
                        scope: EventScope,
                        newItems: new[] { item },
                        oldItems: new[] { this[index] },
                        newStartingIndex: index,
                        oldStartingIndex: index);

            switch (DHostCoalesce.TryAppend(action))
            {
                case SuppressionPhase.None:
                    base.SetItem(index, item);
                    break;
                case SuppressionPhase.Preview:
                    break;
                case SuppressionPhase.Commit:
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void RemoveItem(int index)
        {
            var item = this[index];
            Func<NotifyCollectionChangingEventArgs> action = () =>
                new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Remove,
                        scope: EventScope,
                        oldItems: new[] { item },
                        oldStartingIndex: index);


            switch (DHostCoalesce.TryAppend(action))
            {
                case SuppressionPhase.None:
                    base.RemoveItem(index);
                    break;
                case SuppressionPhase.Preview:
                    break;
                case SuppressionPhase.Commit:
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var item = this[oldIndex];

            Func<NotifyCollectionChangingEventArgs> action = () =>
                new NotifyCollectionChangingEventArgs(
                    action: NotifyCollectionChangeAction.Move,
                    scope: EventScope,
                    newItems: new[] { item },
                    oldItems: new[] { item },
                    newStartingIndex: newIndex,
                    oldStartingIndex: oldIndex);

            switch (DHostCoalesce.TryAppend(action))
            {
                case SuppressionPhase.None:
                    base.MoveItem(oldIndex, newIndex);
                    break;
                case SuppressionPhase.Preview:
                    break;
                case SuppressionPhase.Commit:
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
        protected override void ClearItems()
        {
            var snapshot = this.ToArray();

            Func<NotifyCollectionChangingEventArgs> action = () =>
                new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Reset,
                        scope: EventScope,
                        oldItems: snapshot,
                        oldStartingIndex: -1);


            switch (DHostCoalesce.TryAppend(action))
            {
                case SuppressionPhase.None:
                    base.ClearItems();
                    break;
                case SuppressionPhase.Preview:
                    break;
                case SuppressionPhase.Commit:
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {Phase.ToFullKey()} case is not supported.");
                    break;
            }
        }
#endif

        public IDisposable BeginSuppress() => DHostSuppress.GetToken(this);

        public void CancelSuppress() => DHostSuppress.CancelSuppressNotify();
        public SuppressionPhase Phase => DHostSuppress.Phase;

        public DHostSuppress<T> DHostSuppress
        {
            get
            {
                if (_dhostSuppress is null)
                {
                    _dhostSuppress = new DHostSuppress<T>();
                    _dhostSuppress.FinalDispose += (sender, e) => OnFinalCoalesce((CoalescingFinalDisposeEventArgs)e);
                }
                return _dhostSuppress;
            }
        }
        DHostSuppress<T>? _dhostSuppress = null;

        private void OnFinalCoalesce(CoalescingFinalDisposeEventArgs e)
        {
            OnCollectionChanged((e.Coalesced));
        }

        public new IEnumerator<T> GetEnumerator()
        {
            if(DHostSuppress.IsZero())
            {
                return base.GetEnumerator();
            }
            else
            {
                return DHostSuppress.Snapshot.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
