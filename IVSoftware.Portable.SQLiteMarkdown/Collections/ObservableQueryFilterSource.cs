using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Collections.Preview;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IVSoftware.Portable.SQLiteMarkdown.Collections
{
    /// <summary>
    /// Provides a query-then-filter state engine for collections of <typeparamref name="T"/>, 
    /// supporting expression-based parsing, SQLite-backed filtering, and in-memory dataset routing. 
    /// 
    /// This class is UI-agnostic but designed to work with navigable list views where a shared search
    /// bar drives both initial queries and incremental filtering. It supports both remote query 
    /// and local refinement workflows without assuming any specific platform or UI framework.
    ///
    /// Filtering is driven by attribute-decorated model properties and is internally debounced, 
    /// tracked, and stateful, exposing both query and filter readiness for external observation.
    /// </summary>

    [DebuggerDisplay("Count={Count}")]
    [PublishedContract("1.0.0")]
    public class ObservableQueryFilterSource<T>
        : ModeledMarkdownContext<T>
        , IObservableQueryFilterSource<T>
        , IList
        , IList<T>
        where T : new()
    {
        public ObservableQueryFilterSource() { }

        /// <summary>
        /// Model changes have coalesced and are updating.
        /// </summary>
        /// <remarks>
        /// The model is not a collection and does not implement INotifyCollectionChanged.
        /// </remarks>
        protected override void OnModelChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            base.OnModelChanged(eBCL);
            CollectionChanged?.Invoke(this, eBCL);
        }

        [Obsolete("Use CanonicalRecordset and PredicateMatchSubset for precise semantics.")]
        public IReadOnlyList<T> UnfilteredItems => CanonicalSuperset;

        /// <summary>
        /// "No surprises" IList Clear syntax.
        /// </summary>
        /// <remarks>
        /// Recommended:
        /// "List-like" providers of INotifyCollectionChanged should
        /// expose the expected IList.Clear(0 syntax and force an
        /// explicit boolean value to call into the MDC.
        /// </remarks>
        public void Clear() => base.Clear(true);
        public new FilteringState Clear(bool all) => base.Clear(all);

        /// <summary>
        /// Removes any current items before copying the items passed.
        /// </summary>
        /// <remarks>
        /// Mental Model: "I have a new recordset from my external (e.g., cloud) database."
        /// On completeion SearchEntryState will always be either QueryCompleteNoResults or QueryCompleteWithResults.
        /// </remarks>
        public virtual void ReplaceItems(IEnumerable<T> items)
        {
            // --------------
            // UPGRADE 260301
            base.LoadCanon(items);
            // --------------
        }

        [Probationary("Review  model should be built on a worker thread. ")]
        public virtual async Task ReplaceItemsAsync(IEnumerable<T> items)
        {
            await Task.Run(() => ReplaceItems(items));
        }

#if false
        /// <summary>
        /// Provides full control over model settling semantics to this subclass.
        /// </summary>
        [Careful("Do not expect 'MDC by Composition' to invoke this for tests.")]

        protected override void OnModelChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            if(ProjectionTopology == NetProjectionTopology.AllowDirectChanges)
            {
                base.OnModelChanged(eBCL);
            }
            else
            {
                if (eBCL is not ModelSettledEventArgs eModel)
                {
                    this.ThrowFramework<InvalidOperationException>(
                        $"Insisting on {nameof(ModelSettledEventArgs)} - The pattern match just gets the cast.");
                }
                else
                {
                    // This override can only occur for the Inheritance topology.
                    // The assert verifies the CTor didn't misclassify this instance as Composition.
                    Debug.Assert(
                        Equals(ProjectionTopology.Inheritance, ProjectionTopology),
                        "Expecting, obviously, that this *is* inheritance but making sure the property is set."
                    );

                    IList? projection = CanonicalSupersetProtected;

                    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    // Subclass has OPTED-IN to direct changes.
                    //
                    // Every change made here will 'attempt to' raise events on that
                    // object, but we expect that collection object to apply its own
                    // suppression and instead raise eBCL when the churn has finished
                    // in response to the ModelUpdated that is about to be raised.
                    //
                    // TO THAT END this operation is wrapped in an authority whereby
                    // the ONP can tell this is taking place from the back end.
                    //
                    // [Careful]
                    // Inspecting the sender of those events is *not* an effective
                    // way in in which to determine authority because *that* collection
                    // raises *those* events, i.e., is the sender of them.
                    Debug.Assert(
                        Equals(AuthorityEpochProvider.Authority, CollectionChangeAuthority.Settle),
                        "Expecting this operation takes place under Model authority."
                    );
                    // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                    switch (eBCL.Action)
                    {
                        case NotifyCollectionChangedAction.Add: localAdd(); break;
                        case NotifyCollectionChangedAction.Move: localMove(); break;
                        case NotifyCollectionChangedAction.Remove: localRemove(); break;
                        case NotifyCollectionChangedAction.Replace: localReplace(); break;
                        case NotifyCollectionChangedAction.Reset: localReset(); break;
                        default:
                            this.ThrowFramework<NotSupportedException>($"The {eBCL.Action.ToFullKey()} case is not supported.");
                            break;
                    }

                    // Raising our own event (instead of calling base to do it indirectly)
                    // is the point of NetProjectionTopology.ObservableOnly mode.
                    OnCollectionChanged(eBCL);

                    #region L o c a l F x

                    void localAdd()
                    {
                        if (eBCL.NewItems is not null)
                        {
                            var index =
                                eBCL.NewStartingIndex == -1
                                ? projection.Count
                                : eBCL.NewStartingIndex;
                            foreach (var item in eBCL.NewItems)
                            {
                                projection.Insert(index++, item);
                            }
                        }
                    }

                    void localMove()
                    {
                        if (eBCL.NewItems is null)
                        {
                            ThrowHard<NullReferenceException>($"{nameof(eBCL.NewItems)} cannot be null.");
                        }
                        else
                        {
                            if (eBCL.NewItems.Count != 1)
                            {
                                ThrowHard<NotSupportedException>(
                                    $"In {nameof(OnModelChanged)} Multi item moves are not supported. Override this method for full control.");
                                return;
                            }
                            int oldIndex = eBCL.OldStartingIndex;
                            int newIndex = eBCL.NewStartingIndex;

                            if (oldIndex < 0 || newIndex < 0)
                            {
                                this.ThrowFramework<InvalidOperationException>(
                                    $"Expecting valid indices for {NotifyCollectionChangedAction.Move.ToFullKey()}.");
                            }

                            // Capture items first to preserve ordering for multi-item moves
                            var moved = new List<object?>();
                            foreach (var _ in eBCL.NewItems)
                            {
                                moved.Add(projection[oldIndex]);
                                projection.RemoveAt(oldIndex);
                            }

                            int insertIndex = newIndex;
                            foreach (var item in moved)
                            {
                                projection.Insert(insertIndex++, item);
                            }
                        }
                    }

                    void localRemove()
                    {
                        if (eBCL.OldItems is null)
                        {
                            ThrowHard<NullReferenceException>($"{nameof(eBCL.OldItems)} cannot be null.");
                        }
                        else
                        {
                            if (eBCL.OldStartingIndex >= 0)
                            {
                                int index = eBCL.OldStartingIndex;

                                foreach (var _ in eBCL.OldItems)
                                {
                                    projection.RemoveAt(index);
                                }
                            }
                            else
                            {
                                foreach (var item in eBCL.OldItems)
                                {
                                    projection.Remove(item);
                                }
                            }
                        }
                    }

                    void localReplace()
                    {
                        switch (eModel.Reason)
                        {
                            case NotifyCollectionChangeReason.QueryResult:
                            case NotifyCollectionChangeReason.ApplyFilter:
                            case NotifyCollectionChangeReason.RemoveFilter:
                                // Avoid Clear() here. Some observers treat Clear as a semantic reset
                                // (e.g., selection or virtualization state) rather than a sequence of
                                // removes. Replaying the individual Remove/Add operations preserves
                                // the original mutation semantics and avoids surprising state resets.
                                if (eBCL.OldItems is not null)
                                {
                                    foreach (var item in eBCL.OldItems)
                                    {
                                        projection.Remove(item);
                                    }
                                }
                                if (eBCL.NewItems is not null)
                                {
                                    foreach (var item in eBCL.NewItems)
                                    {
                                        projection.Add(item);
                                    }
                                }
                                break;
                            default:
                                // Normal BCL Replace
                                if (eBCL.OldItems is not null &&
                                    eBCL.NewItems is not null &&
                                    eBCL.OldStartingIndex >= 0)
                                {
                                    int index = eBCL.OldStartingIndex;

                                    foreach (var item in eBCL.NewItems)
                                    {
                                        projection[index++] = item;
                                    }
                                }
                                break;
                        }
#if DEBUG
                        if (RouteToFullRecordset)
                        {   /* G T K */
                        }
                        else
                        {
                            var cMe = Model.ToString();
                        }
#endif
                    }
                    void localReset()
                    {
                        projection.Clear();

                        // Typically this eBCL repesents an "emptying of the collection"
                        // but this is not a guarantee. If the event offers new items,
                        // take this opportunity to copy them.
                        if (eBCL.NewItems is not null)
                        {
                            Debug.Fail($@"IFD ADVISORY - First Time.");
                            foreach (var item in eBCL.NewItems)
                            {
                                projection.Add(item);
                            }
                        }
                    }
                    #endregion L o c a l F x
                }
            }
        }

