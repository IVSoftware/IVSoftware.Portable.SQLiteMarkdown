using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.Events;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using Newtonsoft.Json;
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
    [JsonArray]
    public partial class ObservableQueryFilterSource<T>
        : ModeledMarkdownContext<T>
        , IObservableQueryFilterSource<T>
        , IList<T>
        where T : new()
    {
        public ObservableQueryFilterSource() { }

        [Obsolete("Use CanonicalRecordset and PredicateMatchSubset for precise semantics.")]
        public IReadOnlyList<T> UnfilteredItems => CanonicalSuperset;


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

        /// <summary>
        /// Provides full control over model settling semantics to this subclass.
        /// </summary>
        [Careful("Do not expect 'MDC by Composition' to invoke this for tests.")]

        protected override void OnModelSettled(NotifyCollectionChangedEventArgs eBCL)
        {
            base.OnModelSettled(eBCL);
#if false
            if(ProjectionOption == NetProjectionOption.AllowDirectChanges)
            {
                base.OnModelSettled(eBCL);
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
                    // This override can only occur for the Inheritance 
                    // The assert verifies the CTor didn't misclassify this instance as Composition.
                    Debug.Assert(
                        Equals(ProjectionInheritance, ProjectionTopology),
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
                        DHostAuthorityEpoch.Authority == CollectionChangeAuthority.Model,
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
                    // is the point of NetProjectionOption.ObservableOnly mode.
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
                                    $"In {nameof(OnModelSettled)} Multi item moves are not supported. Override this method for full control.");
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
                            case NotifyCollectionChangedReason.QueryResult:
                            case NotifyCollectionChangedReason.ApplyFilter:
                            case NotifyCollectionChangedReason.RemoveFilter:
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
#endif
        }

#if false
        protected override void OnModelSettled(NotifyCollectionChangedEventArgs eBCL)
        {
            base.OnModelSettled(eBCL);

            switch (ProjectionOption)
            {
                case NetProjectionOption.ObservableOnly:
                    localApplyNonDirectChanges();
                    break;
                case NetProjectionOption.AllowDirectChanges:
                    break;
                default:
                    this.ThrowFramework<NotSupportedException>($"The {ProjectionOption.ToFullKey()} case is not supported.");
                    break;
            }
            void localApplyNonDirectChanges()
            {
                if(eBCL is ModelSettledEventArgs eModel)
                {
                    switch (eModel.Reason)
                    {
                        case NotifyCollectionChangedReason.Reset:
                            CollectionChanged?.Invoke(this, eBCL);
                            break;
                        case NotifyCollectionChangedReason.QueryResult:
                            localLoadCanon();
                            break;
                        case NotifyCollectionChangedReason.RemoveFilter:
                        case NotifyCollectionChangedReason.ApplyFilter: /* G T K - N O O P */
#if DEBUG
                            // Where's the iterator pointing right now?
                            if(this.Any()) // <- specifically the iterator.
                            {
                                foreach (var item in this)
                                {

                                }
                            }
                            else
                            {

                            }
#endif
                            break;
                        default:
                            break;
                    }
                }

                #region L o c a l F x
                void localLoadCanon()
                {
                    switch (eBCL.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            break;
                    }
                    var canonicalItems =
                        Model
                        .Descendants().Select(_ => _.To<T>())
                        .OfType<T>()
                        .ToArray();
                    CanonicalSupersetProtected.Clear();
                    foreach (var item in canonicalItems)
                    {
                        CanonicalSupersetProtected.Add(item);
                    }
                    CollectionChanged?.Invoke(this, eBCL);
                }
                #endregion L o c a l F x


#if false
                IList? projection = ObservableNetProjection as IList;
                // For filtering ops, update the internal snapshot here.
                switch (eModel.Reason)
                {
                    case NotifyCollectionChangedReason.ApplyFilter:
                    case NotifyCollectionChangedReason.RemoveFilter:
                        localCommitProjectionSubset();
                        break;
                }

                if (projection is not null
                    && ProjectionOption == NetProjectionOption.AllowDirectChanges)
                {
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
                        DHostAuthorityEpoch.Authority == CollectionChangeAuthority.Model,
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
                }
                ModelSettled?.Invoke(this, eBCL);

                #region L o c a l F x
                void localCommitProjectionSubset()
                {
                    if (eBCL.OldItems is not null) foreach (var item in eBCL.OldItems)
                    {
                        PredicateMatchSubsetProtected.Remove(item);
                    }
                    if (eBCL.NewStartingIndex == -1)
                    {
                        if (eBCL.NewItems is not null) foreach (var item in eBCL.NewItems)
                        {
                            PredicateMatchSubsetProtected.Add(item);
                        }
                    }
                    else
                    {
                        if (eBCL.NewItems is not null) foreach (var item in eBCL.NewItems)
                        {
                            PredicateMatchSubsetProtected.Add(item);
                        }
                    }
                }

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
                    Debug.Fail($@"IFD ADVISORY - First Time.");
                }

                void localRemove()
                {
                    Debug.Fail($@"IFD ADVISORY - First Time.");
                }

                void localReplace()
                {
                    if (eBCL.OldItems is not null) foreach (var item in eBCL.OldItems)
                    {
                        projection.Remove(item);
                    }
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
#endif
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

        #region I L I S T

        void IList.Clear() => Clear(all: true);
        void ICollection<T>.Clear() => Clear(all: true);

        /// <summary>
        /// "No Suprises" clear on an IList.
        /// </summary>
        /// <remarks>
        /// Collections that inherit MarkdownContext *must* distinguish clear semantics.
        /// Subclass should implement both:
        /// 1. The parameterless "no surprises" Clear().
        /// 2. The UI-oriented [X] demoting clear state machine.
        /// </remarks>
        [Canonical("#{5932CB31-B914-4DE8-9457-7A668CDB7D08}")]
        public new void Clear() => base.Clear(all: true);

        /// <summary>
        /// We need this, but this implementation is probationary and might need some tweaking.
        /// </summary>
        private void OnExternalChange(object? value)
        {
            if (value is ISelectable selectable)
            {
                selectable.Selection = ItemSelection.None;
            }
            FilteringState = FilteringState;
        }

        #endregion I L I S T

        /// <summary>
        /// Public-facing CollectionChanged event, regardless of its source.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            switch (AuthorityEpoch.Authority)
            {
                case CollectionChangeAuthority.SupressAllEvents:
                    // Events are being supressed by this authority epoch.
                    break;
                case CollectionChangeAuthority.Model:
                    // Raise only sanctioned events
                    if(eBCL is ModelChangedEventArgs eModel)
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

        public string SQL => MarkdownContextOR?.ToString();

        protected override void OnInputTextChanged()
        {
            base.OnInputTextChanged();
            switch (FilteringState)
            {
                case FilteringState.Armed:
                    // Basically, this is when a backspace in Filter mode results in an
                    // empty entry text field. We want to stay in filtering mode though,
                    // but the UI visuals might change e.g. icon glyph and/or color.
                    if (FilteringStatePrev == FilteringState.Active)
                    {
                        if (ReplaceItemsEventingOptions.HasFlag(ReplaceItemsEventingOption.StructuralReplaceEvent))
                        {
                            OnCollectionChanged(
                                new ModelChangedEventArgs(
                                    reason: NotifyCollectionChangeReason.RemoveFilter,
                                    action: NotifyCollectionChangedAction.Replace,
                                    oldItems: (IList)PredicateMatchSubset,
                                    newItems: (IList)CanonicalSuperset
                                )
                            );
                        }
                        if (ReplaceItemsEventingOptions.HasFlag(ReplaceItemsEventingOption.ResetOnAnyChange))
                        {
                            OnCollectionChanged(
                                new ModelChangedEventArgs
                                (
                                    reason: NotifyCollectionChangeReason.RemoveFilter,
                                    action: NotifyCollectionChangedAction.Reset
                                )
                            );
                        }
                    }
                    break;
            }
        }

        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemPropertyChanged?.Invoke(this, new ItemPropertyChangedEventArgs(e.PropertyName, sender));
        }
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

        public T this[int index]
        {
            get => 
                RoutedRecordset[index] is T itemT
                ? itemT 
                : default!;
            set
            {
                // Eventually we'll want to add an item to a filtered list, but to do so:
                // - New item needs to be added to the clients external (maybe) database.
                // - New item needs to be added to the local FilterQueryDatabase,
                // - Finally, we need to add it to the filtered items regardless
                //   of whether it meets the current filter (otherwise you might
                //   add it and have it disappear due to the filter.
                // WE WILL NEED TO DO THIS CAREFULLY WHEN THE TIME COMES!
                throw new NotSupportedException();
            }
        }

        object IList.this[int index]
        {
            get => this[index]!;
            set
            {
                if (value is T t)
                {
                    this[index] = t;
                }
                else
                {
                    Debug.Fail("ADVISORY - Invalid cast but don't crash.");
                }
            }
        }
        #endregion R O U T E D    C O N D I T I O N A L S
    }
}