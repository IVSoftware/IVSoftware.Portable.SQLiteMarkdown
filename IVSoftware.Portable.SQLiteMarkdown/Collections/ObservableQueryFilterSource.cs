using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
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
    public partial class ObservableQueryFilterSource<T>
        : MarkdownContext<T>
        , IObservableQueryFilterSource<T>
        , IList<T>
        where T : new()
    {
        public ObservableQueryFilterSource()
        {
            base.ObservableNetProjection = this;
            base.ProjectionOption = NetProjectionOption.ObservableOnly;

#if false && USER_OLD_INPC

            _canonicalRecordset.CollectionChanged += (sender, e) =>
            {
                if (DHostAuthorityEpoch.Authority == CollectionChangeAuthority.MarkdownContext)
                {   /* G T K - N O O P */
                }
                else
                {
                    if (_canonicalRecordset.Count < 2)
                    {
                        // 260301
                        // FilteringState = FilteringState.Ineligible;
                    }
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            foreach (var inpc in e.NewItems?.OfType<INotifyPropertyChanged>() ?? [])
                            {
                                inpc.PropertyChanged += OnItemPropertyChanged;
                            }
                            if (Busy)
                            {   /* G T K */
                            }
                            else
                            {
                                if (FilteringState != FilteringState.Active)
                                {
                                    OnCollectionChanged(e);
                                }
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var inpc in e.OldItems?.OfType<INotifyPropertyChanged>() ?? [])
                            {
                                inpc.PropertyChanged -= OnItemPropertyChanged;
                            }
                            if (Busy)
                            {   /* G T K */
                            }
                            else
                            {
                                OnCollectionChanged(e);
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _predicateMatchSubset.Clear();
                            foreach (var inpc in _unsubscribeItems)
                            {
                                inpc.PropertyChanged -= OnItemPropertyChanged;
                            }
                            OnCollectionChanged(e);
                            break;
                    }
                    _unsubscribeItems = _canonicalRecordset.OfType<INotifyPropertyChanged>().ToArray();
                }
            };
#endif
        }


        [Obsolete("Use CanonicalRecordset and PredicateMatchSubset for precise semantics.")]
        public IReadOnlyList<T> UnfilteredItems => (List<T>)CanonicalSupersetProtected;

        public virtual async Task ReplaceItemsAsync(IEnumerable<T> items)
        {
            Debug.WriteLine($@"260306.a ADVISORY - {nameof(ReplaceItemsAsync)}.");
            using (DHostBusy.GetToken())
            {
                await Task.Run(() =>
                {
                    base.LoadCanon(items);
                });
            }
        }

        /// <summary>
        /// Removes any current items before copying the items passed.
        /// </summary>
        /// <remarks>
        /// Mental Model: "I have a new recordset from my external (e.g., cloud) database."
        /// On completeion SearchEntryState will always be either QueryCompleteNoResults or QueryCompleteWithResults.
        /// </remarks>
        public virtual void ReplaceItems(IEnumerable<T> items)
        {
            using (DHostBusy.GetToken())
            {
                // --------------
                // UPGRADE 260301
                base.LoadCanon(items);
                // --------------
            }
        }

#if false && SAVE
        /// <summary>
        /// Applies filtering based on incremental changes to the input text,
        /// occurring after the initial query. Operates on the in-memory SQLite store
        /// when in QueryAndFilter or Filter mode. Override to customize filter behavior.
        /// </summary>
        protected override async Task ApplyFilter()
        {
            using (DHostBusy.GetToken())
            {
                await base.ApplyFilter();
                try
                {
                    var matchesB4 = PredicateMatchSubset.Cast<T>().ToArray();
                    Debug.Assert(IsFiltering);
                    if (InputText.Length == 0)
                    {
                        // When we're filtering and go to 0 length, we show ALL the items.
                        switch (FilteringState)
                        {
                            case FilteringState.Ineligible:
                                break;
                            case FilteringState.Armed:
                                break;
                            case FilteringState.Active:
                                FilteringState = FilteringState.Armed;
                                break;
                            default:
                                throw new NotImplementedException($"Bad case: {FilteringState}");
                        }
                    }
                    else
                    {
                        switch (FilteringState)
                        {
                            case FilteringState.Ineligible:
                                break;
                            case FilteringState.Armed:
                                FilteringState = FilteringState.Active;
                                break;
                            case FilteringState.Active:
                                break;
                            default:
                                throw new NotImplementedException($"Bad case: {FilteringState}");
                        }

                        var searchEntryState = SearchEntryState;
                        var sql = ParseSqlMarkdown<T>();
                        // Must have "where" and must have at least 1 non whitespace char after it.
                        if (Regex.IsMatch(sql ?? "", @"where\s+\S", RegexOptions.IgnoreCase))
                        {

                        }
                        else
                        {
                            throw new InvalidOperationException($"Expected WHERE clause with content. Parse result was:\n{sql}");
                        }

#if false && DEBUG && SAVE
                        var context = InputText.ParseSqlMarkdown<T>(ref searchEntryState);
                        var cstring = context.ToString();
                        if (sql == cstring)
                        {
                        }
                        else
                        {
                            // Probably 'not' the same so far. Here's what we need to happen:
                            // - Using the string extension is stand-alone and always makes a new context.
                            // - Going forward, the string extension should support only expr (a.k.a. @this), QueryFilterMode, and Minimum Length
                            // - We're done with passing state into the string extension, however. If state is what you want, maintain a context.
                            // - If you want, you can pull that context off the 'out XElement' from the first string call. But honestly this is more intended to be a test feature.
                        }
#endif

                        var filteredRecords = FilterQueryDatabase.Query<T>(sql);

                        // This is 'not' the place for a reconciled sync.
                        // We would do that in the UI if at all.
                        PredicateMatchSubsetProtected.Clear();
                        foreach (var item in filteredRecords)
                        {
                            PredicateMatchSubsetProtected.Add(item);
                        }
                        // Active REGARDLESS of result because if unfiltered
                        // count < 2 we're not supposed to be here in the first place.
                        Debug.Assert(CanonicalSupersetProtected.Count >= 2, "ADVISORY - Filterable source is required.");
                        FilteringState = FilteringState.Active;

                        if(ReplaceItemsEventingOptions.HasFlag(ReplaceItemsEventingOption.StructuralReplaceEvent))
                        {
                            OnCollectionChanged(
                                new ModelSettledEventArgs
                                (
                                    reason: NotifyCollectionChangedReason.ApplyFilter,
                                    action: NotifyCollectionChangedAction.Replace,
                                    oldItems: matchesB4,
                                    newItems: PredicateMatchSubsetProtected
                                )
                            );
                        }
                        if (ReplaceItemsEventingOptions.HasFlag(ReplaceItemsEventingOption.ResetOnAnyChange))
                        {
                            OnCollectionChanged(
                                new ModelSettledEventArgs
                                (
                                    reason: NotifyCollectionChangedReason.ApplyFilter,
                                    action: NotifyCollectionChangedAction.Reset
                                )
                            );
                        }
                    }
                }
                finally
                {
                    lock (_lock)
                    {
                        this.OnAwaited();
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Provides full control over model settling semantics to this subclass.
        /// </summary>
        [Careful("Do not expect 'MDC by Composition' to invoke this for tests.")]

        protected override void OnModelSettled(NotifyCollectionChangedEventArgs eBCL)
        {
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
                        Debug.Fail($@"IFD ADVISORY - First Time.");
                    }

                    void localRemove()
                    {
                        Debug.Fail($@"IFD ADVISORY - First Time.");
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
        public int IndexOf(T item) { return CanonicalSupersetProtected.IndexOf(item); }

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
        public void Clear() => base.Clear(all: true);

        public new FilteringState Clear(bool all = false)
        {
            var fsBase = base.Clear(all);
            if (fsBase < FilteringState.Armed)
            {
                // [Careful] 
                // If we're responding to FilteringState changed to clear the
                // canonical recordset it MIGHT NOT WORK. For example, manual
                // add-remove changes to Items will bypass the input state machine. 
                CanonicalSupersetProtected.Clear();
            }
            return fsBase;
        }

        public bool Contains(T item) { return CanonicalSupersetProtected.Contains(item); }

        public void CopyTo(T[] array, int arrayIndex) { CanonicalSupersetProtected.CopyTo(array, arrayIndex); }

        bool IList.Contains(object value) { return ((IList)CanonicalSupersetProtected).Contains(value); }

        int IList.IndexOf(object value) { return ((IList)CanonicalSupersetProtected).IndexOf(value); }
        public void Insert(int index, T item)
        {
            CanonicalSupersetProtected.Insert(index, item);
            OnExternalChange(item);
        }

        public void Add(T item)
        {
            CanonicalSupersetProtected.Add(item);
            OnExternalChange(item);
        }
        public void RemoveAt(int index)
        {
            object? item;
            if (index < CanonicalSupersetProtected.Count)
            {
                item = CanonicalSupersetProtected[index];
            }
            else
            {
                item = null;
            }
            CanonicalSupersetProtected.RemoveAt(index);
            OnExternalChange(item);
        }

        int IList.Add(object item)
        {
            if (item is T itemT)
            {
                CanonicalSupersetProtected.Add(itemT);
                return CanonicalSupersetProtected.IndexOf(itemT);
            }
            if (typeof(T) == typeof(StringWrapper))
            {
                var wrapper = new StringWrapper(item?.ToString() ?? string.Empty);
                if (wrapper is T itemTT)
                {
                    CanonicalSupersetProtected.Add(itemTT);
                    return CanonicalSupersetProtected.IndexOf(itemTT);
                }
            }
            throw new ArgumentException($"Value of type {item?.GetType()} cannot be added to list of {typeof(T)}");
        }

        public bool Remove(T item)
        {
            var removed = CanonicalSupersetProtected.Remove(item);
            if (removed) OnExternalChange(item);
            return removed;
        }

        void IList.Insert(int index, object item)
        {
            CanonicalSupersetProtected.Insert(index, (T)item);
            OnExternalChange(item);
        }

        void IList.Remove(object item)
        {
            if (CanonicalSupersetProtected.Contains((T)item))
            {
                CanonicalSupersetProtected.Remove((T)item);
                OnExternalChange(item);
            }
        }

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

        void ICollection.CopyTo(Array array, int index) { ((ICollection)CanonicalSupersetProtected).CopyTo(array, index); }

        bool ICollection.IsSynchronized { get { return ((ICollection)CanonicalSupersetProtected).IsSynchronized; } }

        object ICollection.SyncRoot { get { return ((ICollection)CanonicalSupersetProtected).SyncRoot; } }

        #endregion I L I S T

        /// <summary>
        /// Public-facing CollectionChanged event, regardless of its source.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs eBCL)
        {
            switch (DHostAuthorityEpoch.Authority)
            {
                case CollectionChangeAuthority.None:
                    // Events are being supressed by this authority epoch.
                    break;
                case CollectionChangeAuthority.Model:
                    // Raise only sanctioned events
                    if(eBCL is ModelSettledEventArgs eModel)
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
        protected override void OnCommit(RecordsetRequestEventArgs e)
        {
            base.OnCommit(e);

            if (MemoryDatabase != null)
            {
                switch (SearchEntryState)
                {
                    case SearchEntryState.Cleared:
                        break;
                    case SearchEntryState.QueryEmpty:
                        break;
                    case SearchEntryState.QueryENB:
                        break;
                    case SearchEntryState.QueryEN:
                        // Nullable property, but we're not in
                        // a target framework that supports it.
                        if (MemoryDatabase != null)
                        {
                            // Please don't combine these two lines. This is for cMe purposes.
                            var recordset = MemoryDatabase.Query<T>(InputText.ParseSqlMarkdown<T>());
                            ReplaceItems(recordset);
                        }
                        break;
                    case SearchEntryState.QueryCompleteNoResults:
                        break;
                    case SearchEntryState.QueryCompleteWithResults:
                        break;
                    default:
                        break;
                }
            }
        }

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
                                new ModelSettledEventArgs(
                                    reason: NotifyCollectionChangedReason.RemoveFilter,
                                    action: NotifyCollectionChangedAction.Replace,
                                    oldItems: PredicateMatchSubsetProtected,
                                    newItems: CanonicalSupersetProtected
                                )
                            );
                        }
                        if (ReplaceItemsEventingOptions.HasFlag(ReplaceItemsEventingOption.ResetOnAnyChange))
                        {
                            OnCollectionChanged(
                                new ModelSettledEventArgs
                                (
                                    reason: NotifyCollectionChangedReason.RemoveFilter,
                                    action: NotifyCollectionChangedAction.Reset
                                )
                            );
                        }
                    }
                    break;
            }
        }

        protected override async Task OnInputTextSettled(CancelEventArgs e)
        {
            await base.OnInputTextSettled(e);
            RouteToFullRecordset = string.IsNullOrWhiteSpace(InputText);
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

        protected override void OnSearchEntryStateChanged()
        {
            base.OnSearchEntryStateChanged();
            if (SearchEntryState == SearchEntryState.Cleared)
            {
                CanonicalSupersetProtected.Clear();
            }
        }

        /// <summary>
        /// This is a router for whether to show the unfiltered set or the filtered one.
        /// The override allows some intelligence WRT the number of filterable items in the list.
        /// </summary>
        [Careful("This polarity was wrong, and has been fixed.")]
        public override bool RouteToFullRecordset
        {
            get
            {
                // The FULL RECORDSET has less than 2 items total.
                if (CanonicalSupersetProtected.Count < 2) // Filtering state ineligible. Show all items.
                {
                    return true;
                }
                else
                {
                    if (InputText.Length == 0)
                    {
                        return true;         // Show all items. Full stop.
                    }
                    else
                    {
                        // Initial filtering state (query expr unchanged) OR
                        // any subsequent change where InputText is not empty.
                        return FilteringState != FilteringState.Active;
                    }
                }
            }
            protected set
            {
                this.ThrowSoft<InvalidOperationException>(
                    $"{nameof(RouteToFullRecordset)}.Set is a NOOP in the derived class and should not be called.");
            }
        }

        [Careful("This polarity was wrong, and has been fixed.")]
        private IList RoutedRecordset =>
            RouteToFullRecordset 
            ? CanonicalSupersetProtected 
            : PredicateMatchSubsetProtected;

        public ICollection CanonicalSuperset => CanonicalSupersetProtected;
        protected readonly List<T> CanonicalSupersetProtected = new List<T>();

        public new IEnumerator<T> GetEnumerator() => RoutedRecordset.Cast<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public int Count => RoutedRecordset.Count;

        /// <summary>
        /// Required IList support
        /// </summary>
        public bool IsReadOnly => ((IList)CanonicalSupersetProtected).IsReadOnly;

        /// <summary>
        /// Required IList support
        /// </summary>
        public bool IsFixedSize => ((IList)CanonicalSupersetProtected).IsFixedSize;

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