#endif

        /// <summary>
        /// Sets Filter-only mode and initializes the dataset for filtering.
        /// Ideal for static lists (e.g., preferences, enums).
        /// When a filter is "cleared" it means the collection view returns to "all items visible".
        /// </summary>
        [PublishedContract("1.0")]
        public void InitializeFilterOnlyMode(IEnumerable<T> items)
        {
            QueryFilterConfig = QueryFilterConfig.Filter;
            LoadCanon(items);
        }

        /// <summary>
        /// Public-facing CollectionChanged event, regardless of its source.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            switch (Authority)
            {
                case CollectionChangeAuthority.Reset:
                    // Events are being supressed by this authority epoch.
                    break;
                case CollectionChangeAuthority.Settle:
                    // Raise only sanctioned events
                    if (eBCL is ModelSettledEventArgs eModel)
                    {
                        CollectionChanged?.Invoke(this, eBCL);
                    }
                    break;
                default:
                    // Allow under normal collection self-authority.
                    CollectionChanged?.Invoke(this, eBCL);
                    break;
            }
        }
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// No client data connection is assumed, but if a persistent
        /// SQLite data connection is provided it will be queried here.
        /// </summary>
        [PublishedContract("1.0", typeof(IObservableQueryFilterSource))]
        [PublishedContract("2.0", typeof(IMarkdownContext))]
        public new void Commit() => base.Commit();

        [Obsolete("Legacy unit test support only.")]
        public MarkdownContextOR MarkdownContextOR
        {
            get
            {
                var searchEntryState = SearchEntryState;
                return InputText.ParseSqlMarkdown<T>(ref searchEntryState);
            }
        }

        public string SQL => Query;

        public event EventHandler<ItemPropertyChangedEventArgs>? ItemPropertyChanged;
        private INotifyPropertyChanged[] _unsubscribeItems = new INotifyPropertyChanged[] { };

        public string Placeholder =>
                IsFiltering
                ? $"Filter {Title}"
                : $"Search {Title}";
        public string Title
        {
            get => _title;
            set
            {
                if (!Equals(_title, value))
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        string _title = "Items";


        #region R O U T E D    C O N D I T I O N A L S

        /// <summary>
        /// This is a router for whether to show the unfiltered set or the filtered one.
        /// The override allows some intelligence WRT the number of filterable items in the list.
        /// </summary>
        [Careful("This polarity was wrong, and has been fixed.")]
        public override bool RouteToFullRecordset => base.RouteToFullRecordset;


        [Careful("This polarity was wrong, and has been fixed.")]
        private IReadOnlyList<T> RoutedRecordset =>
            RouteToFullRecordset 
            ? CanonicalSuperset 
            : PredicateMatchSubset;

        public new IEnumerator<T> GetEnumerator() => RoutedRecordset.Cast<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        #endregion R O U T E D    C O N D I T I O N A L S
    }
}