using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections.Preview
{
    internal class ObservableCoalescableCollection<T> : ObservableCollection<T>
    {
        public ObservableCoalescableCollection(NotifyCollectionChangeScope eventScope)
        {
            EventScope = eventScope;
        }
        public NotifyCollectionChangeScope EventScope { get; }
        protected override void InsertItem(int index, T item)
        {
            Func<NotifyCollectionChangingEventArgs> action = () =>
                new NotifyCollectionChangingEventArgs(
                        action: NotifyCollectionChangeAction.Add,
                        scope: EventScope,
                        newItems: new[] { item },
                        newStartingIndex: index);

            switch (DHostCoalesce.TryAppend(action))
            {
                case SuppressionPhase.None:
                    base.InsertItem(index, item);
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

        public IDisposable BeginCoalesce(SuppressionPhase phase) => DHostCoalesce.GetToken(phase, this);

        public void CancelCoalesce() => DHostCoalesce.CancelSuppressNotify();
        public SuppressionPhase Phase => DHostCoalesce.Phase;

        public DHostSuppress DHostCoalesce
        {
            get
            {
                if (_dhostCoalesce is null)
                {
                    _dhostCoalesce = new DHostSuppress();
                    _dhostCoalesce.FinalDispose += (sender, e) => OnFinalCoalesce((CoalescingFinalDisposeEventArgs)e);
                }
                return _dhostCoalesce;
            }
        }
        DHostSuppress? _dhostCoalesce = null;

        private void OnFinalCoalesce(CoalescingFinalDisposeEventArgs e)
        {
            OnCollectionChanged((e.Coalesced));
        }
    }
}
